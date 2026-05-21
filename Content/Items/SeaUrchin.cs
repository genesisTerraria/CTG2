using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class SeaUrchin : ModItem
	{
		public override string Texture => "CTG2/Content/Items/SeaUrchin";
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.ChumBucket);

			Item.damage = 24;
			Item.scale = 0;
			Item.shoot = ProjectileID.DripplerFlailExtraBall;
			Item.shootSpeed = 9f;
		}

		 public override void AddRecipes()
		{
			Recipe recipeUrchin = CreateRecipe();
            recipeUrchin.AddIngredient(ItemID.AtlanticCod, 2);
            recipeUrchin.Register();
		}

		public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source,
        Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// Shift projectile spawn when facing right
			if (player.direction == 1)
			{
				position.X -= 1f; // tweak this (8 = half tile)
			}
			else
			{
				position.X += 1f;
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false; // prevent vanilla spawn
		}
	}
}
