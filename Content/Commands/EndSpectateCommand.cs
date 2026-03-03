using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class EndSpectateCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "endspectate";
        public override string Description => "Exit spectator mode and rejoin your team (if you have one).";
        public override string Usage => "/endspectate";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 0)
            {
                caller.Reply("Usage: /endspectate", Color.Red);
                return;
            }

            // Send request to server to exit spectator mode
            
            CTG2.SendExitSpectatorRequest(caller.Player.whoAmI);

            
            caller.Reply("Requesting to exit spectator mode lol", Color.Yellow);
        }
    }
}
