using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;


public class ProtectedRegionTile : GlobalTile
{
    /* Guide: 
        1. Login as admin
        2. Go to the top left corner of the region that you want to be protected and press F5
        3. Go to the bottom right cornor of the region that you want to be protected and press F5
        4. Copy the dimensions of the region given into the rectangle class below
    */
    public static List<Rectangle> ProtectedRegions = new List<Rectangle>
    {
        new Rectangle(2085, 918, 17, 9), //spawn area (change this later)
        new Rectangle(762, 666, 94, 83), //blue base
        new Rectangle(1192,666, 96, 83), //red base
        new Rectangle(1659, 584, 115, 51), //red class selection
        new Rectangle(2411, 580, 115, 54), //blue class selection
    };
    public override bool CanPlace(int i, int j, int type)
    {
        if (IsInProtectedRegion(i, j))
            return false;

        return base.CanPlace(i, j, type);
    }
    public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
    {
        if (IsInProtectedRegion(i, j))
            return false;

        return base.CanKillTile(i, j, type, ref blockDamaged);
    }


    private bool IsInProtectedRegion(int i, int j)
    {
        foreach (var region in ProtectedRegions)
        {
            if (region.Contains(i, j))
                return true;
        }
        return false;
    }
}


public class TempModPlayer : ModPlayer
{
    private Point? firstCorner = null;

    public override void PostUpdate()
    {
        var adminPlayer = Player.GetModPlayer<CTG2.Content.AdminPlayer>();

        if (adminPlayer.IsAdmin && Main.myPlayer == Player.whoAmI &&
            Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5) &&
            !Main.oldKeyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F5))
        {
            int tileX = (int)(Player.position.X / 16);
            int tileY = (int)(Player.position.Y / 16);

            if (firstCorner == null)
            {
                firstCorner = new Point(tileX, tileY);
                Main.NewText($"First corner saved: ({tileX}, {tileY})", 255, 255, 0);
            }
            else
            {
                int x1 = firstCorner.Value.X;
                int y1 = firstCorner.Value.Y;
                int x2 = tileX;
                int y2 = tileY;

                int minX = Math.Min(x1, x2);
                int minY = Math.Min(y1, y2);
                int width = Math.Abs(x2 - x1) + 1;
                int height = Math.Abs(y2 - y1) + 1;

                Main.NewText($"Rectangle: new Rectangle({minX}, {minY}, {width}, {height})", 0, 255, 0);
                firstCorner = null;
            }
        }
    }
}
