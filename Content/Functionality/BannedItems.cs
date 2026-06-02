using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using CTG2.Content;
using CTG2.Content.ServerSide;

namespace CTG2.Content.Functionality
{
    public class BannedItemGlobal : GlobalItem
    {
        private static readonly HashSet<int> BannedItemIDs = new()
        {
            ItemID.WoodenHammer,
            4908, // Dirt Bomb
            4909, // Sticky Dirt Bomb
            4824, // Wet Bomb
            4825, // Honey Bomb
            4826, // Lava Bomb
            4827, // Dry Bomb
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
            4459, // Dry Rocket
            71, // Copper coin
            72, // Silver coin
            73, // Gold coin
            74  // Platinum coin
            // Add more if needed
        };

        private static readonly HashSet<int> RngBannedItemIDs = new()
        {
            7,
            50,
            104,
            114,
            196,
            204,
            217,
            266,
            367,
            509,
            510,
            654,
            657,
            660,
            779,
            787,
            797,
            850,
            851,
            922,
            930,
            970,
            971,
            972,
            973,
            1071,
            1072,
            1100,
            1130,
            1157,
            1234,
            1244,
            1256,
            1262,
            1305,
            1309,
            1315,
            1326,
            1507,
            1543,
            1544,
            1545,
            1572,
            1802,
            2320,
            2364,
            2365,
            2366,
            2516,
            2535,
            2551,
            2584,
            2621,
            2673,
            2746,
            2749,
            2768,
            3014,
            3124,
            3249,
            3336,
            3474,
            3481,
            3487,
            3493,
            3499,
            3505,
            3511,
            3517,
            3522,
            3523,
            3524,
            3525,
            3531,
            3569,
            3571,
            3601,
            3611,
            3612,
            3620,
            3625,
            3818,
            3819,
            3820,
            3824,
            3825,
            3826,
            3829,
            3830,
            3831,
            3832,
            3833,
            3834,
            4269,
            4273,
            4281,
            4317,
            4607,
            4758,
            5005,
            5069,
            5094,
            5114,
            5119,
            5283,
            5335,
            5358,
            5359,
            5360,
            5361,
            5437
        };

        public static bool IsItemBanned(int itemType)
        {
            if (UnbreakableTiles.AllowBreaking)
                return false;

            if (BannedItemIDs.Contains(itemType))
                return true;

            if (ModContent.GetInstance<GameManager>().rngConfig && RngBannedItemIDs.Contains(itemType))
                return true;

            return false;
        }

        public static int GetNonBannedRandomItemType()
        {
            int itemType;
            int attempts = 0;
            const int maxAttempts = 1000;

            do
            {
                itemType = Main.rand.Next(1, ItemLoader.ItemCount);
                attempts++;
            }
            while (IsItemBanned(itemType) && attempts < maxAttempts);

            return itemType;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (IsItemBanned(item.type))
            {
                return false; // Blocks item use
            }

            return base.CanUseItem(item, player);
        }
    }
}
