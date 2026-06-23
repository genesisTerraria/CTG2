using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using CTG2.Content.Buffs;

namespace CTG2.Content.ClientSide;

[Autoload(Side = ModSide.Client)]
public class MarkedIndicator : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.dedServ)
            return;

        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Entity Health Bars");

        if (index == -1)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "CTG2: Marked Indicator",
            DrawMarkedIndicators,
            InterfaceScaleType.Game
        ));
    }

    private bool DrawMarkedIndicators()
    {
        if (Main.dedServ || Main.gameMenu)
            return true;

        int markBuff = ModContent.BuffType<Endangered>();

        foreach (Player player in Main.player)
        {
            if (player == null || !player.active || player.dead)
                continue;

            if (!player.HasBuff(markBuff))
                continue;

            DrawReticle(player);
        }

        return true;
    }

    // Draws a clean, pulsing corner-bracket reticle around a marked player.
    private static void DrawReticle(Player player)
    {
        // Breathing pulse used for both the alpha and the bracket spacing.
        float pulse = 0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);

        float pad = 6f + pulse * 3f;           // brackets gently breathe in and out
        float bracketLen = 9f;                 // length of each bracket arm
        const float thickness = 2f;

        Vector2 center = player.Center - Main.screenPosition;
        float halfW = player.width / 2f + pad;
        float halfH = player.height / 2f + pad;

        float left = center.X - halfW;
        float right = center.X + halfW;
        float top = center.Y - halfH;
        float bottom = center.Y + halfH;

        Color color = new Color(255, 40, 40) * (0.55f + 0.45f * pulse);

        // Each corner is an L made of one horizontal and one vertical bar.
        // Top-left
        DrawBar(left, top, bracketLen, thickness, color);
        DrawBar(left, top, thickness, bracketLen, color);
        // Top-right
        DrawBar(right - bracketLen, top, bracketLen, thickness, color);
        DrawBar(right - thickness, top, thickness, bracketLen, color);
        // Bottom-left
        DrawBar(left, bottom - thickness, bracketLen, thickness, color);
        DrawBar(left, bottom - bracketLen, thickness, bracketLen, color);
        // Bottom-right
        DrawBar(right - bracketLen, bottom - thickness, bracketLen, thickness, color);
        DrawBar(right - thickness, bottom - bracketLen, thickness, bracketLen, color);
    }

    private static void DrawBar(float x, float y, float width, float height, Color color, float rotation = 0f)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Main.spriteBatch.Draw(
            pixel,
            new Vector2(x, y),
            new Rectangle(0, 0, 1, 1),
            color,
            rotation,
            Vector2.Zero,
            new Vector2(width, height),
            SpriteEffects.None,
            0f
        );
    }
}
