using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CTG2.Content.Functionality
{
    public class BannedItemGlobal : GlobalItem
    {
        private static readonly HashSet<int> BannedItemIDs = new()
        {
            ItemID.WoodenHammer,
            4909, // StickyDirtBomb
            4824, // Wet Bomb
            4825, // Honey Bomb
            4826, // Lava Bomb
            205, // Empty Bucket
            206, // Water Bucket
            207, // Lava Bucket
            1128, // Honey Bucket
            3031, // Bottomless Water Bucket
            4820, // Bottomless Lava Bucket
            5302, // Bottomless Honey Bucket
            5364, // Bottomless Shimmer Bucket
            4447, // Wet Rocket
            4448, // Lava Rocket
            4449, // Honey Rocket
            71, // Copper coin
            72, // Silver coin
            73, // Gold coin
            74  // Platinum coin
            // Add more if needed
        };

        public override bool CanUseItem(Item item, Player player)
        {
            if (BannedItemIDs.Contains(item.type))
            {

                return false; // Blocks item use
            }

            return base.CanUseItem(item, player);
        }
    }
}
