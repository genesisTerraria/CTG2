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
        string replayFilePath = ReeseAPI.StopReeseRecordingAndGetFilePath("Queue ended. Server returned to pubs mode."); // stop the scrim recording and keep the replay path for upload
        AnnounceQueueWinner(); // announce the winner of the queue from NeatQueue
        StatsTracking.AnnounceScrimStats(); // announce the final stats of the game
        MatchUpload.UploadPayload(CtgApiServer.LastMatchWinner, replayFilePath); // upload the match + replay to PvPHub (must run before StopScrimTracking)
        StatsTracking.StopScrimTracking(); // stop tracking stats since a scrim no longer exists
        





    }

    private static void AnnounceQueueWinner()
    {
        string winner = CtgApiServer.LastMatchWinner ?? "none";
        Color winnerColor = winner switch
        {
            "red" => Color.IndianRed,
            "blue" => Color.CornflowerBlue,
            _ => Color.Gray
        };
        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral($"The queue has ended. The winner of the queue is: {winner}"),
            winnerColor);
    }
}
