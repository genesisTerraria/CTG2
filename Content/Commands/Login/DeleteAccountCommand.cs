using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;

namespace CTG2.Content.Commands
{
    public class DeleteAccountCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "deleteaccount";
        public override string Description => "Delete your account.";
        public override string Usage => "/deleteaccount <password>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 1)
            {
                caller.Reply("Usage: /deleteaccount <password>", Color.Red);
                return;
            }

            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();

            if (!modPlayer.IsLoggedIn)
            {
                caller.Reply("You must be logged in to delete your account.", Color.Red);
                return;
            }

            string password = args[0];
            string username = modPlayer.Username;

            caller.Reply("Deleting account...", Color.Yellow);

            Task.Run(async () =>
            {
                var result = await AuthAPI.DeleteAccount(username, password);

                Main.QueueMainThreadAction(() =>
                {
                    if (result.Success)
                    {
                        modPlayer.IsLoggedIn = false;
                        modPlayer.IsAdmin = false;
                        modPlayer.Username = "";
                        caller.Reply("Account deleted successfully.", Color.Green);
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