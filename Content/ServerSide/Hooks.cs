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
    // This hook is fired the instant every queued player is simultaneously in the world
    // It is called by NeatQueueTeamAssignmentSystem.PlayersReady
    // Future match-ready logic (maps/bans/pick timers) should be called from here
    public static void OnFullRosterJoined()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("All players in the queue have arrived."),
            Color.LightGreen);

        ModContent.GetInstance<CTG2>().Logger.Info(
            "[NeatQueue] OnFullRosterJoined fired. All queued players are currently in the world.");

        // Example for future scrim automation:
        // teamOneBanTimer = 3 * 60 * 60 (3 minutes to ban)
        // teamTwoBanTimer = 3 * 60 * 60
        // showBanUItocaptains()
        // rollRandomMap(MapsAllowedInScrimsDict)

        // Automating scrims requires two big updates:
        // 1. The payload for the CTG2 server and the railway backend must support captains
        // 2. A system for storing the score of the scrim has to be made and has to
        //    account for things like admins ending the round themselves
    }

    //Perhaps we can check if PlayersReady = true and make a new hook that is called from
    // GameManager.EndGame(). Here we would check if PlayersReady = true. If they are then
    // A scrim is active then increase the score within the hook
}
