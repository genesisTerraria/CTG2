using CTG2.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
    public class SpaceSplitter : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.QueenSlimeHook);
            Item.shoot = ModContent.ProjectileType<SpaceSplitterProjectile>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.damage = 20;
            Item.shootSpeed = 5f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class SpaceSplitterProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenSlimeHook;
        public override bool? CanUseGrapple(Player player) => false;
        public override float GrappleRange() => 25 * 16f;
        private const float RetractSpeed = 24f;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.QueenSlimeHook);
        }

        // ai[0]: 0 = flying, 1 = retracting
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (Projectile.ai[0] != 0f)
                Projectile.ai[0] = 1f;

            // Rotate projectile to face velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.ai[0] != 0f)
            {
                // --- RETRACTING PHASE ---

                Vector2 toPlayer = owner.MountedCenter - Projectile.Center;
                float dist = toPlayer.Length();

                // Close enough to player — kill
                if (dist < RetractSpeed)
                {
                    Projectile.Kill();
                    return;
                }

                // Move toward player
                Projectile.velocity = toPlayer.SafeNormalize(Vector2.Zero) * RetractSpeed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Keep projectile alive
            Projectile.timeLeft = 2;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // On tile hit, stop and retract
            Projectile.velocity = Vector2.Zero;
            Projectile.ai[0] = 1f;
            Projectile.netUpdate = true;
            return false;
        }
    }
}