using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Chat;


public class TimeControlSystem : ModSystem
{
    public override void PreUpdateWorld()
    {
        Main.dayTime = true;
        Main.time = 27000;
        Main.eclipse = false;
        Main.bloodMoon = false;
        Main.raining = false;
        Main.maxRaining = 0f;
        Main.cloudAlpha = 0f;
        Main.moonPhase = 0; 
        Main.windSpeedCurrent = 0f;
        Main.windSpeedTarget = 0f;
    }

    public class NoEnemySpawns : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            spawnRate = int.MaxValue;
            maxSpawns = 0;
        }
    }
}