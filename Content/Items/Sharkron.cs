using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Items
{
	public class Sharkron : ModItem
	{
		public override string Texture => "CTG2/Content/Items/Sharkron";
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.Snowball);

			Item.damage = 34;
			Item.shoot = ProjectileID.MiniSharkron;
			Item.shootSpeed = 12.5f;
			Item.ammo = AmmoID.None;
		}

		 public override void AddRecipes()
		{
			Recipe recipeUrchin = CreateRecipe();
            recipeUrchin.AddIngredient(ItemID.AtlanticCod, 1);
            recipeUrchin.Register();
		}
	}
}
