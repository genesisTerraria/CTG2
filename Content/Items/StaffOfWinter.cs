using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;

namespace CTG2.Content.Items
{
	public class StaffOfWinter : ModItem // White Mage staff
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Type] = true;
		}


		public override void SetDefaults()
		{
	  		Item.CloneDefaults(ItemID.SpectreStaff);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item28;
		}


		public override bool? UseItem(Player player)
		{
			SoundEngine.PlaySound(SoundID.Item20, player.Center);

			return true;
		}
	}
}
