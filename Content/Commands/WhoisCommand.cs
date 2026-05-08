using System;
using System.Collections.Generic;
using CTG2.Content.Commands.Auth;
using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class WhoisCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "whois";
        public override string Usage => "/whois <playerName>";
        public override string Description => "Shows a player's Discord username.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (!TryGetTargetName(args, out string targetName, out string error))
            {
                caller.Reply(error, Color.Red);
                return;
            }

            Player target = FindOnlinePlayer(targetName);
            if (target == null)
            {
                caller.Reply($"No player named '{targetName}' was found.", Color.Red);
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestWhois);
                packet.Write(target.whoAmI);
                packet.Send();
                return;
            }

            if (ModContent.GetInstance<NeatQueueTeamAssignmentSystem>()
                .TryGetDiscordUsername(target.whoAmI, out string discordUsername))
            {
                caller.Reply($"{target.name}'s Discord username: {discordUsername}", Color.CornflowerBlue);
            }
            else
            {
                caller.Reply($"No Discord username is registered for {target.name}.", Color.Red);
            }
        }

        private static bool TryGetTargetName(string[] args, out string targetName, out string error)
        {
            targetName = string.Empty;
            error = string.Empty;

            if (args.Length == 0)
            {
                error = "Usage: /whois <playerName>";
                return false;
            }

            if (!args[0].StartsWith("\""))
            {
                targetName = args[0];
                return true;
            }

            var parts = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                parts.Add(args[i]);

                if (args[i].EndsWith("\""))
                {
                    targetName = string.Join(" ", parts).Trim('"');
                    return true;
                }
            }

            error = "Missing closing quote for player name.";
            return false;
        }

        private static Player FindOnlinePlayer(string targetName)
        {
            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    return player;
            }

            return null;
        }
    }
}
