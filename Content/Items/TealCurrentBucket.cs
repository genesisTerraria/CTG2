using CTG2.Content.Functionality;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
    public class TealCurrentBucket : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.WaterBucket;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TealCurrentTile>());
            Item.width = 16;
            Item.height = 16;
            Item.maxStack = 99;
            Item.value = Item.buyPrice(copper: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void OnConsumeItem(Player player)
        {
            player.QuickSpawnItem(null, ItemID.EmptyBucket);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.EmptyBucket)
                .AddIngredient(ItemID.BottledWater, 3)
                .Register();
        }
    }
}
