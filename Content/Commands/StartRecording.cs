using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;
using System;

namespace CTG2.Content.Commands
{
    public class StartRecordingCommand : ModCommand
    {
        public override CommandType Type => CommandType.Server; 
        //Run this command serverside since reese recording runs on the server
        public override string Command => "record";
        public override string Description => "Starts a Reese recording";
        public override string Usage => "/record";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
             if (!modPlayer.IsAdmin)
             {
                 caller.Reply("You must be an admin to use this command.", Color.Red);
                 return;
             }


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
    }
}