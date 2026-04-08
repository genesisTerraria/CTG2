using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;
using System.Threading.Tasks;

namespace CTG2.Content.Commands
{
    public class RegisterCommand : ModCommand
    {       
        public override CommandType Type => CommandType.Chat;
        public override string Command => "register";
        public override string Description => "Register a new account.";
        public override string Usage => "/register <username> <password>";
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 2)
            {
                caller.Reply("Usage: /register <username> <password>", Color.Red);
                return;
            }

            string username = args[0];
            string password = args[1];
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();

            caller.Reply("Registering...", Color.Yellow);

            Task.Run(async () =>
            {
                var result = await AuthAPI.Register(username, password);

                // Queue reply on main thread
                Main.QueueMainThreadAction(() =>
                {
                    if (result.Success)
                    {
                        modPlayer.IsLoggedIn = true;
                        modPlayer.Username = username;
                        caller.Player.name = username;
                        caller.Reply("Registered and logged in!", Color.Green);
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