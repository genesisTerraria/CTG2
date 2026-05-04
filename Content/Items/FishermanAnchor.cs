using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class FishermansAnchor : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Anchor;
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.Anchor);
			Item.damage = 38;
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Shift projectile spawn when facing right
			if (player.direction == 1)
			{
				position.X -= 8f; // tweak this (8 = half tile)
			}
			else if (player.direction == -1)
			{
				position.X += 8f;
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // prevent vanilla spawn
		}
	}
}
