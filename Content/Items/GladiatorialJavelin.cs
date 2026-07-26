using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace CTG2.Content.Items
{
	public class GladiatorialJavelin : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Javelin;
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.Javelin);

            Item.damage = 22;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.crit = 0;
			Item.shootSpeed = 14f;
			Item.mana = 25;
			Item.knockBack = 10;
			Item.rare = ItemRarityID.Red;
			Item.shoot = ProjectileID.JavelinFriendly;
			Item.UseSound = SoundID.DD2_DarkMageAttack;
            Item.consumable = false;
		}
	}
}
