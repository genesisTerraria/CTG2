using Terraria.ModLoader;
using CTG2.Content;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace CTG2.Commands
{
    public class BreakMode : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "breakmode";
        public override string Description => "Toggles block breaking protection";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            UnbreakableTiles.AllowBreaking = !UnbreakableTiles.AllowBreaking;

            string status = UnbreakableTiles.AllowBreaking ? "enabled" : "disabled";
            caller.Reply($"[CTG] Break mode is now {status}.");
        }
    }
}
