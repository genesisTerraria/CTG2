using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class SpectateCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "spectate";
        public override string Description => "Enables spectator mode.";
        public override string Usage => "/spectate";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 0)
            {
                caller.Reply("Usage: /spectate", Color.Red);
                return;
            }

            CTG2.SendEnterSpectatorRequest(caller.Player.whoAmI);

            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestTeamChange);
            packet.Write(caller.Player.whoAmI);
            packet.Write(0);
            packet.Send();

            caller.Reply("Entering spectator mode.", Color.Green);
        }
    }
}
