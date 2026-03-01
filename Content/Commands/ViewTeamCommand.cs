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
    public class ViewTeamCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "viewteam";
        public override string Description => "Shows all players on your current team.";
        public override string Usage => "/viewteam";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Player player = caller.Player;
            int playerTeam = player.team;
            
            if (playerTeam == 0)
            {
                caller.Reply("You are not on any team!", Color.Red);
                return;
            }

            string teamName = GetTeamName(playerTeam);
            Color teamColor = GetTeamColor(playerTeam);
            
            // Find all players on the same team
            var teammates = new List<Player>();
            foreach (Player p in Main.player)
            {
                if (p.active && p.team == playerTeam)
                {
                    teammates.Add(p);
                }
            }

            if (teammates.Count == 0)
            {
                caller.Reply($"No players found on {teamName} team.", Color.Gray);
                return;
            }

            StringBuilder message = new StringBuilder();
            message.AppendLine($"=== {teamName} Team ({teammates.Count} players) ===");
            
            for (int i = 0; i < teammates.Count; i++)
            {
                Player teammate = teammates[i];
                string status = teammate.dead ? " (Dead)" : teammate.ghost ? " (Ghost)" : "";
                message.AppendLine($"{i + 1}. {teammate.name}{status}");
            }

            caller.Reply(message.ToString(), teamColor);
        }

        private string GetTeamName(int teamId)
        {
            return teamId switch
            {
                1 => "Red",
                2 => "Green", 
                3 => "Blue",
                4 => "Yellow",
                5 => "Pink",
                _ => $"Team {teamId}"
            };
        }

        private Color GetTeamColor(int teamId)
        {
            return teamId switch
            {
                1 => Color.Red,
                2 => Color.Green,
                3 => Color.Blue,
                4 => Color.Yellow,
                5 => Color.Pink,
                _ => Color.White
            };
        }
    }
}