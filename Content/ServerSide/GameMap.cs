using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Terraria;
using Terraria.GameContent.Achievements;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Localization;

namespace CTG2.Content.ServerSide;

public enum MapTypes
{
    Kraken,
    Cranes,
    Stalactite,
    Triangles,
    Keep,
    Classic,
    Goblin
}

public class MapData
{
    public int? TileType { get; set; }
    public int? WallType  { get; set; }
    public int? TileColor  { get; set; }
    public int? WallColor  { get; set; }
    public bool HalfBlock {get; set;}
    public SlopeType Slope      {get; set;}
    public byte LiquidAmount {get; set;}
    public int LiquidType  {get; set;}
}
public class GameMap
{   
    public int PasteX { get; set; }
    public int PasteY { get; set; }

    public GameMap(int pasteX, int pasteY)
    {
        PasteX = pasteX;
        PasteY = pasteY;
    }
    

    public static Dictionary<MapTypes, List<List<MapData>>> PreloadedMaps = new();

    public static void PreloadAllMaps()
    {
        foreach (MapTypes mapType in Enum.GetValues(typeof(MapTypes)))
        {
            string fileName = mapType.ToString().ToLower();
            var mod = ModContent.GetInstance<CTG2>();
            using (var stream = mod.GetFileStream($"Content/MapSaves/{fileName}.json"))
            using (var fileReader = new StreamReader(stream))
            {
                var jsonData = fileReader.ReadToEnd();
                try
                {
                    var mapData = JsonSerializer.Deserialize<List<List<MapData>>>(jsonData);
                    PreloadedMaps[mapType] = mapData;
                }
                catch
                {
                    Main.NewText($"Failed to preload map {fileName}.", Microsoft.Xna.Framework.Color.Red);
                }
            }
        }
    }

    public List<List<MapData>> GetMap(MapTypes map)
    {
        if (PreloadedMaps.TryGetValue(map, out var cached))
        {
            return cached;
        }
            
        string fileName = map.ToString().ToLower();
        var mod = ModContent.GetInstance<CTG2>();

        using (var stream = mod.GetFileStream($"Content/MapSaves/{fileName}.json"))
        using (var fileReader = new StreamReader(stream))
        {
            var jsonData = fileReader.ReadToEnd();
            try
            {
                var mapData = JsonSerializer.Deserialize<List<List<MapData>>>(jsonData);
                return mapData;
            }
            catch
            {
                Main.NewText("Failed to load or parse map file.", Microsoft.Xna.Framework.Color.Red);
                return null;
            }
        }
    }

    public void LoadMap(MapTypes mapPick)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return; // Server must own world edits

        var mapData = GetMap(mapPick);
        int startX = PasteX;
        int startY = PasteY;

        int mapWidth = mapData[0].Count;
        int mapHeight = mapData.Count;

        WorldGen.noTileActions = true;
        WorldGen.gen = true;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var mapTile = mapData[y][x];

                int wx = x + startX;
                int wy = y + startY;

                // Bounds safety (VERY IMPORTANT)
                if (!WorldGen.InWorld(wx, wy))
                    continue;

                Tile tile = Main.tile[wx, wy];

                // Tiles
                if (mapTile.TileType.HasValue)
                {
                    tile.HasTile = true;
                    tile.TileType = (ushort)mapTile.TileType.Value;
                    tile.IsHalfBlock = mapTile.HalfBlock;
                    tile.Slope = mapTile.Slope;
                }
                else
                {
                    tile.ClearTile();
                }

                tile.WallType = (ushort) (mapTile.WallType ?? 0);
                tile.TileColor = (byte) (mapTile.TileColor ?? 0);
                tile.WallColor = (byte) (mapTile.WallColor ?? 0);
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                var mapTile = mapData[y][x];

                int wx = x + startX;
                int wy = y + startY;

                // Bounds safety (VERY IMPORTANT)
                if (!WorldGen.InWorld(wx, wy))
                    continue;

                Tile tile = Main.tile[wx, wy];

                // Liquids
                tile.LiquidType = mapTile.LiquidType;
                tile.LiquidAmount = mapTile.LiquidAmount;
            }
        }

        const int chunk = 50;

        for (int y = 0; y < mapHeight + chunk / 2; y += chunk)
        {
            for (int x = 0; x < mapWidth + chunk / 2; x += chunk)
            {
                int x1 = startX + x;
                int y1 = startY + y;
                int x2 = Math.Min(startX + x + chunk - 1, startX + mapWidth - 1);
                int y2 = Math.Min(startY + y + chunk - 1, startY + mapHeight - 1);

                WorldGen.RangeFrame(x1, y1, x2, y2);
                NetMessage.SendTileSquare(-1, x1, y1, chunk);
            }
        }

        WorldGen.noTileActions = false;
        WorldGen.gen = false;
    }
}
