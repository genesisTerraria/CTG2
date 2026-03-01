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
	

	public class AmalgamatedHand1_3 : ModItem
	{
	    
		public override void SetDefaults() {
	  		Item.CloneDefaults(ItemID.Sunfury);
			Item.damage = 36;
			Item.crit = 0;
			Item.shootSpeed = 20;

			Item.shoot = ModContent.ProjectileType<AmalgamatedHandProjectile1_3>();
		}

	}

	public class AmalgamatedHandProjectile1_3: ModProjectile
	{
		public override string Texture => "CTG2/Content/Items/AmalgamatedHandProjectile";
		private const string ChainTexturePath = "Content/Items/AmalgamatedHandProjectileChain";


		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Melee;
		}


		public override void AI()
		{
			var player = Main.player[Projectile.owner];

			// If owner player dies, remove the flail.
			if (player.dead) {
				Projectile.Kill();
				return;
			}

			// This prevents the item from being able to be used again prior to this Projectile dying
			player.itemAnimation = 10;
			player.itemTime = 10;

			// Here we turn the player and Projectile based on the relative positioning of the player and Projectile.
			int newDirection = Projectile.Center.X > player.Center.X ? 1 : -1;
			player.ChangeDir(newDirection);
			Projectile.direction = newDirection;

			var vectorToPlayer = player.MountedCenter - Projectile.Center;
			float currentChainLength = vectorToPlayer.Length();

			// Here is what various ai[] values mean in this AI code:
			// ai[0] == 0: Just spawned/being thrown out
			// ai[0] == 1: Flail has hit a tile or has reached maxChainLength, and is now in the swinging mode
			// ai[1] == 1 or !Projectile.tileCollide: Projectile is being forced to retract

			// ai[0] == 0 means the Projectile has neither hit any tiles yet or reached maxChainLength
			if (Projectile.ai[0] == 0f) {
				// This is how far the chain would go measured in pixels
				float maxChainLength = 250f;
				Projectile.tileCollide = true;

				if (currentChainLength > maxChainLength) {
					// If we reach maxChainLength, we change behavior.
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
				}
				else if (!player.channel) {
					// Once player lets go of the use button, let gravity take over and let air friction slow down the Projectile
					if (Projectile.velocity.Y < 0f)
						Projectile.velocity.Y *= 0.9f;

					Projectile.velocity.Y += 1f;
					Projectile.velocity.X *= 0.9f;
				}
			}
			else if (Projectile.ai[0] == 1f) {
				// When ai[0] == 1f, the Projectile has either hit a tile or has reached maxChainLength, so now we retract the Projectile
				float elasticFactorA = 25f / player.GetAttackSpeed(DamageClass.Melee);
				float elasticFactorB = 1.4f / player.GetAttackSpeed(DamageClass.Melee);
				float maxStretchLength = 300f; // This is the furthest the flail can stretch before being forced to retract. Make sure that this is a bit less than maxChainLength so you don't accidentally reach maxStretchLength on the initial throw.

				if (Projectile.ai[1] == 1f)
					Projectile.tileCollide = false;

				// If the user lets go of the use button, or if the Projectile is stuck behind some tiles as the player moves away, the Projectile goes into a mode where it is forced to retract and no longer collides with tiles.
				if (!player.channel || currentChainLength > maxStretchLength || !Projectile.tileCollide) {
					Projectile.ai[1] = 1f;

					if (Projectile.tileCollide)
						Projectile.netUpdate = true;

					Projectile.tileCollide = false;

					if (currentChainLength < 20f)
						Projectile.Kill();
				}

				if (!Projectile.tileCollide)
					elasticFactorB *= 2f;

				int restingChainLength = 60;

				// If there is tension in the chain, or if the Projectile is being forced to retract, give the Projectile some velocity towards the player
				if (currentChainLength > restingChainLength || !Projectile.tileCollide) {
					var elasticAcceleration = vectorToPlayer * elasticFactorA / currentChainLength - Projectile.velocity;
					elasticAcceleration *= elasticFactorB / elasticAcceleration.Length();
					Projectile.velocity *= 0.98f;
					Projectile.velocity += elasticAcceleration;
				}
				else {
					// Otherwise, friction and gravity allow the Projectile to rest.
					if (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y) < 6f) {
						Projectile.velocity.X *= 0.96f;
						Projectile.velocity.Y += 0.2f;
					}
					if (player.velocity.X == 0f)
						Projectile.velocity.X *= 0.96f;
				}
			}

			// Here we set the rotation based off of the direction to the player tweaked by the velocity, giving it a little spin as the flail turns around each swing 
			Projectile.rotation = vectorToPlayer.ToRotation() - Projectile.velocity.X * 0.1f;

			// Here is where a flail like Flower Pow could spawn additional Projectiles or other custom behaviors
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// This custom OnTileCollide code makes the Projectile bounce off tiles at 1/5th the original speed, and plays sound and spawns dust if the Projectile was going fast enough.
			bool shouldMakeSound = false;

			if (oldVelocity.X != Projectile.velocity.X) {
				if (Math.Abs(oldVelocity.X) > 4f) {
					shouldMakeSound = true;
				}

				Projectile.position.X += Projectile.velocity.X;
				Projectile.velocity.X = -oldVelocity.X * 0.2f;
			}

			if (oldVelocity.Y != Projectile.velocity.Y) {
				if (Math.Abs(oldVelocity.Y) > 4f) {
					shouldMakeSound = true;
				}

				Projectile.position.Y += Projectile.velocity.Y;
				Projectile.velocity.Y = -oldVelocity.Y * 0.2f;
			}

			// ai[0] == 1 is used in AI to represent that the Projectile has hit a tile since spawning
			Projectile.ai[0] = 1f;

			if (shouldMakeSound) {
				// if we should play the sound..
				Projectile.netUpdate = true;
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				// Play the sound
				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			//Draw flail chain
			var player = Main.player[Projectile.owner];

			Vector2 mountedCenter = player.MountedCenter;
			Texture2D chainTexture = ModContent.Request<Texture2D>("CTG2/Content/Items/AmalgamatedHandProjectileChain").Value;

			var drawPosition = Projectile.Center;
			var remainingVectorToPlayer = mountedCenter - drawPosition;

			float rotation = remainingVectorToPlayer.ToRotation() - MathHelper.PiOver2;

			if (Projectile.alpha == 0) {
				int direction = -1;

				if (Projectile.Center.X < mountedCenter.X)
					direction = 1;

				player.itemRotation = (float)Math.Atan2(remainingVectorToPlayer.Y * direction, remainingVectorToPlayer.X * direction);
			}

			// This while loop draws the chain texture from the Projectile to the player, looping to draw the chain texture along the path
			while (true) {
				float length = remainingVectorToPlayer.Length();

				// Once the remaining length is small enough, we terminate the loop
				if (length < 16f || float.IsNaN(length))
					break;

				// drawPosition is advanced along the vector back to the player by 16 pixels
				// 16 comes from the height of ExampleFlailProjectileChain.png and the spacing that we desired between links
				remainingVectorToPlayer = mountedCenter - drawPosition;

				// Finally, we draw the texture at the coordinates using the lighting information of the tile coordinates of the chain section
				Color color = Lighting.GetColor((int)drawPosition.X / 16, (int)(drawPosition.Y / 16f));
				Main.EntitySpriteDraw(chainTexture, drawPosition - Main.screenPosition, null, color, rotation, chainTexture.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
				drawPosition += remainingVectorToPlayer * 16 / length;
			}

			// Get the texture
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

			// Calculate the position to draw at
			Vector2 drawOrigin = new Vector2(texture.Width / 2f - 2, texture.Height / 2f);
			Vector2 flailDrawPosition = Projectile.Center - Main.screenPosition;

			// Draw the projectile manually
			Main.EntitySpriteDraw(
				texture,
				flailDrawPosition,
				null,
				lightColor,
				Projectile.rotation,
				drawOrigin,
				Projectile.scale,
				SpriteEffects.None,
				0
			);

			return false;
		}
	}
}
