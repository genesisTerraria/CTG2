using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CTG2.Content.ClientSide;
using CTG2.Content.Classes;
namespace CTG2.Content.Functionality;

public class DropBlockSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        for (int i = 0; i < Main.maxItems; i++)
        {
            Item item = Main.item[i];

            if (item.active && item.type != ItemID.DirtBlock && item.type != ItemID.MudBlock && item.type != ItemID.Bubble)
            {
                item.TurnToAir();
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }
    }
}