using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.ServerSide;

//This class will store various timing dependant hooks
public static class Hooks
{
    public const int BanTimerTicks = 3 * 60 * 60; // 3 minutes in ticks (60 ticks per second)

    private const int TeamRed = 1;
    private const int TeamBlue = 3;

    // This hook is fired the instant every queued player is simultaneously in the world
    // It is called by NeatQueueTeamAssignmentSystem.PlayersReady
    // Future match-ready logic (maps/bans/pick timers) should be called from here
    public static void OnFullRosterJoined() //this is called on the server
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("All players in the queue have arrived."),
            Color.LightGreen);

        ModContent.GetInstance<CTG2>().Logger.Info(
            "[NeatQueue] OnFullRosterJoined fired. All queued players are currently in the world.");

        // re-apply teams once before starting to make sure someone not in queue doesn't become captain
        ModContent.GetInstance<NeatQueueTeamAssignmentSystem>().SyncTeamsToRoster();

        StartScrimsGame();

        // rollRandomMap(MapsAllowedInScrimsDict)

        // A system for storing the score of the scrim has to be made and has to
        // account for things like admins ending the round themselves

        // substitutes currently aren't accounted for the api has to be setup to
        // recieve the substitute event and then substitute has to be replace in
        // the mapping
    }

    // Kicks off a scrim round: announces captains and starts the ban phase (timer + captain ban UIs).
    // Games are best of 3, so this runs on full roster join AND again from EndGame between rounds.
    public static void StartScrimsGame()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral($"Both teams have 3 minutes to ban."),
            Color.LightGreen);

        var assignmentSystem = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();
        List<int> captainWhoAmIs = new(assignmentSystem.GetOnlineCaptainWhoAmIs());

        // If a team is missing a captain (none assigned, or theirs is offline)
        // promote a random online player from that team for this round
        // In the future make it so if there isn't a captain the team votes.
        if (!captainWhoAmIs.Any(whoAmI => Main.player[whoAmI].team == TeamRed))
            AddRandomCaptainFromTeam(TeamRed, captainWhoAmIs);
        if (!captainWhoAmIs.Any(whoAmI => Main.player[whoAmI].team == TeamBlue))
            AddRandomCaptainFromTeam(TeamBlue, captainWhoAmIs);

        List<string> captainNames = new();
        foreach (int whoAmI in captainWhoAmIs)
        {captainNames.Add(Main.player[whoAmI].name);}
        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("The captains are " + string.Join(", ", captainNames) + "."),
            Color.LightGreen); //make sure this format is good

        // Clear any bans left over from the previous round and sync the reset to all clients
        ModContent.GetInstance<GameManager>().ResetClassBans();

        StartBanTimer(); //showtheban timer to all players in the world

        // show the ban ui only to the captains
        // how do we account for captains leaving the world during ban process?
        // do we even care? thats their problem?
        foreach (int whoAmI in captainWhoAmIs)
        {
            ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.SendBanUI);
            packet.Write(true);
            packet.Send(toClient: whoAmI);
        }
    }

    // Picks a random active player on the given team and adds them to captainWhoAmIs
    // Does nothing if the team has no eligible players
    private static void AddRandomCaptainFromTeam(int teamId, List<int> captainWhoAmIs)
    {
        List<int> candidates = new();
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player.active && player.team == teamId && !captainWhoAmIs.Contains(i))
                candidates.Add(i);
        }

        if (candidates.Count == 0)
        {
            ModContent.GetInstance<CTG2>().Logger.Warn(
                $"[Scrims] No online players on team {teamId} to promote to captain.");
            return;
        }

        captainWhoAmIs.Add(candidates[Main.rand.Next(candidates.Count)]);
    }

    // True while captains are picking bans; armed by StartBanTimer, cleared when both bans are in
    public static bool BanPhaseActive { get; private set; }

    // Server-side countdown mirroring the client BanTimer display; when it hits 0,
    // any captain who hasn't banned gets a random ban so the game can't stall
    private static int _banTimerTicksRemaining;

    public static void StartBanTimer()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        BanPhaseActive = true;
        _banTimerTicksRemaining = BanTimerTicks;

        ModPacket banTimerPacket = ModContent.GetInstance<CTG2>().GetPacket();
        banTimerPacket.Write((byte)MessageType.SendBanTimer);
        banTimerPacket.Write(BanTimerTicks);
        banTimerPacket.Send();
    }

    // Called every tick by gamemanager
    public static void UpdateBanPhase()
    {
        if (Main.netMode != NetmodeID.Server || !BanPhaseActive)
            return;

        if (--_banTimerTicksRemaining > 0)
            return;

        var gameManager = ModContent.GetInstance<GameManager>();

        if (gameManager.blueTeamBannedClassID <= 0)
        {
            gameManager.blueTeamBannedClassID = PickRandomBannableClassID();
            ChatHelper.BroadcastChatMessage(
                NetworkText.FromLiteral("Red team's captain ran out of time, so their ban was chosen randomly."),
                Color.OrangeRed);
        }
        if (gameManager.redTeamBannedClassID <= 0)
        {
            gameManager.redTeamBannedClassID = PickRandomBannableClassID();
            ChatHelper.BroadcastChatMessage(
                NetworkText.FromLiteral("Blue team's captain ran out of time, so their ban was chosen randomly."),
                Color.OrangeRed);
        }

        TryCompleteBanPhase();

        if (BanPhaseActive)
        {
            BanPhaseActive = false;
            EndBanTimer();
            ModContent.GetInstance<CTG2>().Logger.Error(
                "[Scrims] Ban timer expired but no random ban could be chosen; ban phase aborted.");
        }
    }

    // picks random class if captain didn't ban
    private static int PickRandomBannableClassID()
    {
        bool rngConfig = ModContent.GetInstance<GameManager>().rngConfig;
        List<int> candidates = CTG2.config.Classes
            .Where(c => c.AbilityID >= 1 && c.AbilityID <= 19)
            .Where(c => rngConfig ? c.AbilityID == 18 : c.AbilityID != 18)
            .Select(c => c.AbilityID)
            .ToList();

        if (candidates.Count == 0)
            return 0;

        return candidates[Main.rand.Next(candidates.Count)];
    }

    public static void EndBanTimer()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        // 0 ticks tells clients to hide the ban timer
        ModPacket banTimerPacket = ModContent.GetInstance<CTG2>().GetPacket();
        banTimerPacket.Write((byte)MessageType.SendBanTimer);
        banTimerPacket.Write(0);
        banTimerPacket.Send();
    }

    // Called after each ban is recorded
    //  once both teams have a ban, end the phase and start the game
    public static void TryCompleteBanPhase()
    {
        if (Main.netMode != NetmodeID.Server || !BanPhaseActive)
            return;

        var gameManager = ModContent.GetInstance<GameManager>();
        if (gameManager.redTeamBannedClassID <= 0 || gameManager.blueTeamBannedClassID <= 0)
            return;

        BanPhaseActive = false;
        EndBanTimer();


        gameManager.BroadcastClassBans();

        // Bans are only revealed at the same time
        // redTeamBannedClassID applies to red players (it is what BLUE's captain banned)
        string redTeamsBan = GetClassName(gameManager.blueTeamBannedClassID);
        string blueTeamsBan = GetClassName(gameManager.redTeamBannedClassID);
        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral($"Both teams have banned. Red team has banned: {redTeamsBan}. Blue team has banned: {blueTeamsBan}."),
            Color.OrangeRed);

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("The game is starting!"),
            Color.LightGreen);

        gameManager.StartGame();
    }

    private static string GetClassName(int abilityID)
    {
        return CTG2.config.Classes.FirstOrDefault(c => c.AbilityID == abilityID)?.Name ?? $"class {abilityID}";
    }

   

}

