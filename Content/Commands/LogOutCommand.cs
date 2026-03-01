using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class LogoutCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "logout";
        public override string Description => "Log out of admin.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            modPlayer.IsAdmin = false;
            caller.Reply("You have been logged out.", Color.Yellow);
        }
    }
}
