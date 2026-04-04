using Terraria;
using Terraria.ModLoader;
using System.Linq;
using System.IO;
using Terraria.Localization;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ID;


namespace CTG2.Content.Functionality
{
    public class PlayerDifficultyAnticheat : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (Main.netMode == 1)
            { 
                PlayerDifficultyKick();
            }
        }

        private void PlayerDifficultyKick()
        {
            if (Player.difficulty != 0)
            {
                string message = "This server only allows Softcore characters.";

                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.KickPlayerDifficulty);
                packet.Write(Player.whoAmI);
                packet.Send();
            }
        }
    }
}