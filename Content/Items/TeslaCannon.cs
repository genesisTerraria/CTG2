using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CTG2.Content.Items
{
    public class TeslaCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 29;              // heavier since it's now a single hit, not sustained DPS
            Item.DamageType = DamageClass.Magic;
            Item.crit = 0;
            Item.mana = 6;
            Item.scale = 0.8f;
            Item.useTime = 20;             // slow, deliberate railgun cadence
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.autoReuse = false;        // one press, one shot — not held-fire
            Item.shoot = ModContent.ProjectileType<TeslaBeamProjectile>();
            Item.shootSpeed = 16f;         // just needs to be nonzero so vanilla aims the spawn at the cursor
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack.WithVolumeScale(4f));

            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }
    }
    public class TeslaBeamProjectile : ModProjectile
    {
        private float beamLength;
        private const int Lifetime = 20; // total flash duration in ticks

        public override string Texture => "CTG2/Content/Items/TeslaBeam";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = Lifetime; // each enemy takes damage once total, not once per frame
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0f)
            {
                // scan exactly once, on the tick the shot is created
                Vector2 muzzle = Projectile.Center;
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);

                const float maxRange = 30 * 16f;
                float[] samples = new float[(int)Projectile.width];
                Collision.LaserScan(muzzle, direction, Projectile.width, maxRange, samples);
                beamLength = samples.Max();

                Projectile.velocity = direction; // lock the direction so it can't drift
                Projectile.rotation = direction.ToRotation();
                Projectile.ai[0] = 1f;           // mark "already scanned" so this block never runs again
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Projectile.Center;
            Vector2 end = start + Projectile.velocity * beamLength;
            float _ = float.NaN;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.width, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(0f, tex.Height / 2f);
            Vector2 scale = new Vector2(beamLength / tex.Width, 1f);

            float fade = Projectile.timeLeft / (float)Lifetime; // flash fades out as it expires
            Color drawColor = Color.Cyan * fade;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                new Rectangle(0, 0, tex.Width, tex.Height),
                drawColor,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0);

            return false;
        }
    }
}