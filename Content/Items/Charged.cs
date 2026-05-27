using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using CTG2.Content.Items;
using CTG2.Content;

public class Charged : GlobalItem
{
    public bool Affected;
    private uint useDelay = 50; // Overall maximum firerate

    public override void SetDefaults(Item item)
    {
        base.SetDefaults(item);

        Affected = item.type == ModContent.ItemType<Rancor>(); // rancor item id

        if (item.useAmmo == AmmoID.Arrow && Affected)
        {
            item.channel = true;
            item.autoReuse = true;
            item.UseSound = null;
            item.noUseGraphic = true;
        }
    }

    public override void UpdateInventory(Item item, Player player)
    {
        var manager = player.GetModPlayer<Abilities>();

        if (manager.class1QuickDraw && !manager.class1CooldownSet)
        {
            if (manager.class1LastUsedCounter > 50)
                manager.class1LastUsedCounter -= 50;
            else
                manager.class1LastUsedCounter = 0;

            manager.class1CooldownSet = true;
        }
    }

    public override bool CanUseItem(Item item, Player player)
    {
        var manager = player.GetModPlayer<Abilities>();

        if (item.useAmmo == AmmoID.Arrow && Affected && player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] == 0)
        {
            if (Main.GameUpdateCount - manager.class1LastUsedCounter >= useDelay)
            {
                manager.class1LastUsedCounter = Main.GameUpdateCount;

                return true;
            }
            else
            {
                return false;
            }
        }
        else return base.CanUseItem(item, player);
    }

    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        if (weapon.useAmmo == AmmoID.Arrow && Affected) return !(player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] >= 1);
        else return base.CanBeConsumedAsAmmo(ammo, weapon, player);
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.useAmmo == AmmoID.Arrow && Affected)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] == 0)
            {
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    ModContent.ProjectileType<ChargedBowProjectile>(),
                    0,
                    knockback,
                    player.whoAmI,
                    item.type,
                    type
                );
            }
            return false;
        }
        return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
    }

    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) { return true; }
}
