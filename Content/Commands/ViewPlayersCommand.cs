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
    public class ViewPlayersCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "viewplayers";
        public override string Description => "Shows all players currently in the game.";
        public override string Usage => "/viewplayers";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // Group players by team
            var playersByTeam = new Dictionary<int, List<Player>>();
            int totalPlayers = 0;

            foreach (Player p in Main.player)
            {
                if (p.active)
                {
                    totalPlayers++;
                    if (!playersByTeam.ContainsKey(p.team))
                    {
                        playersByTeam[p.team] = new List<Player>();
                    }
                    playersByTeam[p.team].Add(p);
                }
            }

            if (totalPlayers == 0)
            {
                caller.Reply("No players currently in the game.", Color.Gray);
                return;
            }

            StringBuilder message = new StringBuilder();
            message.AppendLine($"=== All Players ({totalPlayers} total) ===");

            // Sort teams for consistent display (No team first, then numbered teams)
            var sortedTeams = playersByTeam.Keys.OrderBy(teamId => teamId == 0 ? -1 : teamId).ToList();

            foreach (int teamId in sortedTeams)
            {
                var players = playersByTeam[teamId];
                string teamName = GetTeamName(teamId);
                Color teamColor = GetTeamColor(teamId);
                
                message.AppendLine($"\n{teamName} ({players.Count} players):");
                
                for (int i = 0; i < players.Count; i++)
                {
                    Player player = players[i];
                    string status = "";
                    
                    if (player.dead)
                        status += " (Dead)";
                    else if (player.ghost)
                        status += " (Ghost/Spectator)";
                    
              
                    message.AppendLine($"  • {player.name}{status}");
                }
            }

            caller.Reply(message.ToString(), Color.White);
        }

        private string GetTeamName(int teamId)
        {
            return teamId switch
            {
                0 => "No Team",
                1 => "Red Team",
                2 => "Green Team", 
                3 => "Blue Team",
                4 => "Yellow Team",
                5 => "Pink Team",
                _ => $"Team {teamId}"
            };
        }

        private Color GetTeamColor(int teamId)
        {
            return teamId switch
            {
                0 => Color.Gray,
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