// using Microsoft.Xna.Framework;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace CTG2.Content
// {

//     public enum Team { Red, Blue }


//     public class Match
//     {
//         public Dictionary<Team, HashSet<Player>> teams;
//         public HashSet<Player> players;
//         public bool classSelection = false;

//         public DateTime startTime;
//         public DateTime classSelectionEndTime;
//         public DateTime matchEndTime;


//         public Match(Game game)
//         {
//             teams = new Dictionary<Team, HashSet<Player>>();
//             foreach (var kvp in game.scrimTeams)
//             {
//                 teams[kvp.Key] = new HashSet<Player>(kvp.Value);
//             }

//             var allAssignedPlayers = teams.Values.SelectMany(p => p).ToHashSet();
//             List<Player> unassignedPlayers = Main.player
//                 .Where(p => p.active && !allAssignedPlayers.Contains(p))
//                 .ToList();

//             foreach (Player player in unassignedPlayers)
//             {

//                 if (teams[Team.Red].Count <= teams[Team.Blue].Count)
//                 {
//                     teams[Team.Red].Add(player);
//                     player.team = 1; 
//                 }
//                 else
//                 {
//                     teams[Team.Blue].Add(player);
//                     player.team = 3; 
//                 }
//             }

//             players = teams.Values.SelectMany(p => p).ToHashSet();



//             startTime = DateTime.Now;
//             classSelection = true;
//             classSelectionEndTime = startTime.AddSeconds(15);
//         }




//         public void addPlayer(Player player)
//         {
//             if (player == null || !player.active || players.Contains(player))
//                 return;

//             Game game = ModContent.GetInstance<Game>();

//             if (game.playerToTeam.TryGetValue(player, out Team preassignedTeam))
//             {
//                 teams[preassignedTeam].Add(player);
//                 player.team = (preassignedTeam == Team.Red) ? 1 : 3;
//             }
//             else
//             {
//                 if (teams[Team.Red].Count <= teams[Team.Blue].Count)
//                 {
//                     teams[Team.Red].Add(player);
//                     player.team = 1;
//                 }
//                 else
//                 {
//                     teams[Team.Blue].Add(player);
//                     player.team = 3;
//                 }
//             }

//             players.Add(player);

//         }

//         public void removePlayer(Player player)
//         {
//             if (player == null || !players.Contains(player))
//                 return;

//             foreach (var teamSet in teams.Values)
//             {
//                 if (teamSet.Remove(player))
//                     break; 
//             }

//             player.team = 0;
//             players.Remove(player);
//         }

//         public void EndClassSelection()
//         {
//             classSelection = false;
//             matchEndTime = DateTime.Now.AddMinutes(15);
//             // all logic for player transition is handled in 
//         }
//     }
// }
