using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Projectiles
{
    public class CTG2GoldenShowerProjectile : ModProjectile
    {
        // desired hitbox size (smaller than the vanilla 32x32)
        private const int HitboxWidth = 8;
        private const int HitboxHeight = 8;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GoldenShowerFriendly;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3; // GoldenShowerFriendly has 3 animation frames
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.GoldenShowerFriendly);
            Projectile.penetrate = 1;
            AIType = ProjectileID.GoldenShowerFriendly;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Shrink the hitbox around its own center, keeping position/AI/draw untouched
            int shrinkX = (projHitbox.Width - HitboxWidth) / 2;
            int shrinkY = (projHitbox.Height - HitboxHeight) / 2;

            Rectangle smallerHitbox = new Rectangle(
                projHitbox.X + shrinkX,
                projHitbox.Y + shrinkY,
                HitboxWidth,
                HitboxHeight
            );

            return smallerHitbox.Intersects(targetHitbox);
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            width = HitboxWidth;
            height = HitboxHeight;
            return true;
        }
    }
}