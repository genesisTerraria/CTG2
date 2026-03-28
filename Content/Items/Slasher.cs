// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Terraria.DataStructures;
// using System;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Terraria.Audio;
// using Terraria.GameContent;

// public class Slasher : ModItem
// {
//     // Borrow the vanilla Excalibur sprite — no custom asset needed.
//     // If you have your own sprite, remove this override and place it at:
//     // Content/Items/Weapons/Excalibur.png
//     public override string Texture => "Terraria/Images/Item_" + ItemID.Excalibur;

//     public override void SetDefaults()
//     {
//         Item.width          = 40;
//         Item.height         = 40;
//         Item.damage         = 20;               // Base uncharged damage (projectile scales up to 40)
//         Item.DamageType     = DamageClass.Melee;
//         Item.knockBack      = 6f;
//         Item.useTime        = 30;
//         Item.useAnimation   = 30;
//         Item.useStyle       = ItemUseStyleID.Shoot;
//         Item.noMelee        = true;             // All hit detection handled by the projectile
//         Item.noUseGraphic   = true;             // Projectile draws the sword, not the item swing
//         Item.channel        = true;             // Hold to charge
//         Item.shoot          = ModContent.ProjectileType<SlasherHeld>();
//         Item.shootSpeed     = 1f;               // Not used for velocity, just needs to be non-zero
//         Item.value          = Item.buyPrice(gold: 20);
//         Item.rare           = ItemRarityID.Yellow;
//         Item.UseSound       = null;             // All sounds handled inside the projectile
//     }

//     public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
//         Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//     {
//         // Manually spawn the held projectile so we control spawn position and avoid duplicates
//         Projectile.NewProjectile(
//             source,
//             player.MountedCenter,
//             Vector2.Zero,
//             ModContent.ProjectileType<SlasherHeld>(),
//             damage,
//             knockback,
//             player.whoAmI
//         );

//         return false; // Returning false prevents the engine from spawning a second one
//     }
// }


// /// <summary>
// /// A single projectile that lives for the entire weapon interaction:
// ///   Phase 1 – Charging  : sword drawn vertically above head, golden particles emitted.
// ///   Phase 2 – Slashing  : sword sweeps from vertical (up) to horizontal (facing dir),
// ///                         dealing damage scaled to charge level.
// /// </summary>
// public class SlasherHeld : ModProjectile
// {
//     // ── tunables ──────────────────────────────────────────────────────────
//     private const int   ChargeMax       = 60;   // frames for full charge (1 second @ 60fps)
//     private const float BaseDamage      = 20f;
//     private const float MaxDamage       = 40f;
//     private const float SlashArcDegrees = 90f;  // sweep from straight-up to horizontal
//     private const int   SlashDuration   = 18;   // frames the slash takes
//     private const float SwordLength     = 52f;  // pixel length used for hit-box origin

//     private int syncTimer = 0;

//     // ── ai fields stored in ai[] for netSync ─────────────────────────────
//     // ai[0] = charge (0..ChargeMax)
//     // ai[1] = slashTimer (-1 = not yet released, 0..SlashDuration = slashing)
//     // ai[2] = locked facing direction during slash (1 or -1)

//     private ref float Charge      => ref Projectile.ai[0];
//     private ref float SlashTimer  => ref Projectile.ai[1];
//     private ref float LockedDir   => ref Projectile.ai[2];

//     private bool released = false;
//     private bool FullyCharged    => Charge >= ChargeMax;
//     private float ChargeRatio    => Charge / ChargeMax;   // 0..1

//     private bool _chargeSoundPlayed;

//     // ── setup ─────────────────────────────────────────────────────────────
//     public override void SetDefaults()
//     {
//         Projectile.width            = 10;
//         Projectile.height           = 10;
//         Projectile.friendly         = true;
//         Projectile.DamageType       = DamageClass.Melee;
//         Projectile.ignoreWater      = true;
//         Projectile.tileCollide      = false;
//         Projectile.penetrate        = -1;       // hits multiple enemies during slash
//         Projectile.timeLeft         = 9999;
//         Projectile.ownerHitCheck    = true;
//         Projectile.hide             = true;     // we draw manually in DrawBehindMe / Draw
//         Projectile.usesLocalNPCImmunity = true;
//         Projectile.localNPCHitCooldown  = SlashDuration; // hit each enemy once per swing
//     }

//     public override void SetStaticDefaults()
//     {
//         // Borrow vanilla Excalibur texture – no sprite needed in your mod's assets
//         // If you add your own sprite, remove this override and point Texture at your asset.
//     }

//     // Point texture at the vanilla Excalibur item sprite
//     public override string Texture => "Terraria/Images/Item_" + ItemID.Excalibur;

//     // ── main AI ───────────────────────────────────────────────────────────
//     public override void AI()
//     {
//         Player player = Main.player[Projectile.owner];

//         // Kill if player stops using the item unexpectedly (e.g., inventory opened)
//         if (player.dead || !player.active || player.HeldItem.shoot !=
//             ModContent.ProjectileType<SlasherHeld>())
//         {
//             Projectile.Kill();
//             return;
//         }

//         // Anchor projectile center to player
//         Projectile.position = player.MountedCenter;
//         Projectile.timeLeft = 2; // keep alive each tick

//         if (Projectile.owner == Main.myPlayer)
//         {
//             // ── PHASE 1: Charging ─────────────────────────────────────────────
//             if (player.channel && !released)
//             {
//                 player.SetCompositeArmFront(
//                     enabled: true,
//                     stretch: Player.CompositeArmStretchAmount.Full,
//                     rotation: 0 // straight up
//                 );
//                 // Advance charge
//                 if (Projectile.owner == Main.myPlayer)
//                 {
//                     Charge = MathHelper.Min(Charge + 1f, ChargeMax);
//                     Projectile.netUpdate = true;
//                 }

//                 // Play max-charge sound once when fully charged
//                 if (FullyCharged && !_chargeSoundPlayed && Projectile.owner == Main.myPlayer)
//                 {
//                     SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
//                     _chargeSoundPlayed = true;
//                 }

//                 // Golden particles (owner client only to avoid visual spam on others)
//                 if (Projectile.owner == Main.myPlayer && Main.rand.NextBool(2))
//                     SpawnChargeParticle(player);
//             }
//             else if (!released)
//             {
//                 released = true; // transition to phase 2 on release
//                 Projectile.netUpdate = true;
//                 LockedDir = player.direction;
//                 SlashTimer = 0f;
//             }

//             if (++syncTimer % 3 == 0) Projectile.netUpdate = true;
//         }

//         // ── PHASE 2: Slashing ─────────────────────────────────────────────
//         if (released && Projectile.owner == Main.myPlayer)
//         {
//             // Enforce locked facing direction
//             player.direction = (int)LockedDir;

//             if (Projectile.owner == Main.myPlayer)
//                 SlashTimer++;

//             // Kill when animation is done
//             if (SlashTimer > SlashDuration)
//             {
//                 Projectile.Kill();
//                 return;
//             }

//             // Scale damage to charge ratio (linear 20→40)
//             Projectile.damage = (int)MathHelper.Lerp(BaseDamage, MaxDamage, ChargeRatio);

//             // Position hit-box along the sword's current angle
//             float angle = CurrentSwordAngle(player);

//             player.SetCompositeArmFront(
//                 true,
//                 Player.CompositeArmStretchAmount.Full,
//                 angle
//             );

//             Projectile.Center = player.MountedCenter +
//                                 new Vector2(SwordLength * 0.5f, 0f).RotatedBy(angle);
//         }

//         // Prevent player from using other items while active
//         player.heldProj = Projectile.whoAmI;

//         if (released)
//         {
//             player.itemTime = 2;
//             player.itemAnimation = 2;
//         }
//     }

//         // ── helpers ───────────────────────────────────────────────────────────

//         /// <summary>
//         /// Returns the sword's world-space angle for the current frame.
//         /// Charging: straight up (−π/2).
//         /// Slashing: sweeps from −π/2 → 0 (right) or −π/2 → −π (left).
//         /// </summary>
//         private float CurrentSwordAngle(Player player)
//         {
//             if (!released)
//                 return 0;//-MathHelper.PiOver2; // straight up

//             float progress = MathHelper.Clamp(SlashTimer / SlashDuration, 0f, 1f);
//             // Ease-in-out for a snappy feel
//             progress = (float)(1 - Math.Cos(progress * Math.PI)) * 0.5f;

//             float startAngle = -MathHelper.PiOver2;                    // up
//             float endAngle   = player.direction == 1 ? 0f              // right
//                                                         : -MathHelper.Pi; // left
//             return MathHelper.Lerp(startAngle, endAngle, progress);
//         }

//         private void SpawnChargeParticle(Player player)
//         {
//             // Tip of the sword while charging (straight up)
//             Vector2 tip = player.MountedCenter + new Vector2(0f, -SwordLength);

//             // Scatter golden dust around the tip
//             Vector2 offset = Main.rand.NextVector2Circular(14f, 14f);
//             int dust = Dust.NewDust(tip + offset - new Vector2(4f), 8, 8,
//                 DustID.GoldFlame,
//                 Main.rand.NextFloat(-1f, 1f),
//                 Main.rand.NextFloat(-2f, -0.5f),
//                 Scale: Main.rand.NextFloat(0.8f, 1.4f));
//             Main.dust[dust].noGravity = true;

//             // Extra sparkle at full charge
//             if (FullyCharged && Main.rand.NextBool(3))
//             {
//                 int sparkle = Dust.NewDust(tip + offset - new Vector2(4f), 8, 8,
//                     DustID.SparksMech,
//                     Main.rand.NextFloat(-1.5f, 1.5f),
//                     Main.rand.NextFloat(-3f, -1f),
//                     Scale: Main.rand.NextFloat(1f, 1.8f));
//                 Main.dust[sparkle].noGravity = true;
//                 Main.dust[sparkle].color = Color.Gold;
//             }
//         }

//         // ── drawing ───────────────────────────────────────────────────────────

//         // We draw after the player (in front layer)
//         // public override void DrawBehindMe(int index, System.Collections.Generic.List<int> behindNPCsAndTiles,
//         //     System.Collections.Generic.List<int> behindNPCs,
//         //     System.Collections.Generic.List<int> behindProjectiles,
//         //     System.Collections.Generic.List<int> overPlayers,
//         //     System.Collections.Generic.List<int> overWiresUI)
//         // {
//         //     overPlayers.Add(index);
//         // }

//         public override bool PreDraw(ref Color lightColor)
//         {
//             Player player   = Main.player[Projectile.owner];
//             Texture2D tex   = TextureAssets.Item[ItemID.Excalibur].Value;
//             float angle     = CurrentSwordAngle(player);

//             // The sprite is typically oriented diagonally (handle bottom-left, tip top-right).
//             // We offset the rotation by 45° to align tip with our angle direction.
//             float drawRotation = angle - MathHelper.PiOver4;

//             //Vector2 drawPos = player.MountedCenter - Main.screenPosition;

//             Vector2 drawPos = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, angle)
//                       - Main.screenPosition;
//             Vector2 origin = new Vector2(0f, tex.Height);

//             // Glow tint ramps up with charge
//             Color tint = Color.Lerp(lightColor, Color.Gold, ChargeRatio * 0.7f);

//             // Draw the sword
//             Main.EntitySpriteDraw(
//                 tex,
//                 drawPos,
//                 null,
//                 tint,
//                 drawRotation,
//                 origin,
//                 1f,
//                 SpriteEffects.None,
//                 0
//             );

//             // During slash, draw a simple golden arc trail using dust visuals
//             // (actual dust spawned in AI; here we just add a ghost frame if mid-slash)
//             if (released && SlashTimer > 0 && SlashTimer < SlashDuration)
//             {
//                 float prevProgress = MathHelper.Clamp((SlashTimer - 3f) / SlashDuration, 0f, 1f);
//                 prevProgress = (float)(1 - Math.Cos(prevProgress * Math.PI)) * 0.5f;
//                 float startAngle = -MathHelper.PiOver2;
//                 float endAngle   = player.direction == 1 ? 0f : -MathHelper.Pi;
//                 float prevAngle  = MathHelper.Lerp(startAngle, endAngle, prevProgress);

//                 Main.EntitySpriteDraw(
//                     tex,
//                     drawPos,
//                     null,
//                     tint * 0.35f,   // ghost / trail opacity
//                     prevAngle - MathHelper.PiOver4,
//                     tex.Size() * 0.5f,
//                     1f,
//                     SpriteEffects.None,
//                     0
//                 );
//             }

//             return false;
//         }

//         // ── on-hit ────────────────────────────────────────────────────────────

//         public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
//         {
//             // Spawn a burst of gold dust on hit
//             for (int i = 0; i < 8; i++)
//             {
//                 int d = Dust.NewDust(target.position, target.width, target.height,
//                     DustID.GoldFlame,
//                     Main.rand.NextFloat(-3f, 3f),
//                     Main.rand.NextFloat(-3f, 3f),
//                     Scale: 1.2f);
//                 Main.dust[d].noGravity = true;
//             }
//         }
// }