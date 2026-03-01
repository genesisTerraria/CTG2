using Microsoft.Xna.Framework;

namespace DirectDashMod.Items;

public class DoubleDashCrystal : SkippingStone
{
	public override string Texture => "CTG2/Imported/DirectDashMod/Items/DoubleDashCrystal";

	public override float DashDist => 192f;

	public override float DashSpeed => 12.4800005f;

	public override int DashCooldown => 15;

	public override int DashCount => 2;

	public override Color DrawColor => Color.White;

	public override Rectangle DrawFrame => new Rectangle(0, 0, 24, 34);

	public override float DrawScale => 1f;

	public override bool BounceDash => true;

	public override void AddRecipes()
	{
	}
}
