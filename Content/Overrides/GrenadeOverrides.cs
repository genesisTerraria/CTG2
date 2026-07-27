using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2.Content.Items;

namespace CTG2.Content.Overrides
{
    public class GrenadeOverrides : GlobalProjectile
    {
        public override bool InstancePerEntity => false;

        // GUESS, needs verification: the width/height StickyGrenade's hitbox
        // jumps to on the explosion frame vs. its resting stuck size.
        // Log projectile.width every tick in a test world to confirm the real jump.
        private const int SizeThreshold = 20;

        // Target explosion hitbox for a ~3-tile-radius blast (6-tile diameter = 96px)
        private const int DesiredSize = 96;

        // PostAI runs after vanilla's AI() has resized the hitbox for the
        // explosion frame, but before hit detection resolves that tick.
        public override void PostAI(Projectile projectile)
        {
            if (projectile.type != ProjectileID.StickyGrenade)
                return;

            if (projectile.width > SizeThreshold || projectile.height > SizeThreshold)
            {
                Vector2 center = projectile.Center;
                projectile.width = DesiredSize;
                projectile.height = DesiredSize;
                projectile.Center = center;
            }
        }

        public override void ModifyHitPlayer(Projectile projectile, Player player, ref Player.HurtModifiers modifiers)
        {
            int type = projectile.type;

            if (type != ProjectileID.BouncyGrenade
             && type != ProjectileID.StickyGrenade)
                return;

            if (player.whoAmI == projectile.owner)
            {
                modifiers.FinalDamage *= 0.5f;
            }

            if (type == ProjectileID.BouncyGrenade)
            {
                modifiers.Knockback *= 2f;
            }
        }
    }
}