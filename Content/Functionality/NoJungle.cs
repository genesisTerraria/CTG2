using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class DisableLihzahrdJungleCounting : ModSystem
{
    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
    {
        Main.SceneMetrics.JungleTileCount -= tileCounts[TileID.LihzahrdBrick];
        // Main.SceneMetrics.JungleTileCount -= tileCounts[TileID.LihzahrdAltar];
    }
}