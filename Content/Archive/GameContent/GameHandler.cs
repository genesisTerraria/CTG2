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

// public class GameHandler : ModSystem
// {
//     // A list to hold all created game instances. Static for easy access from commands.
//     public static HashSet<CTG2.Content.Game> ActiveGames = new HashSet<CTG2.Content.Game>();


//     public static void removeGameFromList(CTG2.Content.Game game)
//     {
//         ActiveGames.Remove(game);
//     }
//     public override void PostUpdateEverything()
//     {

//         foreach (CTG2.Content.Game game in ActiveGames.ToList())
//         {

//             if (game.match == null)
//                 continue;

//             var match = game.match;


//             if (match.classSelection && DateTime.Now >= match.classSelectionEndTime)
//             {
//                 match.EndClassSelection();

//                 Main.NewText($" Class selection has ended! The match begins now.", 255, 235, 59);
//             }
//             // Check if the match time is over (and class selection is finished)
//             else if (!match.classSelection && DateTime.Now >= match.matchEndTime)
//             {
//                 Main.NewText($" Match has ended!", 255, 235, 59);
//                 game.EndMatch();
//             }
//         }
//     }

//         public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
//         {
//             int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
//             if (index != -1)
//             {
//                 layers.Insert(index + 1, new LegacyGameInterfaceLayer(
//                     "CTG2: Match Timers",
//                     delegate
//                     {
                
//                         DrawMatchTimer();
//                         return true;
//                     },
//                     InterfaceScaleType.UI)
//                 );
//             }
//         }


//         private void DrawMatchTimer()
//         {
         
//             MyPlayer myPlayer = Main.LocalPlayer.GetModPlayer<MyPlayer>();

//             if (myPlayer.currentGame?.match == null || (myPlayer.currentState != PlayerState.ClassSelection && myPlayer.currentState != PlayerState.Active))
//             {
//                 return; 
//             }

        
//             Match match = myPlayer.currentGame.match;
//             string timeText = "";
//             Vector2 position = new Vector2(Main.screenWidth - 250, 40); 
//             Color textColor = Color.White;

//             if (match.classSelection)
//             {
//                 TimeSpan remaining = match.classSelectionEndTime - DateTime.Now;
//                 if (myPlayer.currentState == PlayerState.ClassSelection)
//                 {
//                     timeText = $"Class Selection: {remaining.Seconds}";
//                     textColor = Color.Yellow;
//                 }
//             }
//             else // Match is in progress
//             {
                
//             if (myPlayer.currentState == PlayerState.Active)
//             {
//                 /*This code is broken, i think players dont go into an active state for some reason*/
                
//                 TimeSpan remaining = match.matchEndTime - DateTime.Now;
//                 timeText = $"Time Remaining: {remaining.Minutes:D2}:{remaining.Seconds:D2}";
//                 textColor = Color.Yellow;
//             }
                
//             }

//             if (!string.IsNullOrEmpty(timeText))
//             {
//                 Utils.DrawBorderString(Main.spriteBatch, timeText, position, textColor, 1.2f);
//             }
//         }
// }

