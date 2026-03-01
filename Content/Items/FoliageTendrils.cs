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
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WebSlinger);
			Item.shoot = ModContent.ProjectileType<FoliageTendrilsProjectile>(); // Makes the item shoot the hook's projectile when used.
		}
	}

	public class FoliageTendrilsProjectile : ModProjectile
	{
		private static Asset<Texture2D> chainTexture;

		public override void Load() { // This is called once on mod (re)load when this piece of content is being loaded.
			chainTexture = ModContent.Request<Texture2D>("CTG2/Content/Items/FoliageTendrilsChain");
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Web);
		}

		public override bool? CanUseGrapple(Player player) {
			int hooksOut = 0;
			foreach (var projectile in Main.ActiveProjectiles) {
				if (projectile.owner == Main.myPlayer && projectile.type == Projectile.type) {
					hooksOut++;
				}
			}

			return hooksOut < 4;
		}

		// Old Web Slinger is 256, Amethyst Hook is 300
		public override float GrappleRange() {
			return 362f;
		}

		public override void NumGrappleHooks(Player player, ref int numHooks) {
			numHooks = 4; // The amount of hooks that can latch onto blocks at once
		}

		// Draws the grappling hook's chain.
		public override bool PreDrawExtras() {
			Vector2 playerCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 center = Projectile.Center;
			Vector2 directionToPlayer = playerCenter - Projectile.Center;
			float chainRotation = directionToPlayer.ToRotation() - MathHelper.PiOver2;
			float distanceToPlayer = directionToPlayer.Length();

			while (distanceToPlayer > 20f && !float.IsNaN(distanceToPlayer)) {
				directionToPlayer /= distanceToPlayer; // get unit vector
				directionToPlayer *= chainTexture.Height(); // multiply by chain link length

				center += directionToPlayer; // update draw position
				directionToPlayer = playerCenter - center; // update distance
				distanceToPlayer = directionToPlayer.Length();

				Color drawColor = Lighting.GetColor((int)center.X / 16, (int)(center.Y / 16));

				// Draw chain
				Main.EntitySpriteDraw(chainTexture.Value, center - Main.screenPosition,
					chainTexture.Value.Bounds, drawColor, chainRotation,
					chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0);
			}
			// Stop vanilla from drawing the default chain.
			return false;
		}
	}
}
