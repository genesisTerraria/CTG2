using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class TomeOfTheDarkArts : ModItem // Archer bow
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.DemonScythe;
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.DemonScythe);
	     
			Item.damage = 36;
            Item.shoot = 496;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.shootSpeed = 4.7f;
            Item.crit = 0;
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Shift projectile spawn when facing right
			if (player.direction == 1)
			{
				position.X -= 1f; // tweak this (8 = half tile)
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // prevent vanilla spawn
		}
	}
}
