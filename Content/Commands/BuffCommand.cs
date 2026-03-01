using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Collections.Generic;


namespace CTG2.Content.Commands
{
    public class BuffCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "buff";
        public override string Description => "Apply a buff for a specific timespan";
        public override string Usage => "/buff <ID> <length>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            if (args.Length == 2 && int.TryParse(args[0], out int buffID) && int.TryParse(args[1], out int length))
            {
                Player player = caller.Player;
                var mod = ModContent.GetInstance<CTG2>();
                ModPacket packet1 = mod.GetPacket();
                packet1.Write((byte)MessageType.RequestAddBuff);
                packet1.Write(player.whoAmI);
                packet1.Write(buffID);
                packet1.Write(length * 60);
                packet1.Send();
            }
            else
                caller.Reply("Wrong usage. /buff <ID> <length>");
        }
    }
}