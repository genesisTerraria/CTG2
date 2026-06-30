using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace CTG2.Content.Items
{
    public class GladiatorialPolearm : ModItem
    {
		public override string Texture => "Terraria/Images/Item_" + ItemID.CobaltNaginata;

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CobaltNaginata);
			Item.damage = 36;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.shootSpeed = 4.2f;
			Item.crit = 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var abils = player.GetModPlayer<Abilities>();

			if (abils.class4PowerShot)
			{
				Projectile.NewProjectile(
					source,
					position,
					velocity,
					ModContent.ProjectileType<PolearmProjectile>(),
					damage,
					knockback,
					player.whoAmI
				);

				abils.class4PowerShot = false;

				return false;
			}

			return true;
		}
    }

    public class PolearmProjectile : ModProjectile
    {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CobaltNaginata;
		SoundStyle connectSound = new SoundStyle("CTG2/Content/Classes/Item_182");

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CobaltNaginata);
			AIType = ProjectileID.CobaltNaginata;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
			bool playedSound = false;

            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.Hitbox.Intersects(Projectile.Hitbox) && Main.player[proj.owner].team != owner.team)
                {
                    proj.Kill();

					if (!playedSound)
					{
                    	SoundEngine.PlaySound(connectSound.WithVolumeScale(Main.soundVolume * 2f), Projectile.Center); // CHANGE THIS SOUND
						playedSound = true;
					}
                }
            }

			int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood,
                    0f,
                    0f
                );

			Main.dust[dust].noGravity = true;
			Main.dust[dust].velocity *= 0.5f;

			base.AI();
        }

		public override bool PreDraw(ref Color lightColor)
		{
			lightColor = Color.Red;
			return true; // let vanilla draw proceed with the overridden lightColor
		}
    }
}