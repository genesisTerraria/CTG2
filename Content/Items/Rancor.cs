using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
	public class Rancor : ModItem // Archer bow
	{
		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.TendonBow);
	     
			Item.damage = 37;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 18;
			Item.height = 40;
			Item.useTime = 28; // determines ua/ut: burst firerate
			Item.useAnimation = 28; // doesnt matter
			Item.knockBack = 5;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Red;
			Item.autoReuse = false;
   			Item.mana = 0;
		}
	}
}
