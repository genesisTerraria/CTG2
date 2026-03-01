using System;
using Terraria;
using Terraria.ModLoader;

namespace CTG2.Imported.DirectDashMod.Particles;

public class DashSparkDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.noLightEmittence = false;
		dust.noLight = true;
		dust.alpha = 127;
		dust.velocity *= 0.8f;
		dust.fadeIn = 1f;
	}

	public override bool Update(Dust dust)
	{
		dust.position += dust.velocity;
		dust.velocity *= 0.7f;
		if (dust.fadeIn > 0f)
		{
			dust.fadeIn -= 0.05f;
		}
		dust.alpha += 5;
		if (dust.alpha >= 255)
		{
			dust.active = false;
		}
		dust.scale = (float)Math.Sqrt(1f - (float)dust.alpha / 255f);
		return false;
	}
}
