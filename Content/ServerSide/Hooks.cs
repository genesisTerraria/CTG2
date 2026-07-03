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

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral($"Both teams have 3 minutes to ban."),
            Color.LightGreen);

        ModContent.GetInstance<CTG2>().Logger.Info(
            "[NeatQueue] OnFullRosterJoined fired. All queued players are currently in the world.");

        var assignmentSystem = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();
        var captainWhoAmIs = assignmentSystem.GetOnlineCaptainWhoAmIs();

        List<string> captainNames = new();
        foreach (int whoAmI in captainWhoAmIs)
        {captainNames.Add(Main.player[whoAmI].name);}
        // umm check if the list is empty first????
        // if there is no captain on either team then give it randomly for now
        // in the future make it so if there isn't captain the team votes
        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("The captains are " + string.Join(", ", captainNames) + "."),
            Color.LightGreen); //make sure this format is good


        // Clear any bans left over from a previous match and sync the reset to all clients
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

        // rollRandomMap(MapsAllowedInScrimsDict)

        // A system for storing the score of the scrim has to be made and has to
        // account for things like admins ending the round themselves

        // substitutes currently aren't accounted for the api has to be setup to
        // recieve the substitute event and then substitute has to be replace in
        // the mapping
    }

    // True while captains are picking bans; armed by StartBanTimer, cleared when both bans are in
    public static bool BanPhaseActive { get; private set; }

    public static void StartBanTimer()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        BanPhaseActive = true;

        ModPacket banTimerPacket = ModContent.GetInstance<CTG2>().GetPacket();
        banTimerPacket.Write((byte)MessageType.SendBanTimer);
        banTimerPacket.Write(BanTimerTicks);
        banTimerPacket.Send();
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
    // TODO: give the teams some time between rounds
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

