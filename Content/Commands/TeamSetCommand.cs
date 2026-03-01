using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using CTG2.Content.ClientSide;


namespace CTG2.Content.Commands
{
    public class TeamSetCommand : ModCommand
    {
        private static readonly Dictionary<int, int> playerTeamAssignments = new();

        public override CommandType Type => CommandType.Chat;
        public override string Command => "teamset";
        public override string Usage => "/teamset <playerName> <teamColor>";
        public override string Description => "Set a player to a specific team.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            var match = System.Text.RegularExpressions.Regex.Match(input, @"^/teamset\s+""(.+?)""\s+(\w+)$");
            if (!match.Success)
            {
                caller.Reply("Usage: /teamset \"<playerName>\" <teamColor>", Color.Red);
                return;
            }

            string targetName = match.Groups[1].Value.ToLower();
            string teamColor = match.Groups[2].Value.ToLower();

            Player target = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName);

            if (target == null)
            {
                caller.Reply($"Player '{targetName}' not found.", Color.Red);
                return;
            }

            int teamID = teamColor switch
            {
                "red" => 1,
                "green" => 2,
                "blue" => 3,
                "yellow" => 4,
                "pink" => 5,
                "none" => 0,
                _ => -1
            };

            if (teamID == -1)
            {
                caller.Reply($"Invalid team color '{teamColor}'. Valid: red, green, blue, yellow, pink, none.", Color.Red);
                return;
            }

            var mod1 = ModContent.GetInstance<CTG2>();
            ModPacket packet1 = mod1.GetPacket();
            packet1.Write((byte)MessageType.RequestTeamChange);
            packet1.Write(target.whoAmI);
            packet1.Write(teamID);
            packet1.Send();

            caller.Reply($"Set player '{target.name}' to the {teamColor} team.", Color.Green);
        }
    }
}
