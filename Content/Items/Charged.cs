using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent;
using CTG2.Content.Items;
using CTG2.Content.ClientSide;
using System.IO;


public class Charged : GlobalItem
{
    public bool Affected;
    private uint useDelay = 42; // Time between shots
    private uint postShotDelay = 30;
    private uint lastUsedCounter = 0;


    public override void SetDefaults(Item item)
    {
        base.SetDefaults(item);

        Affected = (item.type == ModContent.ItemType<Rancor>()); // rancor item id

        if (item.useAmmo == AmmoID.Arrow && Affected)
        {
            item.channel = true;
            item.autoReuse = true;
            item.UseSound = null;
            item.noUseGraphic = true;
        }
    }


    public override bool CanBeConsumedAsAmmo(Item ammo, Item weapon, Player player)
    {
        if (weapon.useAmmo == AmmoID.Arrow && Affected) return !(player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] >= 1);
        else return base.CanBeConsumedAsAmmo(ammo, weapon, player);
    }


    public override bool CanUseItem(Item item, Player player)
    {
        if (item.useAmmo == AmmoID.Arrow && Affected && player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] == 0)
        {
            if (Main.GameUpdateCount - lastUsedCounter >= useDelay)
			{
				lastUsedCounter = Main.GameUpdateCount;
	
				return true;
			}
			else
			{
				return false;
			}
        }
        else return base.CanUseItem(item, player);
    }


    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (item.useAmmo == AmmoID.Arrow && Affected)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] == 0)
            {
                if (player.GetModPlayer<TestPlayer>().playerAttribute)
                {
                    bool x = player.GetModPlayer<TestPlayer>().TryGetAimedVelocity(player, position, velocity, out Vector2 outvelocity);
                    if (x)
                    {
                        velocity = outvelocity;
                    }
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
                    type
                );
            }
            return false;
        }
        return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
    }


    public override bool InstancePerEntity => true;


    public override bool AppliesToEntity(Item entity, bool lateInstantiation) { return true; }


    public void PostShotUpdate() {
        if (Main.GameUpdateCount - lastUsedCounter > useDelay - postShotDelay)
            lastUsedCounter = Main.GameUpdateCount - useDelay + postShotDelay;
    }
}
