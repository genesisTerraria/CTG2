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

// using Terraria;
// using Terraria.ModLoader;
// using System.Linq;
// using System.IO;
// using System;
// using System.Security.Cryptography;

// namespace CTG2
// {
//     public class ModWhitelist : ModPlayer
//     {
//         public override void OnEnterWorld()
//         {
//             foreach (var m in ModLoader.Mods)
//             {
//                 string hash = GetModHash(m);
//                 Main.NewText($"{m.Name}: {hash}");
//             }
//             // Only the client needs to report their mods to the server
//             if (Main.netMode == 1)
//             { 
//                 SendModListToServer();
//             }
//         }

//         private void SendModListToServer()
//         {
//             ModPacket packet = Mod.GetPacket();
//             packet.Write((byte)MessageType.SyncModList);

//             // Hash each mod's file and send "name:hash" pairs
//             var modEntries = ModLoader.Mods.Select(m => {
//                 string hash = GetModHash(m);
//                 return $"{m.Name}:{hash}";
//             });

//             string modList = string.Join(",", modEntries);
//             packet.Write(modList);
//             packet.Send();
//         }

//         private string GetModHash(Mod mod)
//         {
//             // Workshop cache
//             string workshopPath = Path.Combine(
//                 Main.SavePath, "..", "..", "..", "..",
//                 "workshop", "content", "1281930"
//             );

//             if (Directory.Exists(workshopPath))
//             {
//                 foreach (string dir in Directory.GetDirectories(workshopPath))
//                 {
//                     string candidate = Path.Combine(dir, mod.Name + ".tmod");
//                     if (File.Exists(candidate))
//                         return HashFile(candidate);
//                 }
//             }

//             // Check local mods folder after
//             string localPath = Path.Combine(Main.SavePath, "Mods", mod.Name + ".tmod");
//             if (File.Exists(localPath))
//                 return HashFile(localPath);

//             return "unknown";
//         }

//         private string HashFile(string path)
//         {
//             using var sha256 = SHA256.Create();
//             using var stream = File.OpenRead(path);
//             byte[] hashBytes = sha256.ComputeHash(stream);
//             return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
//         }
//     }
// }