using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.ReeseIntegration;

internal static class ReeseTimelineEvents
{
    public static void GemPickedUp(int gemId, int playerWhoAmI)
    {
        RecordGemEvent("pickup", "picked up", gemId, playerWhoAmI);
    }

    public static void GemDropped(int gemId, int playerWhoAmI)
    {
        RecordGemEvent("drop", "dropped", gemId, playerWhoAmI);
    }

    public static void GemCaptured(int gemId, int playerWhoAmI)
    {
        RecordGemEvent("capture", "captured", gemId, playerWhoAmI);
    }

    private static void RecordGemEvent(string actionKey, string actionText, int gemId, int playerWhoAmI)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        if (!ModLoader.TryGetMod("Reese", out Mod reese))
            return;

        string playerName = GetPlayerName(playerWhoAmI);
        string gemName = GetGemName(gemId);
        int iconId = GetGemIconItemId(gemId);

        string key = $"ctg2:gem:{gemId}:{actionKey}:{playerWhoAmI}:{Main.GameUpdateCount}";
        string text = $"{playerName} has {actionText} the {gemName}!";

        try
        {
            object result = reese.Call("AddReplayTimelineEvent", key, text, "Item", iconId);

            if (result is bool ok && !ok)
                ModContent.GetInstance<global::CTG2.CTG2>().Logger.Warn($"[Reese compat] Reese rejected timeline event: {text}");
        }
        catch (Exception exception)
        {
            ModContent.GetInstance<global::CTG2.CTG2>().Logger.Warn($"[Reese compat] Failed to add timeline event \"{text}\": {exception}");
        }
    }

    private static string GetPlayerName(int playerWhoAmI)
    {
        if (playerWhoAmI < 0 || playerWhoAmI >= Main.maxPlayers)
            return "Unknown player";

        Player player = Main.player[playerWhoAmI];

        return player?.active == true && !string.IsNullOrWhiteSpace(player.name)
            ? player.name.Trim()
            : $"Player {playerWhoAmI + 1}";
    }

    private static string GetGemName(int gemId)
    {
        return gemId switch
        {
            1 => "red team's gem",
            2 => "blue team's gem",
            _ => "gem"
        };
    }

    private static int GetGemIconItemId(int gemId)
    {
        return gemId switch
        {
            1 => ItemID.Ruby,
            2 => ItemID.Sapphire,
            _ => ItemID.Diamond
        };
    }
}
