using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using System;


namespace CTG2.Content.Items {

	public class HexagonalBarrier : ModItem
	{
	    
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MagicMissile);
			Item.damage = 0;
			Item.crit = 0;
			Item.shootSpeed = 20;
			Item.useStyle = 5;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.UseSound = SoundID.Item29;
			Item.mana = 5;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.autoReuse = true;

			Item.shoot = ModContent.ProjectileType<HexagonalBarrierProjectile>();
		}

	}


	public class HexagonalBarrierProjectile : ModProjectile
	{
		public override string Texture => "CTG2/Content/Items/HexagonalBarrierProjectile";
		private bool firstFrame = true;
		private bool broken = false;
		public bool alive = true;
		public bool teamCheck = false;


		public override void SetDefaults() {
			Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
			Projectile.width = 21; // The width of your projectile
			Projectile.height = 21; // The height of your projectile
			Projectile.friendly = true; // Deals damage to enemies
			Projectile.penetrate = -1; // Infinite pierce
			Projectile.DamageType = DamageClass.Magic; // Deals magic damage
			Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
			Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic
			Projectile.timeLeft = 120;
			Projectile.netUpdate = true;
		}


		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			// Kill the projectile if the player dies or gets crowd controlled
			if (!player.active || player.dead || player.noItems || player.CCed || Vector2.Distance(Projectile.Center, player.Center) > 900f) {
				Projectile.Kill();
				return;
			}
			if (Main.myPlayer == Projectile.owner && Main.mapFullscreen) {
				Projectile.Kill();
				return;
			}
			Vector2 mountedCenter = player.MountedCenter;
			Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
			
			if (firstFrame)
				Projectile.velocity = unitVectorTowardsMouse * 10 + player.velocity;
			else
				Projectile.velocity *= 0.8f;

			firstFrame = false;

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];

				teamCheck = other.owner != Projectile.owner && Main.player[other.owner].team != Main.player[Projectile.owner].team;

				if (other.active && other.type != ModContent.ProjectileType<HexagonalBarrierProjectile>() && other.whoAmI != Projectile.whoAmI && teamCheck)
				{
					if (Projectile.Hitbox.Intersects(other.Hitbox))
					{
						broken = true;
						Projectile.Kill();
						if (other.type != ProjectileID.ThornChakram && other.type!= ProjectileID.Bananarang && other.type != ProjectileID.Flamarang)
							other.Kill();
						
						break; // Prevent multiple collisions per frame
					}
				}
			}

			Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
			Projectile.spriteDirection = Projectile.direction;

			if (Projectile.timeLeft <= 30)
			{
				// Lerp alpha from 0 (opaque) to 255 (invisible)
				float fadeProgress = 1f - (Projectile.timeLeft / 30f);
				Projectile.alpha = (int)(fadeProgress * 205f + 50f);

				// Clamp in case of rounding issues
				if (Projectile.alpha > 255)
					Projectile.alpha = 255;
			}
			else
				Projectile.alpha = 50;
		}


		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = -oldVelocity.X;

			if (Projectile.velocity.Y != oldVelocity.Y)
				Projectile.velocity.Y = -oldVelocity.Y;

			return false;
		}


		public override bool? CanDamage()
		{
			return false;
		}


		public override void Kill(int timeLeft)
		{
			if (broken)
			{
				// Play a sound (optional)
				SoundEngine.PlaySound(SoundID.Item27, Projectile.position);

				// Spawn 10 dust particles
				for (int i = 0; i < 4; i++)
				{
					Dust.NewDust(
						Projectile.position,                      // position
						Projectile.width,                         // width
						Projectile.height,                        // height
						67,                             // type (try others like DustID.Fire, DustID.GoldCoin, etc.)
						Projectile.velocity.X * 0.2f,             // X velocity
						Projectile.velocity.Y * 0.2f,             // Y velocity
						100,                                      // alpha
						default,                                  // color
						1f                                      // scale
					);
				}

				broken = false;
			}
		}
	}
}
