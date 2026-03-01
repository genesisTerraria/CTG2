using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace CTG2.Content.Commands
{
    public class ChatFlair : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "chatflair";
        public override string Description => "Set the item ID to be used for chat flair";
        public override string Usage => "/chatflair itemID";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            string itemID = args[0];

            if (!int.TryParse(itemID, out int value)){Main.NewText("Not a valid item ID"); return;}
            if (!ContentSamples.ItemsByType.ContainsKey(value)){ Main.NewText("Item ID wasn't found"); return; }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.SendNewChatFlair);
                packet.Write((string)Main.clientUUID);
                packet.Write((int)value);
                packet.Send();
            }
        }

    }
}