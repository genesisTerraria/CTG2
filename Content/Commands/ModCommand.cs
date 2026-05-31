using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;
using CTG2.Content.ClientSide;

namespace CTG2.Content.Commands
{
    public class AdminMenuCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;

        public override string Command => "mod";

        public override string Usage => "/mod";

        public override string Description => "Open the admin player management menu.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var authPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!authPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            PlayerManager playerManager = caller.Player.GetModPlayer<PlayerManager>();
            playerManager.ShowModUI = !playerManager.ShowModUI;
            if (playerManager.ShowModUI)
            {
                caller.Reply("Admin menu opened.", Color.Green);
            }
            else
            {
                caller.Reply("Admin menu closed.", Color.Yellow);
            }
        }
    }
}
