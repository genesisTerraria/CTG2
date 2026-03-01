using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class ViewMapQueueCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "viewmapqueue";
        public override string Usage => "/viewmapqueue";
        public override string Description => "Shows the current map queue.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            // Send request to server for map queue
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.RequestViewMap);
            packet.Write(caller.Player.whoAmI);
            packet.Send();
        }
    }
}