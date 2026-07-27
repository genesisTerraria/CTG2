using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
    public class ConcussiveGrenadeProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BouncyGrenade;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.StickyGrenade);
            AIType = ProjectileID.StickyGrenade;
            Projectile.timeLeft = 60;
            Projectile.ai[0] = 0;
            Projectile.ai[1] = 0;
        }
    }
}