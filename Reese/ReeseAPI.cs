
using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.Localization;

namespace CTG2.ReeseIntegration;

class ReeseAPI
{
        public static void StartReeseRecording()
    {
        if (ModLoader.TryGetMod("Reese", out Mod reese))
        {
            try
            {
                reese.Call("StartRecording");
            }
            catch (Exception e)
            {
                reese.Logger.Warn("Failed to start Reese recording via Mod.Call: " + e);
            }
        }
    }

    public static void StopReeseRecording(string reason = "Cross-mod integration")
    {
        if (ModLoader.TryGetMod("Reese", out Mod reese))
        {
            try
            {
                reese.Call("StopRecording", reason);
            ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("Reese recording stopped"),
            Color.LightGreen);
            }
            catch (Exception e)
            {
                reese.Logger.Warn("Failed to stop Reese recording via Mod.Call: " + e);
            }
        }
    }


}