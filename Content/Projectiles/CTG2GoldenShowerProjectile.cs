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
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            const int shrinkX = 6; //change these to change fish projectile size
            const int shrinkY = 6;

            hitbox.X += shrinkX;
            hitbox.Y += shrinkY;
            hitbox.Width -= shrinkX * 2;
            hitbox.Height -= shrinkY * 2;
        }
    }
}
