using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID; // Added for ItemID and ItemUseStyleID
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
    public class MapSaveWand : ModItem
    {
        // Use the texture of the vanilla item with ID 5098, which is the Classy Cane.
        public override string Texture => "Terraria/Images/Item_5098";

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = false;
            Item.damage = 0;
            Item.noMelee = true;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        // This method is ESSENTIAL for right-click functionality.
        // It MUST be present and return true.
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool? UseItem(Player player)
        {
            // We only want this logic to run on the client of the player using the item.
            if (player.whoAmI == Main.myPlayer)
            {
                // Right-Click Logic (player.altFunctionUse is 2 for right-click)
                if (player.altFunctionUse == 2)
                {
                    MapSave.endPoint = new Vector2(Main.MouseWorld.X / 16, Main.MouseWorld.Y / 16);
                    Main.NewText($"Point 2 set to: {MapSave.endPoint.ToPoint()}", 255, 235, 59); // Added a yellow color
                }
                // Left-Click Logic
                else
                {
                    MapSave.startPoint = new Vector2(Main.MouseWorld.X / 16, Main.MouseWorld.Y / 16);
                    Main.NewText($"Point 1 set to: {MapSave.startPoint.ToPoint()}", 173, 216, 230); // Added a light blue color
                }
            }

            return true;
        }


    }
}
