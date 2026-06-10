using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;
using System;

namespace CTG2.Content.Commands
{
    public class StopRecordingCommand : ModCommand
    {
        public override CommandType Type => CommandType.Server;
        public override string Command => "stoprecording";
        public override string Description => "Stops the current Reese recording";
        public override string Usage => "/stoprecording";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
             if (!modPlayer.IsAdmin)
             {
                 caller.Reply("You must be an admin to use this command.", Color.Red);
                 return;
             }


        string reason = "Cross-mod integration";
        if (ModLoader.TryGetMod("Reese", out Mod reese))
        {
            try
            {
                reese.Call("StopRecording", reason);
            }
            catch (Exception e)
            {
                reese.Logger.Warn("Failed to stop Reese recording via Mod.Call: " + e);
            }
        }
    

            
        }
    }
}