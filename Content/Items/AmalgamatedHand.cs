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
using System.Collections.Generic;
using Terraria.Localization;
using System;


namespace CTG2.Content.Items {

	public class AmalgamatedHand : ModItem
	{
		public override void SetDefaults() {
	  		Item.CloneDefaults(ItemID.Sunfury);
			Item.damage = 38;
			Item.useTime = 40;
            Item.useAnimation = 40;
            Item.shootSpeed = 13.8f;
			Item.rare = ItemRarityID.Red;
		}


		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			TooltipLine tooltip = new TooltipLine(Mod, "CTG2: Amalgamated Hand", "Left-click: spin, timed drop-shots, forward thrusts\nRight-click: dragging, forward thrusts") { OverrideColor = Color.Red };
			tooltips.Add(tooltip);
		}


		public override bool AltFunctionUse(Player player)
		{
			return false; // uncomment to allow 1.3 flail
		}


		public override bool CanUseItem(Player player)
		{
			if (player.altFunctionUse == 2) //right click
			{
				Item.shootSpeed = 18;
				Item.shoot = ModContent.ProjectileType<AmalgamatedHandProjectile2>();
			}
			else //left click
			{
				Item.shootSpeed = 20;
				Item.shoot = ModContent.ProjectileType<AmalgamatedHandProjectile1>();
			}
			
			return base.CanUseItem(player);
		}
	}


	public class AmalgamatedHandProjectile1 : ModProjectile
	{
		private const string ChainTexturePath = "CTG2/Content/Items/AmalgamatedHandProjectileChain"; // The folder path to the flail chain sprite
		public override string Texture => "CTG2/Content/Items/AmalgamatedHandProjectile";

		private static Asset<Texture2D> chainTexture;

		private enum AIState
		{
			Spinning,
			LaunchingForward,
			Retracting,
			UnusedState,
			ForcedRetracting,
			Ricochet,
			Dropping
		}

		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private AIState CurrentAIState {
			get => (AIState)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}
		public ref float StateTimer => ref Projectile.ai[1];
		public ref float CollisionCounter => ref Projectile.localAI[0];
		public ref float SpinningStateTimer => ref Projectile.localAI[1];
		public float LifeTimer = 0;

		public override void Load() {
			chainTexture = ModContent.Request<Texture2D>(ChainTexturePath);
		}

		public override void SetStaticDefaults() {
			// These lines facilitate the trail drawing
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

		public override void SetDefaults() {
			Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
			Projectile.width = 16; // The width of your projectile
			Projectile.height = 16; // The height of your projectile
			Projectile.friendly = true; // Deals damage to enemies
			Projectile.penetrate = -1; // Infinite pierce
			Projectile.DamageType = DamageClass.Melee; // Deals melee damage
			Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
			Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic

			// Vanilla flails all use aiStyle 15, but the code isn't customizable so an adaption of that aiStyle is used in the AI method
		}

		// This AI code was adapted from vanilla code: Terraria.Projectile.AI_015_Flails() 
		public override void AI() {
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
			bool doFastThrowDust = false;
			bool shouldOwnerHitCheck = false;
			int launchTimeLimit = 16;  // How much time the projectile can go before retracting (speed and shootTimer will set the flail's range)
			float launchSpeed = 16f; // How fast the projectile can move
			float maxLaunchLength = 900f; // How far the projectile's chain can stretch before being forced to retract when in launched state
			float retractAcceleration = 2.5f; // How quickly the projectile will accelerate back towards the player while retracting
			float maxRetractSpeed = 16f; // The max speed the projectile will have while retracting
			float forcedRetractAcceleration = 6f; // How quickly the projectile will accelerate back towards the player while being forced to retract
			float maxForcedRetractSpeed = 16f; // The max speed the projectile will have while being forced to retract
			float unusedRetractAcceleration = 1f;
			float unusedMaxRetractSpeed = 14f;
			int unusedChainLength = 60;
			int defaultHitCooldown = 10; // How often your flail hits when resting on the ground, or retracting
			int spinHitCooldown = 20; // How often your flail hits when spinning
			int movingHitCooldown = 10; // How often your flail hits when moving
			int ricochetTimeLimit = launchTimeLimit + 5;

			// Scaling these speeds and accelerations by the players melee speed makes the weapon more responsive if the player boosts it or general weapon speed
			float meleeSpeedMultiplier = player.GetTotalAttackSpeed(DamageClass.Melee);
			launchSpeed *= meleeSpeedMultiplier;
			unusedRetractAcceleration *= meleeSpeedMultiplier;
			unusedMaxRetractSpeed *= meleeSpeedMultiplier;
			retractAcceleration *= meleeSpeedMultiplier;
			maxRetractSpeed *= meleeSpeedMultiplier;
			forcedRetractAcceleration *= meleeSpeedMultiplier;
			maxForcedRetractSpeed *= meleeSpeedMultiplier;
			float launchRange = launchSpeed * launchTimeLimit;
			float maxDroppedRange = launchRange + 160f;
			Projectile.localNPCHitCooldown = defaultHitCooldown;

			switch (CurrentAIState) {
				case AIState.Spinning: {
					shouldOwnerHitCheck = true;
					if (Projectile.owner == Main.myPlayer) {
						Vector2 unitVectorTowardsMouse = mountedCenter.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.UnitX * player.direction);
						player.ChangeDir((unitVectorTowardsMouse.X > 0f).ToDirectionInt());
						if (!player.channel) // If the player releases then change to moving forward mode
						{
							CurrentAIState = AIState.LaunchingForward;
							StateTimer = 0f;
							Projectile.velocity = unitVectorTowardsMouse * launchSpeed + player.velocity;
							Projectile.Center = mountedCenter;
							Projectile.netUpdate = true;
							Projectile.ResetLocalNPCHitImmunity();
							Projectile.localNPCHitCooldown = movingHitCooldown;
							break;
						}
					}
					SpinningStateTimer += 1f;
					// This line creates a unit vector that is constantly rotated around the player. 10f controls how fast the projectile visually spins around the player
					Vector2 offsetFromPlayer = new Vector2(player.direction).RotatedBy((float)Math.PI * 10f * (SpinningStateTimer / 60f) * player.direction);

					offsetFromPlayer.Y *= 0.8f;
					if (offsetFromPlayer.Y * player.gravDir > 0f) {
						offsetFromPlayer.Y *= 0.5f;
					}
					Projectile.Center = mountedCenter + offsetFromPlayer * 30f + new Vector2(0, player.gfxOffY);
					Projectile.velocity = Vector2.Zero;
					Projectile.localNPCHitCooldown = spinHitCooldown; // set the hit speed to the spinning hit speed
					break;
				}
				case AIState.LaunchingForward: {
						doFastThrowDust = true;
						bool shouldSwitchToRetracting = StateTimer++ >= launchTimeLimit;
						shouldSwitchToRetracting |= Projectile.Distance(mountedCenter) >= maxLaunchLength;
						if (player.controlUseItem) // If the player clicks, transition to the Dropping state
						{
							CurrentAIState = AIState.Dropping;
							StateTimer = 0f;
							Projectile.netUpdate = true;
							Projectile.velocity *= 0.2f;
							// This is where Drippler Crippler spawns its projectile
							/*
							if (Main.myPlayer == Projectile.owner)
								Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Projectile.velocity, 928, Projectile.damage, Projectile.knockBack, Main.myPlayer);
							*/
							break;
						}
						if (shouldSwitchToRetracting) {
							CurrentAIState = AIState.Retracting;
							StateTimer = 0f;
							Projectile.netUpdate = true;
							Projectile.velocity *= 0.3f;
							// This is also where Drippler Crippler spawns its projectile, see above code.
						}
						player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
						Projectile.localNPCHitCooldown = movingHitCooldown;
						break;
					}
				case AIState.Retracting: {
						Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
						if (Projectile.Distance(mountedCenter) <= maxRetractSpeed) {
							Projectile.Kill(); // Kill the projectile once it is close enough to the player
							return;
						}
						if (player.controlUseItem) // If the player clicks, transition to the Dropping state
						{
							CurrentAIState = AIState.Dropping;
							StateTimer = 0f;
							Projectile.netUpdate = true;
							Projectile.velocity *= 0.2f;
						}
						else {
							Projectile.velocity *= 0.98f;
							Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxRetractSpeed, retractAcceleration);
							player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
						}
						break;
					}
				// Projectile.ai[0] == 3; This case is actually unused, but maybe a Terraria update will add it back in, or maybe it is useless, so I left it here.
				case AIState.UnusedState: {
						if (!player.controlUseItem) {
							CurrentAIState = AIState.ForcedRetracting; // Move to super retracting mode if the player taps
							StateTimer = 0f;
							Projectile.netUpdate = true;
							break;
						}
						float currentChainLength = Projectile.Distance(mountedCenter);
						Projectile.tileCollide = StateTimer == 1f;
						bool flag3 = currentChainLength <= launchRange;
						if (flag3 != Projectile.tileCollide) {
							Projectile.tileCollide = flag3;
							StateTimer = Projectile.tileCollide ? 1 : 0;
							Projectile.netUpdate = true;
						}
						if (currentChainLength > unusedChainLength) {

							if (currentChainLength >= launchRange) {
								Projectile.velocity *= 0.5f;
								Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * unusedMaxRetractSpeed, unusedMaxRetractSpeed);
							}
							Projectile.velocity *= 0.98f;
							Projectile.velocity = Projectile.velocity.MoveTowards(Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero) * unusedMaxRetractSpeed, unusedRetractAcceleration);
						}
						else {
							if (Projectile.velocity.Length() < 6f) {
								Projectile.velocity.X *= 0.96f;
								Projectile.velocity.Y += 0.2f;
							}
							if (player.velocity.X == 0f) {
								Projectile.velocity.X *= 0.96f;
							}
						}
						player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
						break;
					}
				case AIState.ForcedRetracting: {
						Projectile.tileCollide = false;
						Vector2 unitVectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
						if (Projectile.Distance(mountedCenter) <= maxForcedRetractSpeed) {
							Projectile.Kill(); // Kill the projectile once it is close enough to the player
							return;
						}
						Projectile.velocity *= 0.98f;
						Projectile.velocity = Projectile.velocity.MoveTowards(unitVectorTowardsPlayer * maxForcedRetractSpeed, forcedRetractAcceleration);
						Vector2 target = Projectile.Center + Projectile.velocity;
						Vector2 value = mountedCenter.DirectionFrom(target).SafeNormalize(Vector2.Zero);
						if (Vector2.Dot(unitVectorTowardsPlayer, value) < 0f) {
							Projectile.Kill(); // Kill projectile if it will pass the player
							return;
						}
						player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
						break;
					}
				case AIState.Ricochet:
					if (StateTimer++ >= ricochetTimeLimit) {
						CurrentAIState = AIState.Dropping;
						StateTimer = 0f;
						Projectile.netUpdate = true;
					}
					else {
						Projectile.localNPCHitCooldown = movingHitCooldown;
						Projectile.velocity.Y += 0.6f;
						Projectile.velocity.X *= 0.95f;
						player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
					}
					break;
				case AIState.Dropping:
					if (!player.controlUseItem || Projectile.Distance(mountedCenter) > maxDroppedRange) {
						CurrentAIState = AIState.ForcedRetracting;
						StateTimer = 0f;
						Projectile.netUpdate = true;
					}
					else {
						Projectile.velocity.Y += 0.8f;
						Projectile.velocity.X *= 0.95f;
						player.ChangeDir((player.Center.X < Projectile.Center.X).ToDirectionInt());
					}
					break;
			}

			// This is where Flower Pow launches projectiles. Decompile Terraria to view that code.

			Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
			Projectile.spriteDirection = Projectile.direction;
			Projectile.ownerHitCheck = shouldOwnerHitCheck; // This prevents attempting to damage enemies without line of sight to the player. The custom Colliding code for spinning makes this necessary.

			// This rotation code is unique to this flail, since the sprite isn't rotationally symmetric and has tip.
			bool freeRotation = CurrentAIState == AIState.Ricochet || CurrentAIState == AIState.Dropping;
			if (freeRotation) {
				if (Projectile.velocity.Length() > 1f)
					Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
				else
					Projectile.rotation += Projectile.velocity.X * 0.1f; // roll
			}
			else {
				Vector2 vectorTowardsPlayer = Projectile.DirectionTo(mountedCenter).SafeNormalize(Vector2.Zero);
				Projectile.rotation = vectorTowardsPlayer.ToRotation() + MathHelper.PiOver2;
			}

			// If you have a ball shaped flail, you can use this simplified rotation code instead
			/*
			if (Projectile.velocity.Length() > 1f)
				Projectile.rotation = Projectile.velocity.ToRotation() + Projectile.velocity.X * 0.1f; // skid
			else
				Projectile.rotation += Projectile.velocity.X * 0.1f; // roll
			*/

			Projectile.timeLeft = 2; // Makes sure the flail doesn't die (good when the flail is resting on the ground)
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2); // Add a delay so the player can't button mash the flail
			player.itemRotation = Projectile.DirectionFrom(mountedCenter).ToRotation();
			if (Projectile.Center.X < mountedCenter.X) {
				player.itemRotation += (float)Math.PI;
			}
			player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

			if (CurrentAIState != AIState.Spinning)
				LifeTimer++;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			int defaultLocalNPCHitCooldown = 10;
			int impactIntensity = 0;
			Vector2 velocity = Projectile.velocity;
			float bounceFactor = 0.2f;
			if (CurrentAIState == AIState.LaunchingForward || CurrentAIState == AIState.Ricochet) {
				bounceFactor = 0.4f;
			}

			if (CurrentAIState == AIState.Dropping) {
				bounceFactor = 0f;
			}

			if (oldVelocity.X != Projectile.velocity.X) {
				if (Math.Abs(oldVelocity.X) > 4f) {
					impactIntensity = 1;
				}

				Projectile.velocity.X = (0f - oldVelocity.X) * bounceFactor;
				// if (Vector2.Distance(Main.player[Projectile.owner].MountedCenter, Projectile.Center) > 0.1 || LifeTimer > 10)
				// 	Projectile.velocity.X = (0f - oldVelocity.X) * bounceFactor;
				// else
				// 	Projectile.velocity.X = 0;
				CollisionCounter += 1f;
			}

			if (oldVelocity.Y != Projectile.velocity.Y) {
				if (Math.Abs(oldVelocity.Y) > 4f) {
					impactIntensity = 1;
				}

					Projectile.velocity.Y = (0f - oldVelocity.Y) * bounceFactor;
				CollisionCounter += 1f;
			}

			// If in the Launched state, spawn sparks
			if (CurrentAIState == AIState.LaunchingForward) {
				CurrentAIState = AIState.Ricochet;
				Projectile.localNPCHitCooldown = defaultLocalNPCHitCooldown;
				Projectile.netUpdate = true;
				Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
				Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
				impactIntensity = 2;
				Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
				Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, velocity);
				Projectile.position -= velocity;
			}

			// Here the tiles spawn dust indicating they've been hit
			if (impactIntensity > 0) {
				Projectile.netUpdate = true;
				for (int i = 0; i < impactIntensity; i++) {
					Collision.HitTiles(Projectile.position, velocity, Projectile.width, Projectile.height);
				}

				SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			}

			// Force retraction if stuck on tiles while retracting
			if (CurrentAIState != AIState.UnusedState && CurrentAIState != AIState.Spinning && CurrentAIState != AIState.Ricochet && CurrentAIState != AIState.Dropping && CollisionCounter >= 10f) {
				CurrentAIState = AIState.ForcedRetracting;
				Projectile.netUpdate = true;
			}

			// tModLoader currently does not provide the wetVelocity parameter, this code should make the flail bounce back faster when colliding with tiles underwater.
			//if (Projectile.wet)
			//	wetVelocity = Projectile.velocity;

			return false;
		}

		public override bool? CanDamage() {
			// Flails in spin mode won't damage enemies within the first 12 ticks. Visually this delays the first hit until the player swings the flail around for a full spin before damaging anything.
			if (CurrentAIState == AIState.Spinning && SpinningStateTimer <= 12f) {
				return false;
			}
			return base.CanDamage();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			// Flails do special collision logic that serves to hit anything within an ellipse centered on the player when the flail is spinning around the player. For example, the projectile rotating around the player won't actually hit a bee if it is directly on the player usually, but this code ensures that the bee is hit. This code makes hitting enemies while spinning more consistent and not reliant of the actual position of the flail projectile.
			if (CurrentAIState == AIState.Spinning) {
				Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
				Vector2 shortestVectorFromPlayerToTarget = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
				shortestVectorFromPlayerToTarget.Y /= 0.8f; // Makes the hit area an ellipse. Vertical hit distance is smaller due to this math.
				float hitRadius = 55f; // The length of the semi-major radius of the ellipse (the long end)
				return shortestVectorFromPlayerToTarget.Length() <= hitRadius;
			}
			// Regular collision logic happens otherwise.
			return base.Colliding(projHitbox, targetHitbox);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// Flails do a few custom things, you'll want to keep these to have the same feel as vanilla flails.

			if (CurrentAIState == AIState.Spinning) {
				modifiers.SourceDamage *= 0.5f;
			}

			// The hitDirection is always set to hit away from the player, even if the flail damages the npc while returning
			modifiers.HitDirectionOverride = (Main.player[Projectile.owner].Center.X < target.Center.X).ToDirectionInt();

			// Knockback is only 25% as powerful when in spin mode
			if (CurrentAIState == AIState.Spinning) {
				modifiers.Knockback *= 0.25f;
			}
			// Knockback is only 50% as powerful when in drop down mode
			else if (CurrentAIState == AIState.Dropping) {
				modifiers.Knockback *= 0.5f;
			}
		}

		// PreDraw is used to draw a chain and trail before the projectile is drawn normally.
		public override bool PreDraw(ref Color lightColor) {
			Vector2 playerArmPosition = Main.GetPlayerArmPosition(Projectile);

			// This fixes a vanilla GetPlayerArmPosition bug causing the chain to draw incorrectly when stepping up slopes. The flail itself still draws incorrectly due to another similar bug. This should be removed once the vanilla bug is fixed.
			playerArmPosition.Y -= Main.player[Projectile.owner].gfxOffY;

			Rectangle? chainSourceRectangle = null;
			// Drippler Crippler customizes sourceRectangle to cycle through sprite frames: sourceRectangle = asset.Frame(1, 6);
			float chainHeightAdjustment = 0f; // Use this to adjust the chain overlap. 

			Vector2 chainOrigin = chainSourceRectangle.HasValue ? (chainSourceRectangle.Value.Size() / 2f) : (chainTexture.Size() / 2f);
			Vector2 chainDrawPosition = Projectile.Center;
			Vector2 vectorFromProjectileToPlayerArms = playerArmPosition.MoveTowards(chainDrawPosition, 4f) - chainDrawPosition;
			Vector2 unitVectorFromProjectileToPlayerArms = vectorFromProjectileToPlayerArms.SafeNormalize(Vector2.Zero);
			float chainSegmentLength = (chainSourceRectangle.HasValue ? chainSourceRectangle.Value.Height : chainTexture.Height()) + chainHeightAdjustment;
			if (chainSegmentLength == 0) {
				chainSegmentLength = 10; // When the chain texture is being loaded, the height is 0 which would cause infinite loops.
			}
			float chainRotation = unitVectorFromProjectileToPlayerArms.ToRotation() + MathHelper.PiOver2;
			int chainCount = 0;
			float chainLengthRemainingToDraw = vectorFromProjectileToPlayerArms.Length() + chainSegmentLength / 2f;

			// This while loop draws the chain texture from the projectile to the player, looping to draw the chain texture along the path
			while (chainLengthRemainingToDraw > 0f) {
				// This code gets the lighting at the current tile coordinates
				Color chainDrawColor = Lighting.GetColor((int)chainDrawPosition.X / 16, (int)(chainDrawPosition.Y / 16f));

				// Flaming Mace and Drippler Crippler use code here to draw custom sprite frames with custom lighting.
				// Cycling through frames: sourceRectangle = asset.Frame(1, 6, 0, chainCount % 6);
				// This example shows how Flaming Mace works. It checks chainCount and changes chainTexture and draw color at different values

				var chainTextureToDraw = chainTexture;

				// Here, we draw the chain texture at the coordinates
				Main.spriteBatch.Draw(chainTextureToDraw.Value, chainDrawPosition - Main.screenPosition, chainSourceRectangle, chainDrawColor, chainRotation, chainOrigin, 1f, SpriteEffects.None, 0f);

				// chainDrawPosition is advanced along the vector back to the player by the chainSegmentLength
				chainDrawPosition += unitVectorFromProjectileToPlayerArms * chainSegmentLength;
				chainCount++;
				chainLengthRemainingToDraw -= chainSegmentLength;
			}

			// Add a motion trail when moving forward, like most flails do (don't add trail if already hit a tile)
			if (CurrentAIState == AIState.LaunchingForward) {
				Texture2D projectileTexture = TextureAssets.Projectile[Type].Value;
				Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
				SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
				int afterimageCount = Math.Min(Projectile.oldPos.Length - 1, (int)StateTimer);
				for (int k = afterimageCount; k > 0; k--) {
					Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
					Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
					Main.spriteBatch.Draw(projectileTexture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length / 3, spriteEffects, 0f);
				}
			}
			
			// Get the flail projectile
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

			// Calculate the position to draw at
			Vector2 drawOrigin2 = new Vector2(texture.Width / 2f - 2, texture.Height / 2f);
			Vector2 flailDrawPosition = Projectile.Center - Main.screenPosition;

			// Draw the projectile manually
			Main.EntitySpriteDraw(
				texture,
				flailDrawPosition,
				null,
				lightColor,
				Projectile.rotation,
				drawOrigin2,
				Projectile.scale,
				SpriteEffects.None,
				0
			);

			return false;
		}
	}


	public class AmalgamatedHandProjectile2: ModProjectile
	{
		public override string Texture => "CTG2/Content/Items/AmalgamatedHandProjectile";
		private const string ChainTexturePath = "Content/Items/AmalgamatedHandProjectileChain";


		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
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
				float maxChainLength = 170f;
				Projectile.tileCollide = true;
    			Projectile.netUpdate = true; //send netupdate when initially thrown too

				if (currentChainLength > maxChainLength) {
					// If we reach maxChainLength, we change behavior.
					Projectile.ai[0] = 1f;
					Projectile.netUpdate = true;
				}
				else if (!Main.mouseRight) {
					// Once player lets go of the use button, let gravity take over and let air friction slow down the Projectile
					if (Projectile.velocity.Y < 0f)
						Projectile.velocity.Y *= 0.9f;

					Projectile.velocity.Y += 1f; //1f
					Projectile.velocity.X *= 0.9f;
				}
			}
			else if (Projectile.ai[0] == 1f) {
				// When ai[0] == 1f, the Projectile has either hit a tile or has reached maxChainLength, so now we retract the Projectile
				float elasticFactorA = 25f / player.GetAttackSpeed(DamageClass.Melee);
				float elasticFactorB = 1.4f / player.GetAttackSpeed(DamageClass.Melee);
				float maxStretchLength = 460f; // This is the furthest the flail can stretch before being forced to retract. Make sure that this is a bit less than maxChainLength so you don't accidentally reach maxStretchLength on the initial throw.

				if (Projectile.ai[1] == 1f)
					Projectile.tileCollide = false;

				// If the user lets go of the use button, or if the Projectile is stuck behind some tiles as the player moves away, the Projectile goes into a mode where it is forced to retract and no longer collides with tiles.
				if (!Main.mouseRight || currentChainLength > maxStretchLength || !Projectile.tileCollide) {
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
			Projectile.timeLeft = 2; // Makes sure the flail doesn't die (good when the flail is resting on the ground)
			player.SetDummyItemTime(2); // Add a delay so the player can't button mash the flail

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
