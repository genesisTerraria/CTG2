using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;

namespace CTG2.Content.Commands
{
    public class ChangePasswordCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "changepassword";
        public override string Description => "Change your account password.";
        public override string Usage => "/changepassword <oldpassword> <newpassword>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 2)
            {
                caller.Reply("Usage: /changepassword <oldpassword> <newpassword>", Color.Red);
                return;
            }

            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();

            if (!modPlayer.IsLoggedIn)
            {
                caller.Reply("You must be logged in to change your password.", Color.Red);
                return;
            }

            string oldPassword = args[0];
            string newPassword = args[1];
            string username = modPlayer.Username;

            caller.Reply("Changing password...", Color.Yellow);

            Task.Run(async () =>
            {
                var result = await AuthAPI.ChangePassword(username, oldPassword, newPassword);

                Main.QueueMainThreadAction(() =>
                {
                    if (result.Success)
                        caller.Reply("Password changed successfully.", Color.Green);
                    else
                        caller.Reply(result.Message, Color.Red);
                });
            });
        }
    }
}