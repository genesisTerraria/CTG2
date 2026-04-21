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
    public class MuteCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "mute";

        public override string Usage => "/mute <playerName>";

        public override string Description => "Mutes a player in the server.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            string rawInput = Main.chatText; 

            string message = "";
            if (rawInput.Length > 5) 
            {
                message = rawInput.Substring(5).Trim();
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                caller.Reply("Message cannot be empty!", Color.Red);
                return;
            }

            string[] messageParts = message.Split(' ');

            string targetName = "";

            if (messageParts[0].StartsWith("\""))
            {
                List<string> parts = new List<string>();
                bool foundClosingQuote = false;

                for (int i = 0; i < messageParts.Length; i++)
                {
                    parts.Add(messageParts[i]);

                    if (messageParts[i].EndsWith("\""))
                    {
                        foundClosingQuote = true;

                        targetName = string.Join(" ", parts).Trim('"');

                        break;
                    }
                }

                if (!foundClosingQuote)
                {
                    caller.Reply("Missing closing quote for item name.", Color.Red);
                    return;
                }
            }
            else
            {
                targetName = args[0];
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
                        caller.Reply("You cannot mute another admin.", Color.Red);
                        return;
                    }

                    ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                    packet.Write((byte)MessageType.RequestMute);
                    packet.Write(player.whoAmI); 
                    packet.Send();
                    caller.Reply($"Player '{player.name}' has been muted.", Color.Green);
                    return;
                }
            }

            caller.Reply($"No player named '{targetName}' was found.");
        }
    }
}
