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
                        modPlayer.SyncPlayer(-1, caller.Player.whoAmI, false);
                        
                        if (AuthPlayer.Admins.Contains(username))
                        {
                            modPlayer.IsAdmin = true;
                            caller.Reply("Logged in successfully. Welcome, admin.", Color.Gold);
                        }
                        else
                        {
                            caller.Reply("Logged in successfully.", Color.Green);
                        }
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