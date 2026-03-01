using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CTG2.Imported.DirectDashMod.Particles; //or.particles


public class DashDust : ModDust
{
	public static ArmorShaderData drawEffect;

	public override void SetStaticDefaults()
	{
		if (!Main.dedServ)
		{
			DashDust.drawEffect = new ArmorShaderData(base.Mod.Assets.Request<Effect>("Imported/DirectDashMod/Effects/DashDustEffect"), "Pass0");
		}
	}

	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.frame = new Rectangle(0, 0, 16, 16);
		dust.scale *= 3f;
	}

	public override bool Update(Dust dust)
	{
		if (dust.fadeIn < 1f)
		{
			dust.fadeIn += 0.1f;
		}
		dust.alpha += (int)((float)(255 - dust.alpha) * 0.1f + 1f);
		if (dust.alpha >= 255)
		{
			dust.active = false;
		}
		return false;
	}

	public override bool MidUpdate(Dust dust)
	{
		return true;
	}

	public override bool PreDraw(Dust dust)
	{
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
		ArmorShaderData armorShaderData = DashDust.drawEffect;
		armorShaderData.UseTargetPosition(dust.velocity.SafeNormalize(Vector2.Zero));
		armorShaderData.UseOpacity((float)dust.alpha / 255f);
		armorShaderData.UseSaturation(dust.fadeIn);
		armorShaderData.Apply(null);
		Main.spriteBatch.Draw(base.Texture2D.Value, dust.position - Main.screenPosition - dust.frame.Size() / 2f * (dust.scale / 2f), dust.frame, Lighting.GetColor((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f)), 0f, Vector2.Zero, dust.scale, SpriteEffects.None, 0f);
		Main.spriteBatch.End();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		return false;
	}
}
