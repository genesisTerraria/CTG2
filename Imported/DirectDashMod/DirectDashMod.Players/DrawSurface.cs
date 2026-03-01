using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace DirectDashMod.Players;

public class DrawSurface
{
	public RenderTarget2D surface;

	public SpriteBatch batch;

	public RenderTargetBinding[] oldTargets;

	public GraphicsDevice device;

	public readonly Vector2 offset;

	public readonly Vector2 size;

	public readonly Vector2 center;

	public const int BASE_WIDE = 128;

	public const int BASE_TALL = 192;

	public static Vector2 BASE_CENTER = new Vector2(64f, 96f);

	public DrawSurface(int x = 0, int y = 0, int wide = 128, int tall = 192)
	{
		this.device = Main.graphics.GraphicsDevice;
		this.surface = new RenderTarget2D(this.device, wide, tall);
		this.batch = new SpriteBatch(this.device);
		this.offset = new Vector2(x, y);
		this.size = new Vector2(wide, tall);
		this.center = this.size / 2f;
	}

	public void Begin()
	{
		this.oldTargets = this.device.GetRenderTargets();
		this.device.SetRenderTargets(new RenderTargetBinding(this.surface));
		this.device.Clear(Color.Transparent);
		this.batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.EffectMatrix);
	}

	public Texture2D End()
	{
		this.batch.End();
		this.device.SetRenderTargets(this.oldTargets);
		return this.surface;
	}
}
