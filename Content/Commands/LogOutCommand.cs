// using Terraria.ModLoader;
// using Terraria;
// using Microsoft.Xna.Framework;
// using CTG2.Content.Commands.Auth;

// namespace CTG2.Content.Commands
// {
//     public class LogoutCommand : ModCommand
//     {
//         public override CommandType Type => CommandType.Chat;
//         public override string Command => "logout";
//         public override string Description => "Log out of admin.";

//         public override void Action(CommandCaller caller, string input, string[] args)
//         {
//             var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();

//             if (!modPlayer.IsAdmin)
//             {
//                 caller.Reply("You must be an admin to use this command.", Color.Red);
//                 return;
//             }

//             modPlayer.IsAdmin = false;

//             caller.Reply("You have been logged out.", Color.Green);
//         }
//     }
// }
