using Microsoft.Xna.Framework;

namespace DirectDashMod.Items;

public class DashCrystal : SkippingStone
{
	public override string Texture => "CTG2/Imported/DirectDashMod/Items/DashCrystal";

	public override float DashDist => 146f;

	public override float DashSpeed => 11.52f;

	public override int DashCooldown => 60;

	public override int DashCount => 1;

	public override Color DrawColor => Color.White;

	public override Rectangle DrawFrame => new Rectangle(0, 0, 24, 24);

	public override float DrawScale => 1f;

	public override bool BounceDash => true;

	public override void AddRecipes()
	{
	}
}
