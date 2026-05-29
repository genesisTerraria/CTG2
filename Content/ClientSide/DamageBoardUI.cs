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

public static class DamageBoardData
{
    public class DamageBoardEntry
    {
        public string Name;
        public int Team;
        public int Kills;
        public int Deaths;
        public int Damage;
        public int DamageTaken;
    }

    public static readonly List<DamageBoardEntry> Players = new();
    public static bool Visible = false;
}

public class DamageBoardUI : UIState
{
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!DamageBoardData.Visible)
        {
            return;
        }

        DrawDamageBoard(spriteBatch);
    }

    private void DrawDamageBoard(SpriteBatch spriteBatch)
    {
        DynamicSpriteFont font = FontAssets.MouseText.Value;
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        var blue = DamageBoardData.Players.Where(p => p.Team == 3).ToList();
        var red = DamageBoardData.Players.Where(p => p.Team == 1).ToList();

        int rowHeight = 22;
        int headerHeight = 34;  // header + spacing below
        int maxRows = Math.Max(blue.Count, red.Count);
        int panelWidth = Math.Min(850, Main.screenWidth - 80);
        int panelHeight = headerHeight + (maxRows * rowHeight) + 32;  // header + rows + padding
        panelHeight = Math.Min(panelHeight, Main.screenHeight - 40);  // clamp to screen
        int panelX = (Main.screenWidth - panelWidth) / 2;
        int panelY = (Main.screenHeight - panelHeight) / 2;
        Rectangle panel = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        Main.LocalPlayer.mouseInterface = false;

        // background for each team
        Rectangle leftPanel = new Rectangle(panelX + 8, panelY + 8, (panelWidth / 2) - 12, panelHeight - 16);
        Rectangle rightPanel = new Rectangle(panelX + panelWidth / 2 + 4, panelY + 8, (panelWidth / 2) - 12, panelHeight - 16);

        spriteBatch.Draw(pixel, leftPanel, new Color(14, 38, 96) * 0.92f);
        spriteBatch.Draw(pixel, rightPanel, new Color(96, 20, 20) * 0.92f);

        DrawBorder(spriteBatch, pixel, leftPanel, new Color(65, 135, 235) * 0.9f, 2);
        DrawBorder(spriteBatch, pixel, rightPanel, new Color(235, 85, 85) * 0.9f, 2);
        DrawBorder(spriteBatch, pixel, panel, Color.White * 0.2f, 2);

        int headerTop = panelY + 18;
        int centerX = panelX + panelWidth / 2;
        int leftX = panelX + 18;
        int rightX = centerX + 10;
        int columnWidth = (panelWidth / 2) - 28;

        // fixed column positions so stats don't shift with name length
        int leftNameX = leftPanel.X + 8;
        int leftDamageX = leftPanel.Right - 160;
        int leftDamageTakenX = leftPanel.Right - 100;
        int leftKDX = leftPanel.Right - 45;

        int rightNameX = rightPanel.X + 8;
        int rightDamageX = rightPanel.Right - 160;
        int rightDamageTakenX = rightPanel.Right - 100;
        int rightKDX = rightPanel.Right - 45;

        Rectangle leftHeader = new Rectangle(leftX, headerTop, columnWidth, 28);
        Rectangle rightHeader = new Rectangle(rightX, headerTop, columnWidth, 28);
        spriteBatch.Draw(pixel, leftHeader, new Color(0, 60, 140) * 0.9f);
        spriteBatch.Draw(pixel, rightHeader, Color.DarkRed * 0.9f);
        DrawBorder(spriteBatch, pixel, leftHeader, new Color(50, 150, 255), 1);
        DrawBorder(spriteBatch, pixel, rightHeader, Color.Red, 1);

        Utils.DrawBorderString(spriteBatch, "Blue Team", new Vector2(leftNameX + 1, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "Dealt", new Vector2(leftDamageX, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "Taken", new Vector2(leftDamageTakenX, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "K/D", new Vector2(leftKDX, headerTop + 4), Color.White, 0.95f);

        Utils.DrawBorderString(spriteBatch, "Red Team", new Vector2(rightNameX + 1, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "Dealt", new Vector2(rightDamageX, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "Taken", new Vector2(rightDamageTakenX, headerTop + 4), Color.White, 0.95f);
        Utils.DrawBorderString(spriteBatch, "K/D", new Vector2(rightKDX, headerTop + 4), Color.White, 0.95f);

        int listTop = headerTop + 34;

        // Draw all rows
        for (int i = 0; i < maxRows; i++)
        {
            int rowY = listTop + i * rowHeight;

            if (i < blue.Count)
            {
                bool isLocalBlue = string.Equals(blue[i].Name, Main.LocalPlayer.name, StringComparison.OrdinalIgnoreCase);
                DrawRow(spriteBatch, font, leftNameX, leftDamageX, leftDamageTakenX, leftKDX, rowY, i + 1, blue[i], new Color(50, 150, 255), isLocalBlue);
            }
            if (i < red.Count)
            {
                bool isLocalRed = string.Equals(red[i].Name, Main.LocalPlayer.name, StringComparison.OrdinalIgnoreCase);
                DrawRow(spriteBatch, font, rightNameX, rightDamageX, rightDamageTakenX, rightKDX, rowY, i + 1, red[i], new Color(220, 50, 50), isLocalRed);
            }
        }
    }

    private static void DrawRow(SpriteBatch spriteBatch, DynamicSpriteFont font, int nameX, int damageX, int damageTakenX, int kdX, int y, int rank, DamageBoardData.DamageBoardEntry entry, Color accent, bool isLocal)
    {
        Color nameColor = isLocal ? Color.Yellow : Color.White;

        // draw rank and name
        string nameText = $"{rank,2}. {entry.Name}";
        Utils.DrawBorderString(spriteBatch, nameText, new Vector2(nameX, y), nameColor, 0.85f);

        // draw damage dealt
        string dmgText = $"{entry.Damage}";
        Utils.DrawBorderString(spriteBatch, dmgText, new Vector2(damageX, y), isLocal ? nameColor : Color.White, 0.85f);

        // draw damage taken
        string dmgTakenText = $"{entry.DamageTaken}";
        Utils.DrawBorderString(spriteBatch, dmgTakenText, new Vector2(damageTakenX, y), isLocal ? nameColor : Color.White, 0.85f);

        // draw K/D
        string kdText = $"{entry.Kills}/{entry.Deaths}";
        Utils.DrawBorderString(spriteBatch, kdText, new Vector2(kdX, y), isLocal ? nameColor : Color.White, 0.85f);

        // draw name accent
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        spriteBatch.Draw(pixel, new Rectangle(nameX - 5, y, 3, 16), accent * 0.95f);
    }

    private static void DrawBorder(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color color, int thickness)
    {
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }
}
