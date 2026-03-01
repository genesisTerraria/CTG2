using Microsoft.Xna.Framework;

namespace DirectDashMod.Items;

public class SkippingStone : DashGemBase
{
	public override float DashDist => 160f;

	public override float DashSpeed => 9.6f;

	public override int DashCooldown => 600;

	public override int DashCount => 1;

	public override Color DrawColor => Color.White;

	public override Rectangle DrawFrame => new Rectangle(0, 0, 32, 32);

	public override int ConstDefense => 1;

	public override void AddRecipes()
	{
		base.CreateRecipe().AddIngredient(3, 50).AddTile(17)
			.Register();
	}
}
