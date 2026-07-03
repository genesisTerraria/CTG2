using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;

namespace CTG2.Content.ClientSide;

public static class BanTimer
{
    private static ulong _endTick;

    public static bool Visible { get; private set; }

    public static void ShowBanTimer(int durationTicks)
    {
        Visible = true;
        _endTick = Main.GameUpdateCount + (ulong)Math.Max(0, durationTicks);
    }

    public static void HideBanTimer()
    {
        Visible = false;
        _endTick = 0;
    }

    public static void Draw()
    {
        if (!Visible)
            return;

        int remainingTicks = GetRemainingTicks();
        if (remainingTicks <= 0)
        {
            HideBanTimer();
            return;
        }

        int remainingSeconds = (int)Math.Ceiling(remainingTicks / 60f);
        int minutes = remainingSeconds / 60;
        int seconds = remainingSeconds % 60;
        string timeText = $"{minutes}:{seconds:D2}";

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        float scale = 1f;
        int boxPaddingX = 14;
        int boxPaddingY = 4;
        int topY = 10;

        Vector2 timeSize = font.MeasureString(timeText) * scale;
        int timeW = (int)timeSize.X + boxPaddingX * 2;
        int timeH = (int)timeSize.Y + boxPaddingY * 2;
        int startX = (Main.screenWidth - timeW) / 2;

        Rectangle timeBox = new Rectangle(startX, topY, timeW, timeH);
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel, timeBox, Color.Black * 0.75f);
        DrawBorder(Main.spriteBatch, pixel, timeBox, Color.White, 2);

        Vector2 timeTextPos = new Vector2(
            timeBox.X + (timeBox.Width - timeSize.X) / 2,
            timeBox.Y + (timeBox.Height - timeSize.Y) / 2);

        Utils.DrawBorderString(Main.spriteBatch, timeText, timeTextPos, Color.White, scale);
    }

    private static int GetRemainingTicks()
    {
        if (Main.GameUpdateCount >= _endTick)
            return 0;

        ulong remaining = _endTick - Main.GameUpdateCount;
        return remaining > int.MaxValue ? int.MaxValue : (int)remaining;
    }

    private static void DrawBorder(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rect, Color color, int thickness)
    {
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
