using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CTG2.Imported.DirectDashMod.Tiles;

public class SpaceWaterTile : ModTile
{
	public class SpaceWaterItem : ModItem
	{
		public override void SetDefaults()
		{
			base.Item.CloneDefaults(2);
			base.Item.createTile = ModContent.TileType<SpaceWaterTile>();
		}

		public override void AddRecipes()
		{
			base.CreateRecipe(25).AddIngredient(3, 25).AddIngredient(75)
				.AddIngredient(23, 10)
				.Register();
		}
	}

	public Asset<Texture2D> Space;

	public Asset<Texture2D> Bounds;

	public static readonly float[] Primes = new float[3] { 5f, 7f, 11f };

	public static readonly int[] Frames = new int[3] { 1, 2, 3 };

	public const int FRAME_SIZE = 320;

	public const int FRAME_PAD = 16;

	public const int FRAME_SCALE = 1;

	public override string Texture => "CTG2/Imported/DirectDashMod/Images/empty";

	public override void Load()
	{
		this.Space = ModContent.Request<Texture2D>("CTG2/Imported/DirectDashMod/Images/WaterSpace");
		this.Bounds = ModContent.Request<Texture2D>("CTG2/Imported/DirectDashMod/Images/SpaceBounds");
	}

	public override void SetStaticDefaults()
	{
		Main.tileSolid[base.Type] = true;
		Main.tileMergeDirt[base.Type] = false;
		Main.tileBlockLight[base.Type] = true;
		base.DustType = 29;
		base.AddMapEntry(new Color(16, 16, 64));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = (fail ? 1 : 3);
	}

	public int IsSpace(int i, int j, int ret)
	{
		if (i < 0 || j < 0 || i >= Main.tile.Width || j >= Main.tile.Height)
		{
			return 0;
		}
		if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == base.Type)
		{
			return ret;
		}
		return 0;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Vector2 zero = (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange));
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)((float)(i * 16 - (int)Main.screenPosition.X) + zero.X), (int)((float)(j * 16 - (int)Main.screenPosition.Y) + zero.Y), 16, 16), Color.Black);
		int frameOff = 0;
		for (int I = 0; I < 3; I++)
		{
			int loopFrames = ((I <= 0) ? 1 : ((SpaceWaterTile.Frames[I] - 1) * 2));
			int t = (int)(Main.timeForVisualEffects * (double)SpaceWaterTile.Primes[I] / 20.0 / 5.0) % loopFrames;
			int frameIndx = SpaceWaterTile.Frames[I] - 1 - Math.Abs(SpaceWaterTile.Frames[I] - 1 - t);
			Rectangle frame = new Rectangle(16 + 352 * (frameOff + frameIndx), 16, 16, 16);
			Vector2 vector = new Vector2(i, j) * 16f + Main.screenPosition / SpaceWaterTile.Primes[I];
			int offX = (int)vector.X % 320;
			int offY = (int)vector.Y % 320;
			frame.X += offX;
			frame.Y += offY;
			spriteBatch.Draw(this.Space.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, frame, Color.White, 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
			frameOff += SpaceWaterTile.Frames[I];
		}
		byte bounds = (byte)(this.IsSpace(i, j - 1, 1) | this.IsSpace(i + 1, j, 2) | this.IsSpace(i, j + 1, 4) | this.IsSpace(i - 1, j, 8));
		if (bounds != 15)
		{
			spriteBatch.Draw(this.Bounds.Value, new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero, new Rectangle(bounds % 4 * 16, bounds / 4 * 16, 16, 16), Lighting.GetColor(i, j), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);
		}
		return false;
	}
}
