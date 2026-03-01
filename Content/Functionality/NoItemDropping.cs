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

if (item.velocity.Y == -2f && item.active && (GameInfo.matchStage == 1 || GameInfo.matchStage == 2)) //BlockRewardSystem.canBeDropped == false
{
    for (int p = 0; p < Main.maxPlayers; p++)
    {
        Player player = Main.player[p];
        if (player.active && player.Hitbox.Intersects(item.Hitbox))
        {
            Item returnedItem = item.Clone(); 
            item.TurnToAir();
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);


            player.GetItem(player.whoAmI, returnedItem, GetItemSettings.InventoryUIToInventorySettings);    


            if (p == Main.myPlayer)
                Main.NewText("Dropping items is disabled. Item returned.", Color.Orange);

            break; 
        }
    }
}

}
        }
    }







