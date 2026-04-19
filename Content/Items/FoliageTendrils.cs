using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace CTG2.Content.Items
{
    public class FoliageTendrils : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WebSlinger);
            Item.shoot = ModContent.ProjectileType<FoliageTendrilsProjectile>();
        }

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.RotatedRelativePoint(player.MountedCenter, true);
		}
    }

    public class FoliageTendrilsProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Web;
        private static Asset<Texture2D> chainTexture;

        public override void Load()
        {
            //chainTexture = ModContent.Request<Texture2D>("CTG2/Content/Items/FoliageTendrilsChain");
            chainTexture = ModContent.Request<Texture2D>("Terraria/Images/Chain15");
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Web);
        }

        public override bool? CanUseGrapple(Player player)
        {
            int hooksOut = 0;
            foreach (var projectile in Main.ActiveProjectiles)
            {
                // Fixed: use Projectile.owner instead of Main.myPlayer
                if (projectile.owner == Projectile.owner && projectile.type == Projectile.type)
                {
                    hooksOut++;
                }
            }
            return hooksOut < 4;
        }

        public override float GrappleRange()
        {
            return 352f;
        }

        public override void NumGrappleHooks(Player player, ref int numHooks)
        {
            numHooks = 4;
        }

        public override bool PreDrawExtras()
        {
            Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
            Vector2 center = Projectile.Center;
            Vector2 directionToPlayer = playerCenter - Projectile.Center;
            float chainRotation = directionToPlayer.ToRotation() - MathHelper.PiOver2;
            float distanceToPlayer = directionToPlayer.Length();

            while (distanceToPlayer > 32f && !float.IsNaN(distanceToPlayer))
            {
                directionToPlayer /= distanceToPlayer;
                directionToPlayer *= chainTexture.Height();
                center += directionToPlayer;
                directionToPlayer = playerCenter - center;
                distanceToPlayer = directionToPlayer.Length();

                Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

                Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
                    chainTexture.Value.Bounds, drawColor, chainRotation,
                    chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}