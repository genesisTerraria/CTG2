using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
	public class UpgradedShardstonePickaxe : ModItem
	{
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.TitaniumPickaxe);
			Item.damage = 8;
			Item.useTime = 6;
			Item.useAnimation = 12;
			Item.scale = .75f;
   			Item.crit = 0;
			Item.knockBack = 0.7f;
		}
	}
}
