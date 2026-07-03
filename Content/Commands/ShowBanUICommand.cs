using CTG2.Content.ClientSide;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    // Temporary debug command for previewing the ban UI. Remove once bans are fully wired up.
    public class ShowBanUICommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "showbanui";
        public override string Usage => "/showbanui";
        public override string Description => "Shows the class ban UI (debug).";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            BanUI.ShowBanUI();
            caller.Reply("Ban UI shown.", Color.Green);
        }
    }
}
