
//  using Microsoft.Xna.Framework;
//  using System;
//  using Terraria;
//  using Terraria.Graphics.Capture;
//  using Terraria.ID;
//  using Terraria.ModLoader;
//  using CTG2.Content.ServerSide;
//  using CTG2.Content.ClientSide;
//  using CTG2.Content.Configs;


// namespace CTG2.Content
// {
//     /*
//       //Shows setting up two basic biomes. For a more complicated example, please request.
//     public class CTG2Biome : ModBiome
//     {
//         public override int Music
//         {
//             get
//             {
//                 return MusicLoader.GetMusicSlot(Mod, "Assets/Music/clashroyaleOT");
//             }
//         }

//         public override bool IsBiomeActive(Player player)
//             => ModContent.GetInstance<CTG2Config>().ClashRoyaleOTMusic; //make biome only active during overtime
//                                                                         //todo: Make sure isovertime is synced clientside

//         public override SceneEffectPriority Priority
//             => SceneEffectPriority.BiomeHigh;
//     } */
    

// public class DynamicMusic : ModBiome
// {
//     public override bool IsBiomeActive(Player player) => true;
//     public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

//     public override int Music {
        
//         get
//             {
//                 int idx = ModContent.GetInstance<CTG2Config>().SelectedMusicIndex;
//                 switch (idx)
//                 {
//                     case 1: return MusicLoader.GetMusicSlot(Mod, "Assets/Music/clashroyaleOT");
//                     case 2: return MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery");

//                     default:
//                         return -1;
//                 }
//             }
//     }
// }
// }
