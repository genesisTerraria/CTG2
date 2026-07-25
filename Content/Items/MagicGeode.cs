using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class MagicGeode : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Geode;
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.Geode);

            Item.damage = 30;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.shootSpeed = 8f;
            Item.crit = 0;
            Item.consumable = false;
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Shift projectile spawn when facing left and right
			if (player.direction == 1)
			{
				position.X -= 1f; // tweak this (8 = half tile)
			}
            else if (player.direction == -1)
			{
				position.X += 1f;
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // prevent vanilla spawn
		}
	}
}
