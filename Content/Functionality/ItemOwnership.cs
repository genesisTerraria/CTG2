using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CTG2.Content
{
    public class ItemOwnership : GlobalItem
    {
        public int ownerWhoAmI = -1;
        public override bool InstancePerEntity => true;
        public override void OnSpawn(Item item, IEntitySource source)
        {
            if (source is EntitySource_OverfullInventory overfullSource)
            {
                ownerWhoAmI = overfullSource.Player.whoAmI;
            }
        }
        public override void SaveData(Item item, TagCompound tag)
        {
            tag["owner"] = ownerWhoAmI;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            ownerWhoAmI = tag.GetInt("owner");
        }

    }

    public class OwnershipPlayer : ModPlayer
    {
        public override bool OnPickup(Item item)
        {
            if (item.TryGetGlobalItem(out ItemOwnership itemOwnership))
            {
                if (itemOwnership.ownerWhoAmI != -1)
                {
                    if (itemOwnership.ownerWhoAmI != Player.whoAmI)
                    {
                        return false;
                    }
                }
            }

            return base.OnPickup(item);
        }
    }

}