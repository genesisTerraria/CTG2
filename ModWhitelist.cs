using Terraria;
using Terraria.ModLoader;
using System.Linq;
using System.IO;

namespace CTG2
{
    public class ModWhitelist : ModPlayer
    {
        public override void OnEnterWorld()
        {
            // Only the client needs to report their mods to the server
            if (Main.netMode == 1) { 
                SendModListToServer();
            }
        }

        private void SendModListToServer()
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncModList);
            
            // Join all mod names into one string to send
            string modNames = string.Join(",", ModLoader.Mods.Select(m => m.Name));
            packet.Write(modNames);
            packet.Send();
        }
    }
}