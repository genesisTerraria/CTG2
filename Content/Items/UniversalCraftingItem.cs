using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2.Content.Tiles;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
    public class UniversalCraftingItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = Item.buyPrice(platinum: 9999);
            Item.rare = ItemRarityID.Expert;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useTime = 10;
            Item.useAnimation = 15;
            Item.consumable = false;
            Item.createTile = ModContent.TileType<UniversalCraftingTile>();
        }
    }
}