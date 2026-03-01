using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace DirectDashMod.Players;

public class DashPlayer3_FrameCollector : PlayerDrawLayer
{
	public static readonly Color FLASH_COLOR = new Color(255, 192, 192);

	public override bool IsHeadLayer => false;

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
	{
		return true;
	}

	public override Position GetDefaultPosition()
	{
		return PlayerDrawLayers.AfterLastVanillaLayer;
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		Player ply = drawInfo.drawPlayer;
		if (ply.TryGetModPlayer<DashPlayer3>(out var dash) && (dash.Dashing || dash.Swimming) && ((ply.Center - dash.lastShadowPos) * DashPlayer3.DASH_DIST_SCALE).LengthSquared() > 1024f)
		{
			if (ply.Center.DistanceSQ(dash.lastShadowPos) < ply.position.DistanceSQ(dash.expectedPos))
			{
				dash.lastShadowPos = ply.Center;
			}
			Vector2 dir2 = (ply.Center - dash.lastShadowPos) * DashPlayer3.DASH_DIST_SCALE;
			float lenLeft = dir2.Length();
			dir2 = dir2.SafeNormalize(Vector2.Zero);
			for (int I = 0; I < 3; I++)
			{
				if (!(lenLeft >= 32f))
				{
					break;
				}
				dash.lastShadowPos += dir2 / DashPlayer3.DASH_DIST_SCALE * 32f;
				lenLeft -= 32f;
				dash.AddDashShadow(drawInfo.DrawDataCache, dash.lastShadowPos);
			}
		}
		if (!ply.TryGetModPlayer<WallJumpPlayer>(out var jumpPly) || !jumpPly.StaminaFlash)
		{
			return;
		}
		int effect = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DashPlayer3_DashEffect_Item>());
		Span<DrawData> stuff = CollectionsMarshal.AsSpan(drawInfo.DrawDataCache);
		for (int i = 0; i < stuff.Length; i++)
		{
			if (stuff[i].shader != effect)
			{
				stuff[i].color = stuff[i].color.MultiplyRGB(DashPlayer3_FrameCollector.FLASH_COLOR);
			}
		}
	}
}
