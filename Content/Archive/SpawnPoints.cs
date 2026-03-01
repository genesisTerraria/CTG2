using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CTG2.Content
{   
    /*
    public class SpawnPoints : ModPlayer
    {
        // Store spawn points for multiple maps
        private static readonly Dictionary<string, (Point Red, Point Blue)> MapSpawns = new()
        {
            { "cranes", (new Point(1343, 555), new Point(13, 216)) },
            { "stalactite", (new Point(1855, 450), new Point(1353, 450)) },
            { "goblin", (new Point(300, 280), new Point(600, 210)) }, //cooked rn 
            { "kraken", (new Point(2735, 453), new Point(2233, 453)) },
            { "keep", (new Point(3338, 576), new Point(2836, 576)) },
            { "triangles", (new Point(1231, 445), new Point(729, 445)) },
            { "classic", (new Point(1234, 573), new Point(732, 573)) },
            { "stones", (new Point(400, 300), new Point(650, 210)) } //cooked rn
        };

        private static string currentMap = "map1";

        public static void SetCurrentMap(string mapName)
{
    if (MapSpawns.ContainsKey(mapName.ToLower()))
    {
        currentMap = mapName.ToLower();
        Main.NewText($"Map switched to: {mapName}", 50, 255, 130);

        switch (currentMap)
        {
            case "cranes":
                WorldProperties.savedX = 1343;
                WorldProperties.savedY = 555;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(1343, 555, 1876, 647);
                break;

            case "stalactite":
                WorldProperties.savedX = 1336;
                WorldProperties.savedY = 430;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(1336, 430, 1871, 519);
                break;

            case "goblin":
                WorldProperties.savedX = 2224;
                WorldProperties.savedY = 550;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(2224, 550, 2760, 640);
                break;
                
            case "kraken":
                WorldProperties.savedX = 2221;
                WorldProperties.savedY = 434;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(2221, 434, 2749, 552);
                break;

            case "keep":
                WorldProperties.savedX = 2821;
                WorldProperties.savedY = 557;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(2821, 557, 3353, 645);
                break;

            case "triangles":
                WorldProperties.savedX = 2816;
                WorldProperties.savedY = 438;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(2816, 438, 3350, 528);
                break;

            case "classic":
                WorldProperties.savedX = 716;
                WorldProperties.savedY = 556;
                WorldProperties.savedRegion = WorldProperties.SaveRegion(716, 556, 1249, 643);
                break;


            default:
                Main.NewText("No region save defined for this map.", Color.Gray);
                break;
        }

        Main.NewText("Map region saved!", Color.LightGreen);
    }
    else
    {
        Main.NewText($"Map '{mapName}' not found!", 255, 50, 50);
    }
}


        public static void TeleportToTeamSpawn(Player player)
        {
            var (red, blue) = MapSpawns[currentMap];

            if (player.team == 1)
                player.Teleport(new Vector2(red.X * 16, red.Y * 16), 1);
            else if (player.team == 3)
                player.Teleport(new Vector2(blue.X * 16, blue.Y * 16), 1);
        }

        public static void TeleportToGameSpawn(Player player)
        {
            player.Teleport(GameUI.arenaCoords, 1);
        }

        public static void TeleportAllPlayersToSpawn()
        {
            foreach (Player player in Main.player)
            {
                if (player.active)
                {
                    TeleportToTeamSpawn(player);
                    player.AddBuff(BuffID.Webbed, 30);
                }
            }
        }

        public static void TeleportPlayersToClassSelection()
        {
            var (red, blue) = MapSpawns[currentMap];

            foreach (Player player in Main.player)
            {
                if (!player.active) continue;

                if (player.team == 1)
                    player.Teleport(new Vector2(1913 * 16, 381 * 16), 1);
                else if (player.team == 3)
                    player.Teleport(new Vector2(2184 * 16, 376 * 16), 1);
            }
        
        }
    }

*/
}
