using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Net;
using System;
using Terraria.ID;
using Microsoft.Xna.Framework;

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
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();

            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (args.Length != 1)
            {
                caller.Reply("Usage: /kick <playerName>", Color.Red);
                return;
            }

            string targetName = string.Join(" ", args);

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    var adminPlayer = player.GetModPlayer<AdminPlayer>();

                    if (player.IsAdmin)
                    {
                        caller.Reply("You cannot kick another admin.", Color.Red);
                        return;
                    }
                    
                    NetMessage.BootPlayer(player.whoAmI, NetworkText.FromLiteral("You have been kicked by an admin."));
                    caller.Reply($"Player '{player.name}' has been kicked.", Color.Green);
                    return;
                }

            }

            caller.Reply($"No player named '{targetName}' was found.", Color.Red);
        }
    }
}
