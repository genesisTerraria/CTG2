using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Microsoft.Xna.Framework;
using System;
using Terraria.Chat;
using CTG2;


namespace CTG2.Content.Commands
{
    public class RandomMapCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "rollmap";
        public override string Description => "Generates a random map";
        public override string Usage => "/rollmap";
        private long latestUseTime = 0;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.GameUpdateCount - latestUseTime < 60)
            {
                caller.Reply("You are using this command too quickly!", Color.Red);
                return;
            }

            latestUseTime = Main.GameUpdateCount;
            Player player = caller.Player;
            Random random = new Random();

            int mapNumber = random.Next(1, 9);
            string mapName = "";

            switch(mapNumber)
            {
                case 1:
                    mapName = "Triangles";
                    break;
                case 2:
                    mapName = "Stalactite";
                    break;
                case 3:
                    mapName = "Goblin";
                    break;
                case 4:
                    mapName = "Keep";
                    break;
                case 5:
                    mapName = "Kraken";
                    break;
                case 6:
                    mapName = "Cranes";
                    break;
                case 7:
                    mapName = "SteppingStones";
                    break;
                case 8:
                    mapName = "Temple";
                    break;
            }

            string msg = $"{player.name} generated the random map {mapName}!";
            
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestChat);
            packet.Write(msg);
            packet.Send();
        }
    }
}


