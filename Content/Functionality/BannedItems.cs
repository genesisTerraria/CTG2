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
