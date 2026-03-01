using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CTG2.Content.ClientSide;
using Terraria.Chat;
using Terraria.Localization;
using CTG2.Content.ServerSide;


namespace CTG2.Content.Commands
{
    public class PauseCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "pause";
        public override string Usage => "/pause";
        public override string Description => "Pauses or unpauses the game (admin only, only during active match)";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            // Only allow pause/unpause when game is active (not class selection or inactive)
            if (GameInfo.matchStage != 2)
            {
                caller.Reply("You can only pause/unpause during an active match.", Color.OrangeRed);
                return;
            }

            // Send the pause request packet to the server
            var mod = ModContent.GetInstance<CTG2>();
            var packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestTogglePause);
            packet.Send();
        }
    }
}
