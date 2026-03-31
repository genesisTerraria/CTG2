using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class InvisibilityPickup : GlobalItem
    {
        public override bool CanPickup(Item item, Player player)
        {
            if (player.HasBuff(BuffID.Invisibility) && player.itemAnimation == 0)
            {
                return false;
            }

            return base.CanPickup(item, player);
        }
    }
}