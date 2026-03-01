// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using System;
// using System.Linq;
// using System.Collections.Generic;

// namespace CTG2.Content.Commands
// {
//     public class TeamSet : ModCommand
//     {
//         public override CommandType Type => CommandType.Chat;
//         public override string Command => "newteamset";
//         public override string Usage => "/newteamset <playerName> <teamColor>";
//         public override string Description => "Set a player to a specific scrim team (Red or Blue).";

//         public override void Action(CommandCaller caller, string input, string[] args)
//         {
//             if (args.Length < 2)
//             {
//                 caller.Reply("Usage: /newteamset <playerName> <teamColor>", Color.Red);
//                 return;
//             }

//             var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
//             if (!modPlayer.IsAdmin)
//             {
//                 caller.Reply("You must be an admin to use this command.", Color.Red);
//                 return;
//             }

//             if (modPlayer.game is null)
//             {
//                 caller.Reply("You have not created the game yet", Color.Red);
//                 return;
//             }
//             Game game = modPlayer.game;

//             string targetName = args[0].ToLower();
//             string teamColor = args[1].ToLower();

//             // Find the player in the game.
//             Player target = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName);

//             if (target == null)
//             {
//                 caller.Reply($"Player '{targetName}' not found.", Color.Red);
//                 return;
//             }
//             if (!game.players.Contains(target)) {
//                 caller.Reply($"Player '{targetName}' is not in the game", Color.Red);
//                 return;
//             }




//             // Assign vanilla team ID based on color for in-game visuals.
//             int teamID = teamColor switch
//             {
//                 "red" => 1,
//                 "blue" => 3,
//                 "none" => 0,
//                 "green" => 2, // Included for completeness, but not a scrim team.
//                 "yellow" => 4,
//                 "pink" => 5,
//                 _ => -1 // Invalid color
//             };

//             if (teamID == -1)
//             {
//                 caller.Reply($"Invalid team color '{teamColor}'. Valid: red, blue, none.", Color.Red);
//                 return;
//             }
            
//             // Set the vanilla team for visuals (health bar colors, map icons).
//             target.team = teamID;

//             // Handle removing a player from teams.
//             if (teamID == 0) // "none"
//             {
//                 game.removePlayer(target);
//                 caller.Reply($"Removed player '{target.name}' from all scrim teams.", Color.Orange);
//             }
//             // Handle adding player to RED team.
//             else if (teamID == 1)
//             {
//                 game.addPlayer(target, Team.Red);
//                 caller.Reply($"Set player '{target.name}' to the Red team.", Color.Green);
//             }
//             // Handle adding player to BLUE team.
//             else if (teamID == 3)
//             {
//                 game.addPlayer(target, Team.Blue);
//                 caller.Reply($"Set player '{target.name}' to the Blue team.", Color.Green);
//             }
//             // Handle other teams that are not part of your scrim system.
//             else
//             {
//                 game.removePlayer(target); // Ensure they are not on a scrim team.
//                 caller.Reply($"Set player '{target.name}' to vanilla team '{teamColor}'. This is not a scrim team.", Color.Yellow);
//             }

//             // Sync the team change with the server and other clients.
//             if (Main.netMode == NetmodeID.Server)
//             {
//                 NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, target.whoAmI);
//             }
//         }
//     }
// }