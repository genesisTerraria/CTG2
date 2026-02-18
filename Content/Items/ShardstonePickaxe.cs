using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
	public class ShardstonePickaxe : ModItem
	{
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.TitaniumPickaxe);
			Item.damage = 8;
			Item.useTime = 7;
			Item.useAnimation = 14;
			Item.scale = .8f;
   			Item.crit = 0;
			Item.knockBack = 0.7f;
		}
	}
}
