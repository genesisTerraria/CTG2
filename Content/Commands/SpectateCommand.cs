using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class SpectateCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "spectate";
        public override string Description => "Enter spectator mode.";
        public override string Usage => "/spectate";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 0)
            {
                caller.Reply("Usage: /spectate", Color.Red);
                return;
            }

            // Send request to server to enter spectator mode
            CTG2.SendEnterSpectatorRequest(caller.Player.whoAmI);
            
            caller.Reply("Requesting to enter spectator mode!!", Color.Yellow);
        }
    }
}
