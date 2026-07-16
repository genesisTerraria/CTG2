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
    private uint rightClickLastUsedCounter; // separate cooldown tracker for manual-fire path

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

    public override bool AltFunctionUse(Item item, Player player)
    {
        return Affected;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source,
    Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (!(item.useAmmo == AmmoID.Arrow && Affected))
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);

        // Ignore right click. HoldItem handles it.
        if (player.altFunctionUse == 2)
            return false;

        var abils = player.GetModPlayer<Abilities>();

        int arrowType;
        if (!abils.class1UsedLuminite)
        {
            arrowType = 639; // Luminite
        }
        else
        {
            arrowType = ProjectileID.HellfireArrow;
        }

        Projectile.NewProjectile(
            source,
            position,
            velocity,
            ModContent.ProjectileType<ChargedBowProjectile>(),
            0,
            knockback,
            player.whoAmI,
            item.type,
            arrowType);

        return false;
    }

    public override void HoldItem(Item item, Player player)
    {
        if (!(item.useAmmo == AmmoID.Arrow && Affected))
            return;

        if (Main.myPlayer != player.whoAmI)
            return;

        if (!Main.mouseRight)
            return;

        if (player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] > 0)
            return;

        if (Main.GameUpdateCount - rightClickLastUsedCounter < useDelay)
            return;

        if (item.mana > 0 && !player.CheckMana(item, -1, true))
            return;

        rightClickLastUsedCounter = Main.GameUpdateCount;

        var abils = player.GetModPlayer<Abilities>();

        int arrowType;
        if (!abils.class1UsedLuminite)
        {
            arrowType = 639; // Luminite
            abils.class1UsedLuminite = true;
        }
        else
        {
            arrowType = ProjectileID.ShimmerArrow;
        }

        Vector2 velocity =
            (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * item.shootSpeed;

        Projectile.NewProjectile(
            player.GetSource_ItemUse(item),
            player.Center,
            velocity,
            ModContent.ProjectileType<ChargedBowProjectile>(),
            0,
            player.GetWeaponKnockback(item),
            player.whoAmI,
            item.type,
            arrowType);
    }

    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) { return true; }
}