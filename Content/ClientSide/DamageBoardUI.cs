using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace CTG2.Content.ClientSide;

public class DamageBoardUI : UIState
{
    private int _scrollRows;

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!DamageBoardData.Visible)
        {
            _scrollRows = 0;
            return;
        }

        DrawDamageBoard(spriteBatch);
    }

    private void DrawDamageBoard(SpriteBatch spriteBatch)
    {
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        int panelWidth = Math.Min(650, Main.screenWidth - 80);
        int panelHeight = Math.Min(210, Main.screenHeight - 120);
        int panelX = (Main.screenWidth - panelWidth) / 2;
        int panelY = (Main.screenHeight - panelHeight) / 2;
        Rectangle panel = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        Main.LocalPlayer.mouseInterface = false;

        // background + border
        spriteBatch.Draw(pixel, panel, Color.Black * 0.82f);
        DrawBorder(spriteBatch, pixel, panel, Color.White, 2);

        var blue = DamageBoardData.Players.Where(p => p.Team == 3).ToList();
        var red = DamageBoardData.Players.Where(p => p.Team == 1).ToList();

        int headerTop = panelY + 18;
        int centerX = panelX + panelWidth / 2;
        int leftX = panelX + 18;
        int rightX = centerX + 10;
        int columnWidth = (panelWidth / 2) - 28;

        Rectangle leftHeader = new Rectangle(leftX, headerTop, columnWidth, 28);
        Rectangle rightHeader = new Rectangle(rightX, headerTop, columnWidth, 28);
        spriteBatch.Draw(pixel, leftHeader, new Color(0, 60, 140) * 0.9f);
        spriteBatch.Draw(pixel, rightHeader, Color.DarkRed * 0.9f);
        DrawBorder(spriteBatch, pixel, leftHeader, new Color(50, 150, 255), 1);
        DrawBorder(spriteBatch, pixel, rightHeader, Color.Red, 1);

        Utils.DrawBorderString(spriteBatch, $"Blue Team ({blue.Count})", new Vector2(leftX + 8, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, $"Red Team ({red.Count})", new Vector2(rightX + 8, headerTop + 4), Color.White, 0.95f);

        int listTop = headerTop + 34;
        int listBottom = panelY + panelHeight - 16;
        int rowHeight = 22;
        int visibleRows = Math.Max(1, (listBottom - listTop) / rowHeight);
        int maxRows = Math.Max(blue.Count, red.Count);
        int maxScroll = Math.Max(0, maxRows - visibleRows);

        int wheelDelta = PlayerInput.ScrollWheelDelta / 120;
        if (wheelDelta != 0)
        {
            _scrollRows = Math.Clamp(_scrollRows - wheelDelta, 0, maxScroll);
        }
        _scrollRows = Math.Clamp(_scrollRows, 0, maxScroll);

        for (int i = 0; i < visibleRows; i++)
        {
            int idx = _scrollRows + i;
            int rowY = listTop + i * rowHeight;

            if (idx < blue.Count)
            {
                DrawRow(spriteBatch, font, leftX + 8, rowY, idx + 1, blue[idx], new Color(50, 150, 255));
            }
            if (idx < red.Count)
            {
                DrawRow(spriteBatch, font, rightX + 8, rowY, idx + 1, red[idx], new Color(220, 50, 50));
            }
        }

        if (maxScroll > 0)
        {
            int end = Math.Min(maxRows, _scrollRows + visibleRows);
            string footer = $"Showing {_scrollRows + 1}-{end} of {maxRows}";
            Vector2 size = font.MeasureString(footer) * 0.8f;
            Utils.DrawBorderString(spriteBatch, footer, new Vector2(panelX + panelWidth - size.X - 16, panelY + panelHeight - 22), Color.LightGray, 0.8f);
        }
    }

    private static void DrawRow(SpriteBatch spriteBatch, DynamicSpriteFont font, int x, int y, int rank, DamageBoardData.DamageBoardEntry entry, Color accent)
    {
        string rowText = $"{rank,2}. {entry.Name,-18}  {entry.Damage,5} dmg  {entry.Kills}/{entry.Deaths}";
        Utils.DrawBorderString(spriteBatch, rowText, new Vector2(x, y), Color.White, 0.85f);

        // left accent marker
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(pixel, new Rectangle(x - 6, y + 3, 3, 14), accent * 0.95f);
    }

    private static void DrawBorder(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color color, int thickness)
    {
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
