using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CTG2.Imported.DirectDashMod.Particles;
public class BubbleDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.noLightEmittence = false;
		dust.noLight = false;
		dust.velocity = Vector2.Zero;
		dust.scale *= Main.rand.NextFloat(0.85f, 1.15f);
		dust.fadeIn = 0.3f;
	}

	public override bool Update(Dust dust)
	{
		if (dust.scale > 0.022f)
		{
			dust.scale -= 0.022f;
		}
		else
		{
			dust.active = false;
		}
		if (dust.fadeIn > 0f)
		{
			dust.fadeIn *= 0.5f;
		}
		dust.velocity.Y += 0.02f;
		dust.position += dust.velocity;
		return false;
	}

	public override bool MidUpdate(Dust dust)
	{
		return false;
	}

	public override bool PreDraw(Dust dust)
	{
		Main.spriteBatch.Draw(base.Texture2D.Value, dust.position - Main.screenPosition, null, Lighting.GetColor((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), dust.color), 0f, base.Texture2D.Size() / 2f, dust.scale, SpriteEffects.None, 0f);
		return false;
	}
}
