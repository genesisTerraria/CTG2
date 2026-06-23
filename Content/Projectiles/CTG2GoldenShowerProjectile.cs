using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Projectiles
{
    public class CTG2GoldenShowerProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GoldenShowerFriendly;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.GoldenShowerFriendly);
            AIType = ProjectileID.GoldenShowerFriendly;

            Projectile.width = 8;
            Projectile.height = 8;
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            const int hitboxSize = 8;

            int centerX = hitbox.X + hitbox.Width / 2;
            int centerY = hitbox.Y + hitbox.Height / 2;

            hitbox = new Rectangle(
                centerX - hitboxSize / 2,
                centerY - hitboxSize / 2,
                hitboxSize,
                hitboxSize
            );
        }
    }
}
