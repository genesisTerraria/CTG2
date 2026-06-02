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
            Item.shoot = ModContent.ProjectileType<DissonanceHookProjectile>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.mana = 30;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class DissonanceHookProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.QueenSlimeHook;
        public override bool? CanUseGrapple(Player player) => false;

        private const float MaxRange = 30 * 16f; // 30 tiles in pixels
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

            if (Projectile.ai[0] == 0f)
            {
                // --- FLYING PHASE ---

                // Check max range — start retracting
                if (Vector2.Distance(Projectile.Center, owner.MountedCenter) >= MaxRange)
                {
                    Projectile.ai[0] = 1f;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                    return;
                }

                // Check enemy player collision — teleport
                // Check enemy player collision — teleport

                Player ownerPlayer = Main.player[Projectile.owner];

                if (Projectile.owner == Main.myPlayer)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player target = Main.player[i];
                        if (!target.active || target.dead || i == Projectile.owner) continue;
                        if (!target.hostile || !owner.hostile) continue;

                        if (Projectile.Hitbox.Intersects(target.Hitbox) && ownerPlayer.team != target.team)
                        {
                            Vector2 dest = Projectile.Center - new Vector2(owner.width / 2f, owner.height / 2f);

                            var mod = ModContent.GetInstance<CTG2>();
                            ModPacket packet = mod.GetPacket();
                            packet.Write((byte)MessageType.RequestTeleport);
                            packet.Write(owner.whoAmI);
                            packet.Write((int)dest.X);
                            packet.Write((int)dest.Y);
                            packet.Send();

                            ModPacket packet1 = mod.GetPacket();
                            packet1.Write((byte)MessageType.RequestAddBuff);
                            packet1.Write(target.whoAmI);
                            packet1.Write(320);
                            packet1.Write(120);
                            packet1.Send();

                            packet1 = mod.GetPacket();
                            packet1.Write((byte)MessageType.RequestAddBuff);
                            packet1.Write(target.whoAmI);
                            packet1.Write(ModContent.BuffType<TimeDilation>());
                            packet1.Write(120);
                            packet1.Send();

                            SoundEngine.PlaySound(SoundID.Item8, owner.Center);

                            Projectile.Kill();
                            return;
                        }
                    }
                }

                // Rotate projectile to face velocity direction
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
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