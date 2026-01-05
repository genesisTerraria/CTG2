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
	     
			Item.damage = 39;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 18;
			Item.height = 40;
			Item.useTime = 22;
			Item.useAnimation = 43;
			Item.knockBack = 5;
			Item.value = Item.buyPrice(silver: 1);
			Item.rare = ItemRarityID.Red;
			Item.autoReuse = false;
   			Item.mana = 0;
		}
	}
}
