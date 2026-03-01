// using Terraria;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using System.Collections.Generic;

// namespace CTG2.Content
// {
//     public enum PlayerState
//     {
//         Lobby,          // Default state when not in a game.
//         Spectator,      // In a game, but cannot interact.
//         ClassSelection, // In a game, choosing a class.
//         Active          // In a game, actively playing.
//     }

//     public class MyPlayer : ModPlayer
//     {
//         public PlayerState currentState = PlayerState.Lobby;

//         public Game currentGame = null;

//         private int classSelectionTimer = 0;
        
        
//         // --- State Transition Methods ---
//         public void EnterLobbyState()
//         {
//             currentState = PlayerState.Lobby;
//             currentGame = null;

//             // Restore normal player properties
//             Player.ghost = false;
//             Player.respawnTimer = 0;
//             // load saved inventory
//             Player.SpawnX = Main.spawnTileX;
//             Player.SpawnY = Main.spawnTileY;
//         }

//         public void EnterSpectatorState(Game gameInstance)
//         {
//             currentState = PlayerState.Spectator;
//             currentGame = gameInstance;
            
            
//             Player.ghost = true;
//             Player.respawnTimer = 9999; 

//             // maybe if admin a bit differently
//             Player.SpawnX = (int)(gameInstance.arenaCoords.X / 16);
//             Player.SpawnY = (int)(gameInstance.arenaCoords.Y / 16);
//         }

//         public void EnterClassSelectionState()
//         {
//             if (currentState == PlayerState.Spectator || currentState == PlayerState.Active)
//                 return;
                
//             currentState = PlayerState.ClassSelection;
//             classSelectionTimer = 15 * 60; 

            
//             Player.ghost = false;
//             Player.statLife = Player.statLifeMax; 
//             // do class selection here
//             for (int i = 0; i < 58; i++)
//             {
//                 Player.inventory[i].SetDefaults(0);
//             }
//         }

//         public void EnterActiveState()
//         {

//             if (currentState != PlayerState.ClassSelection)
//                 return;
                
//             currentState = PlayerState.Active;
//             Main.NewText($"{Player.name} in active mode");
//             classSelectionTimer = 0;

//             Player.ghost = false;

//         }

//         // --- tModLoader Hooks ---

//         public override void OnEnterWorld()
//         {
//             EnterLobbyState();
//         }

//         public override void PreUpdate()
//         {
          
//             switch (currentState)
//             {
//                 case PlayerState.Lobby:
             
//                     break;

//                 case PlayerState.Spectator:
//                     Player.ghost = true;
//                     Player.respawnTimer = 9999;
//                     break;

//                 case PlayerState.ClassSelection:
//                     if (classSelectionTimer > 0)
//                     {
//                         classSelectionTimer--;
//                     }
//                     else
//                     {
//                         Main.NewText($"{Player.name}'s time is up! Assigning a random class.");

//                         // class selelction random
//                         EnterActiveState();
//                     }
//                     break;

//                 case PlayerState.Active:
//                     // zone checking and active player logic goes here
//                     // TODO: Write game display here (timer, gem status)
//                     break;
//             }
//         }

//         // When a player disconnects, this hook can clean up their data.
//         public override void PlayerDisconnect()
//         {
//             if (Player.GetModPlayer<AdminPlayer>().IsAdmin)
//             {
//                 if (Player.GetModPlayer<AdminPlayer>().game != null)
//                 {
//                     Player.GetModPlayer<AdminPlayer>().game.endGame();
//                     Player.GetModPlayer<AdminPlayer>().game = null;
//                 }
//             }

//             if (currentGame != null)
//             {
//                 currentGame.match?.removePlayer(Player);
//                 currentGame.removePlayer(Player);
//             }
//         }
//     }
// }