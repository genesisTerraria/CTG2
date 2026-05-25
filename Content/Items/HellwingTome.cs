using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class HellwingTome : ModItem
	{
		public override string Texture => "CTG2/Content/Items/HellwingTome";
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(165);
			Item.shoot = 485;
			Item.damage = 34;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.crit = 0;
			Item.scale = 0;
			Item.mana = 0;
			Item.shootSpeed = 7f;
			Item.UseSound = SoundID.Item105;
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Shift projectile spawn when facing right
			if (player.direction == 1)
			{
				position.X -= 2f; // tweak this (8 = half tile)
			}
			else if (player.direction == -1)
			{
				position.X += 2f;
			}

			foreach (Projectile projectile in Main.projectile)
			{
				if (projectile.active && projectile.type == 485 && projectile.owner == player.whoAmI)
				{
					projectile.Kill();
				}
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // prevent vanilla spawn
		}
	}
}
