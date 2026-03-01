using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace DirectDashMod.Players;

public class DashPlayer3_DashEffect_Item : ModItem
{
	public override string Texture => "CTG2/Imported/DirectDashMod/Images/empty";

	public override void SetStaticDefaults()
	{
		if (!Main.dedServ)
		{
			GameShaders.Armor.BindShader(base.Item.type, new ArmorShaderData(base.Mod.Assets.Request<Effect>("Imported/DirectDashMod/Effects/WhiteTransEffect"), "Pass0"));
		}
		base.Item.ResearchUnlockCount = 3;
	}

	public override void SetDefaults()
	{
		int dye = base.Item.dye;
		base.Item.CloneDefaults(3561);
		base.Item.dye = dye;
	}
}
