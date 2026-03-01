using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace DirectDashMod.Players;

public class DashFrameInfo
{
	public const float MAX_LIFE = 30f;

	public List<DrawData> data;

	public float life;

	public bool isClean;

	public DashFrameInfo(List<DrawData> draw, DashPlayer3 dashPly, Vector2 plyOff, float life = 30f)
	{
		int startCollecting = draw.FindLastIndex((DrawData drawData) => drawData.texture == DashPlayer3.DRAW_BOUND_TEX);
		this.life = life;
		this.data = new List<DrawData>(draw.Count - startCollecting + 1);
		Color targCol = dashPly.GetDashedColor(dashPly.Player.GetHairColor(useLighting: false));
		int effect = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DashPlayer3_DashEffect_Item>());
		_ = plyOff - dashPly.Player.Center;
		for (int I = startCollecting + 1; I < draw.Count; I++)
		{
			DrawData info = draw[I];
			if (info.shader != effect)
			{
				DrawData copy = info;
				copy.shader = effect;
				copy.color = targCol;
				copy.position = info.position + Main.screenPosition;
				this.data.Add(copy);
			}
		}
	}

	public void Clean(DashPlayer3 dashPly)
	{
		if (this.isClean)
		{
			return;
		}
		this.isClean = true;
		Vector2 min = dashPly.Player.Center;
		Vector2 max = dashPly.Player.Center;
		foreach (DrawData info in this.data)
		{
			min = Vector2.Min(min, info.position - info.origin * info.scale);
			max = Vector2.Max(max, info.position - info.origin * info.scale + info.texture.Size() * info.scale);
		}
		min = Vector2.Min(min, dashPly.Player.Center - Vector2.One * 160f);
		max = Vector2.Max(max, dashPly.Player.Center + Vector2.One * 160f);
		DrawSurface surf = new DrawSurface((int)min.X, (int)min.Y, (int)(max.X - min.X + 1f), (int)(max.Y - min.Y + 1f));
		surf.Begin();
		foreach (DrawData info2 in this.data)
		{
			surf.batch.Draw(info2.texture, info2.position - surf.offset, info2.sourceRect, info2.color, info2.rotation, info2.origin, info2.scale, info2.effect, 0f);
		}
		Texture2D flatImg = surf.End();
		int effect = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<DashPlayer3_DashEffect_Item>());
		DrawData d = new DrawData(flatImg, surf.offset, null, this.data[0].color);
		d.shader = effect;
		this.data.Clear();
		this.data.Add(d);
	}
}
