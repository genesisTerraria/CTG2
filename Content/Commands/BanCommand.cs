using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Net;
using System;
using Terraria.ID;
using CTG2;
using CTG2.Content.Commands.Auth;
using System.Collections.Generic;

namespace CTG2.Content.Commands
{
    public class BanCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "ban";

        public override string Usage => "/ban <playerName> [\"reason\"]";

        public override string Description => "Bans a player from the server.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (!CommandInputParser.TryParseTargetAndOptionalReason(
                Main.chatText,
                Command,
                args,
                out string targetName,
                out string reason,
                out string error))
            {
                caller.Reply(error, Color.Red);
                return;
            }

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.Ordinal))
                {
                    var adminPlayer = player.GetModPlayer<AuthPlayer>();

                    if (adminPlayer.IsAdmin)
                    {
                        caller.Reply("You cannot ban another admin.", Color.Red);
                        return;
                    }

                    ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                    packet.Write((byte)MessageType.RequestBanPlayer);
                    packet.Write(targetName);
                    packet.Write(reason);
                    packet.Send();
                    caller.Reply($"Player '{player.name}' has been banned.", Color.Green);
                    return;
                }
            }

            caller.Reply($"No player named '{targetName}' was found.");
        }
    }
}
