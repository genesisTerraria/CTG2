// using Terraria;
// using Terraria.ModLoader;
// using Terraria.ID;
// using CTG2.Content.Items;
// using CTG2.Content.Items.ModifiedWeps;
// using Microsoft.Xna.Framework; 
// using System;
// using System.Linq;
// using System.Collections.Generic;
// using System.IO;
// using System.Text.Json;
// using System.Runtime.CompilerServices;
// using CTG2;
// using CTG2.Content.ClientSide;
// using Terraria.Localization;
// using Terraria.Chat;
// using CTG2.Content;
// using CTG2.Content.ServerSide;
// using CTG2.Content.Buffs;

// public class AutoPingFreezer : ModPlayer
// {
//     public int ping = 0;
//     public int count = 0;
    
//     bool messageSent = false;
//     public override void PostUpdate()
//     {
//         if (count % 60 == 0)
//         {
//             messageSent = false;

//             var mod = ModContent.GetInstance<CTG2.CTG2>();
//             var packet = mod.GetPacket();
//             packet.Write((byte)MessageType.RequestPlayerPing);
//             packet.Write(Player.whoAmI);
//             packet.Write(false);
//             packet.Send();
//         }

//         if (ping > 750)
//         {
//             Player.AddBuff(149, 180);

//             if (!messageSent)
//             {
//                 Main.NewText("Your ping is too high!", Color.Red);

//                 messageSent = true;
//             }
//         }

//         count++;
//     }
// }