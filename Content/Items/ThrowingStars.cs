using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;


namespace CTG2.Content.Items {
    public class ThrowingStars : ModItem
    {
        public override void SetDefaults() {
            Item.damage = 32;
            Item.crit = 0;
            Item.shootSpeed = 18;
            Item.useStyle = 5;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item1;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.autoReuse = true;
            Item.mana = 15;

            Item.shoot = ModContent.ProjectileType<ThrowingStarsProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, MathHelper.ToRadians(-10f));
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, MathHelper.ToRadians(10f));

            return false;
        }
    }

    public class ThrowingStarsProjectile : ModProjectile
    {
        public override string Texture => "CTG2/Content/Items/ThrowingStarsProjectile";
        private bool velocitySet = false;
        SoundStyle sound = new SoundStyle("CTG2/Content/Items/ThrowingStarsSound");


        public override void SetDefaults()
        {
            Projectile.netImportant = true; // This ensures that the projectile is synced when other players join the world.
            Projectile.width = 8; // The width of your projectile
            Projectile.height = 8; // The height of your projectile
            Projectile.friendly = true; // Deals damage to enemies
            Projectile.penetrate = 1; // Infinite pierce
            Projectile.DamageType = DamageClass.Magic; // Deals magic damage
            Projectile.usesLocalNPCImmunity = true; // Used for hit cooldown changes in the ai hook
            Projectile.localNPCHitCooldown = 10; // This facilitates custom hit cooldown logic
            Projectile.timeLeft = 50;
            Projectile.netUpdate = true;
        }


        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!velocitySet)
            {
                Vector2 directionToMouse = player.MountedCenter
                    .DirectionTo(Main.MouseWorld)
                    .SafeNormalize(Vector2.UnitX * player.direction);

                directionToMouse = directionToMouse.RotatedBy(Projectile.ai[0]);

                SoundEngine.PlaySound(sound.WithVolumeScale(Main.soundVolume * 1.25f), player.Center);

                Projectile.velocity = directionToMouse * 10f;
                velocitySet = true;
            }

            Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.alpha = 0;

            // Spin the sprite
            Projectile.rotation += 0.3f * Projectile.direction;
        }


        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();

            return false;
        }


        public override bool? CanDamage()
        {
            return true;
        }
    }
}