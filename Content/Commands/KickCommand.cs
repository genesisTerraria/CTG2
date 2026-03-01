using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Net;
using System;
using Terraria.ID;

namespace CTG2.Content.Commands
{
    public class KickCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "kick";

        public override string Usage => "/kick <playerName>";

        public override string Description => "Kicks a player from the server.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {

            if (Main.netMode != NetmodeID.Server)
            {
                caller.Reply("This command can only be run from the server console.");
                return;
            }

            if (args.Length < 1)
            {
                caller.Reply("Usage: /kick <playerName>");
                return;
            }

            string targetName = string.Join(" ", args);

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    NetMessage.BootPlayer(player.whoAmI, NetworkText.FromLiteral("You have been kicked by an admin."));
                    caller.Reply($"Player '{player.name}' has been kicked.");
                    return;
                }

            }

            caller.Reply($"No player named '{targetName}' was found.");
        }
    }
}
