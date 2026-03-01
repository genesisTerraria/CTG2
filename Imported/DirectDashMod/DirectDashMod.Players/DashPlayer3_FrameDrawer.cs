using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DirectDashMod.Players;

public class DashPlayer3_FrameDrawer : PlayerDrawLayer
{
	public override bool IsHeadLayer => false;

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
	{
		return true;
	}

	public override Position GetDefaultPosition()
	{
		return PlayerDrawLayers.BeforeFirstVanillaLayer;
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		if (!drawInfo.drawPlayer.TryGetModPlayer<DashPlayer3>(out var dash))
		{
			return;
		}
		foreach (DashFrameInfo dashShadowFrame in dash.dashShadowFrames)
		{
			double fade = dashShadowFrame.life / 30f;
			fade = (fade * fade * fade / 3.0 + fade / 3.0 + Math.Sqrt(fade) / 3.0) * 1.0;
			foreach (DrawData datum in dashShadowFrame.data)
			{
				DrawData data = datum;
				data.position -= Main.screenPosition;
				data.color.A = (byte)(255.0 * fade);
				drawInfo.DrawDataCache.Add(data);
			}
		}
		DrawData stopper = new DrawData(DashPlayer3.DRAW_BOUND_TEX, new Rectangle(0, 0, 0, 0), Color.Transparent);
		drawInfo.DrawDataCache.Add(stopper);
	}
}
