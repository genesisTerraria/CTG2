using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Net;
using System;
using Terraria.ID;
using CTG2;

namespace CTG2.Content.Commands
{
    public class BanCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "ban";

        public override string Usage => "/ban <playerName>";

        public override string Description => "Bans a player from the server.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            if (args.Length < 1)
            {
                caller.Reply("Usage: /ban <playerName>");
                return;
            }

            string targetName = string.Join(" ", args);

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                    packet.Write((byte)MessageType.RequestBanPlayer);
                    packet.Write(args[0]); 
                    packet.Send();
                    caller.Reply($"Player '{player.name}' has been banned.");
                    return;
                }

            }

            caller.Reply($"No player named '{targetName}' was found.");


        
        }
    }
}
