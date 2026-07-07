using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CTG2.ReeseIntegration;

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

        ModContent.GetInstance<GameManager>().ResetClassBans();
        ModContent.GetInstance<NeatQueueTeamAssignmentSystem>().ClearAssignments();
        ReeseAPI.StopReeseRecording("Queue ended. Server returned to pubs mode.");


        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("Queue ended. Server returned to pubs mode."),
            Color.LightGreen);


    }
}
