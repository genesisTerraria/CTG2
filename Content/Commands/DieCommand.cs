using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class DieCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "die";
        public override string Usage => "/die";
        public override string Description => "Kill yourself.";

        private uint lastUsed = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length > 0)
            {
                caller.Reply("Usage: /die", Color.Red);
                return;
            }

            if (Main.GameUpdateCount - lastUsed < 120)
            {
                caller.Reply("Wait at least two seconds before using this command again!");
                return;
            }

            lastUsed = Main.GameUpdateCount;

            // Send request to server to kill this player
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.RequestDie);
            packet.Write(caller.Player.whoAmI);
            packet.Send();
            
            caller.Reply("Requesting death...", Color.Gray);
        }
    }
}
