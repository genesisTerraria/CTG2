using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria.ID;
using System.Collections;
using System.Collections.Generic;
using System;
using CTG2.Content.Commands.Auth;
using System.Threading.Tasks;

namespace CTG2.Content.Commands
{
    public class LoginCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "login";
        public override string Description => "Log in as admin.";
        public override string Usage => "/login <password>";

//         public override void Action(CommandCaller caller, string input, string[] args)
//         {
//             var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();

//             if (args.Length != 1)
//             {
//                 caller.Reply("Usage: /login <password>", Color.Red);
//                 return;
//             }

//             if (modPlayer.IsAdmin)
//             {
//                 caller.Reply("You are already logged in!", Color.Red);
//                 return;
//             }

//             string inputPassword = args[0];
//             var mod = ModContent.GetInstance<CTG2>();
//             using (var stream = mod.GetFileStream("Content/Commands/Login/password.hash"))
//             using (var reader = new StreamReader(stream))
//             {
//                 string storedHash = reader.ReadToEnd().Trim();

//                 if (storedHash == null)
//                 {
//                     caller.Reply("Password hash not found.");
//                     return;
//                 }

//                 string inputHash = PasswordHelper.HashPassword(inputPassword);

//                 if (inputHash == storedHash)
//                 {
//                     caller.Player.GetModPlayer<AdminPlayer>().IsAdmin = true;
//                     caller.Reply("You are now logged in as admin.", Color.Green);
//                 }
//                 else
//                 {
//                     caller.Reply("Wrong password.", Color.Red);
//                 }
//             }
//         }
//     }
// }

public override void Action(CommandCaller caller, string input, string[] args)
{
    if (args.Length != 2)
    {
        caller.Reply("Usage: /login <username> <password>", Color.Red);
        return;
    }

    string username = args[0];
    string password = args[1];
    var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();

    if (modPlayer.IsLoggedIn)
    {
        caller.Reply("You are already logged in!", Color.Red);
        return;
    }

    caller.Reply("Logging in...", Color.Yellow);

    Task.Run(async () =>
    {
        var result = await AuthAPI.Login(username, password);

        Main.QueueMainThreadAction(() =>
        {
            if (result.Success)
            {
                modPlayer.IsLoggedIn = true;
                modPlayer.Username = username;
                caller.Player.name = username;
                caller.Reply("Logged in successfully.", Color.Green);
            }
            else
            {
                caller.Reply(result.Message, Color.Red);
            }
        });
    });
}
    }
}