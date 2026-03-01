using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace DirectDashMod.Players;

public class MoveOverridePlayer : ModPlayer
{
	public Vector2 velocity = Vector2.Zero;

	public Vector2 lastPosition;

	public Vector2 expectedPosition;

	public bool IsOverriding;

	public bool lastIsOverriding;

	public bool AllowRefresh;

	public void SetPlayerSpeed(Vector2 vel, bool AllowRefresh = false)
	{
		if (!this.lastIsOverriding)
		{
			this.lastPosition = (this.expectedPosition = base.Player.position);
		}
		this.velocity = vel;
		this.AllowRefresh = AllowRefresh;
		this.IsOverriding = true;
		if (AllowRefresh)
		{
			base.Player.velocity = Vector2.Zero;
			return;
		}
		base.Player.velocity = vel;
		if (base.Player.velocity.Y == 0f)
		{
			base.Player.velocity.Y = 1E-06f;
		}
	}

	public override void ResetEffects()
	{
		this.lastIsOverriding = this.IsOverriding;
		this.IsOverriding = false;
	}

	public static Vector2 BoundPos(Vector2 pos, Vector2 size)
	{
		Vector2 worldMin = new Vector2(Main.leftWorld, Main.topWorld);
		Vector2 worldMax = new Vector2(Main.rightWorld, Main.bottomWorld);
		pos = Vector2.Max(pos, worldMin);
		pos = Vector2.Min(pos, worldMax - size);
		return pos;
	}

	public override void PostUpdate()
	{
		if (this.IsOverriding)
		{
			this.expectedPosition = this.lastPosition + this.velocity;
			Vector2 dx = Collision.TileCollision(this.lastPosition, this.velocity, base.Player.width, base.Player.height, fallThrough: true);
			base.Player.position = this.lastPosition + dx;
			base.Player.position = MoveOverridePlayer.BoundPos(base.Player.position, base.Player.Size);
			this.lastPosition = base.Player.position;
			base.Player.gfxOffY = 0f;
		}
	}
}
