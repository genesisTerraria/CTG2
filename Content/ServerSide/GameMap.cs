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
    Goblin,
    SteppingStones,
    Runes,
    Shapes,
    Drippler,
    Crown,
    Caliburn,
    Crypt,
    Cheese
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
            return; 

        var mapData = GetMap(mapPick);
        int startX = PasteX;
        int startY = PasteY;
        int mapWidth = mapData[0].Count;
        int mapHeight = mapData.Count;

        WorldGen.noTileActions = true;
        WorldGen.gen = true;

        // PASS 1: TILES AND WALLS
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int wx = x + startX;
                int wy = y + startY;

                if (!WorldGen.InWorld(wx, wy))
                    continue;

                Tile tile = Main.tile[wx, wy];
                var mapTile = mapData[y][x];

                // Completely wipe existing state
                tile.ClearEverything();

                if (mapTile.TileType.HasValue)
                {
                    tile.HasTile = true;
                    tile.TileType = (ushort) (mapTile.TileType ?? 0);
                    tile.IsHalfBlock = mapTile.HalfBlock;
                    tile.Slope = mapTile.Slope;
                    tile.WallType = (ushort) (mapTile.WallType ?? 0);
                    tile.TileColor = (byte) mapTile.TileColor;
                    tile.WallColor = (byte) mapTile.WallColor;
                }
            }
        }

        // PASS 2: LIQUIDS
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int wx = x + startX;
                int wy = y + startY;

                if (!WorldGen.InWorld(wx, wy))
                    continue;

                Tile tile = Main.tile[wx, wy];
                var mapTile = mapData[y][x];

                tile.LiquidType = mapTile.LiquidType;
                tile.LiquidAmount = mapTile.LiquidAmount;
            }
        }

        // PASS 3: EXPLICIT FRAMING
        // We do this before syncing so the server knows exactly what the tiles look like
        WorldGen.RangeFrame(startX, startY, startX + mapWidth, startY + mapHeight);

        // PASS 4: Network Force-Sync
        if (Main.netMode == NetmodeID.Server)
        {
            // Calculate the 200x150 sections affected by this map load
            int sectionX1 = startX / 200;
            int sectionX2 = (startX + mapWidth) / 200;
            int sectionY1 = startY / 150;
            int sectionY2 = (startY + mapHeight) / 150;

            for (int sx = sectionX1; sx <= sectionX2; sx++)
            {
                for (int sy = sectionY1; sy <= sectionY2; sy++)
                {
                    // 1. Mark the section as "not seen" by clients to force a refresh
                    for (int i = 0; i < 255; i++)
                    {
                        if (Netplay.Clients[i].IsActive)
                            Netplay.Clients[i].TileSections[sx, sy] = false;
                    }
                    
                    // 2. Send the raw section data (This mimics the "joining world" sync)
                    NetMessage.SendSection(-1, sx, sy);
                }
            }
        }

        WorldGen.noTileActions = false;
        WorldGen.gen = false;
    }
}
