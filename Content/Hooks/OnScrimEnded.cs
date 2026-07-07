using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CTG2.ReeseIntegration;
using PvPHubIntegration;

namespace CTG2.Content.GameHooks;

public static partial class Hooks
{
    // Runs when CTG2 API receives the queue ended gamemode switch back to pubs
    // This runs AFTER the gamemode is switched back to pubs
    public static void OnScrimEnded()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        BanPhaseActive = false;
        _banTimerTicksRemaining = 0;
        EndBanTimer();

        ModContent.GetInstance<GameManager>().ResetClassBans(); // clear bans from both teams
        ModContent.GetInstance<NeatQueueTeamAssignmentSystem>().ClearAssignments(); // clear the team assignments for each player
        ReeseAPI.StopReeseRecording("Queue ended. Server returned to pubs mode."); // stop the scrim recording
        StatsTracking.AnnounceScrimStats(); // announce the final stats of the game
        StatsTracking.StopScrimTracking(); // stop tracking stats since a scrim no longer exists
        
        // pvphubIntegration.createpayload() create payload with existing stats
        // pvphub.uploadpayload() send the payload once done


        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("Queue ended. Server returned to pubs mode."),
            Color.LightGreen);


    }
}
