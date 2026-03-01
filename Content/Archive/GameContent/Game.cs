// using System;
// using Terraria;
// using Terraria.ModLoader;
// using Terraria.UI;
// using Terraria.GameContent.UI;
// using Terraria.GameContent;
// using Terraria.ID;
// using Microsoft.Xna.Framework;
// using Terraria.Localization;
// using System.Collections.Generic;
// using Terraria.Chat;
// using System.IO;
// using Microsoft.Xna.Framework.Graphics;
// using CTG2.Content;
// using System.Linq;
// using System.Security.Policy;
// using System.Numerics;
// using Vector2 = Microsoft.Xna.Framework.Vector2;

// // Ensure Team is public and accessible
// public enum Team
// {
//     Red,
//     Blue
// }

// namespace CTG2.Content
// {
//     public class Game
//     {
//         public Match match = null;
//         public ModPlayer host = null;
//         public HashSet<Player> players = new HashSet<Player>();

//         public Dictionary<Team, HashSet<Player>> scrimTeams = new Dictionary<Team, HashSet<Player>>();
//         public Dictionary<Player, Team> playerToTeam = new Dictionary<Player, Team>();
//         public Vector2 arenaCoords = new Microsoft.Xna.Framework.Vector2(40429, 12001);

//         public Vector2 blueClassSelectionSpawn = new();
//         public Vector2 redClassSelectionSpawn = new();
//         public Vector2 mapCoords = new Vector2(36283, 10835);

//         public Game()
//         {

//             scrimTeams[Team.Red] = new HashSet<Player>();
//             scrimTeams[Team.Blue] = new HashSet<Player>();
//         }

//         public void setArenaCoords(Vector2 arenaCoords)
//         {
//             this.arenaCoords = arenaCoords;
//         }

//         public void setMapCoords(Vector2 mapCoords)
//         {
//             this.mapCoords = mapCoords;
//         }


//         public void removePlayer(Player player)
//         {

//             if (playerToTeam.ContainsKey(player))
//             {
//                 Team currentTeam = playerToTeam[player];

//                 scrimTeams[currentTeam].Remove(player);

//                 playerToTeam.Remove(player);

//                 players.Remove(player);
//             }
//         }

//         public void addPlayer(Player player, Team newTeam)
//         {
//             removePlayer(player);

//             scrimTeams[newTeam].Add(player);

//             playerToTeam[player] = newTeam;

//             players.Add(player);
//         }

//         public void StartMatch()
//         {
//             match = new Match(this);
//             var allPlayersInMatch = match.players;

//             // Iterate through all players now in the match to update their state.
//             foreach (Player p in allPlayersInMatch)
//             {
//                 // Get the player's custom instance.
//                 var myPlayer = p.GetModPlayer<MyPlayer>();

//                 myPlayer.EnterClassSelectionState();

          
//                 p.Teleport(arenaCoords, 1);
//             }
//         }

//         public void EndMatch()
//         {
//             var allPlayersInMatch = match?.players ?? Enumerable.Empty<Player>();
//             foreach (Player p in allPlayersInMatch)
//             {
//                 p.team = 0;

//                 p.GetModPlayer<MyPlayer>().EnterSpectatorState(this);
//             }
//             match = null;
//         }

//         public void endGame()
//         {
//             this.EndMatch();
//             GameHandler.removeGameFromList(this);
//             var allPlayersinGame = this.players ?? Enumerable.Empty<Player>();
//             foreach (Player p in allPlayersinGame)
//             {
//                 p.team = 0;
//                 p.GetModPlayer<MyPlayer>().EnterLobbyState();
//             }
//         }


//     }
// }