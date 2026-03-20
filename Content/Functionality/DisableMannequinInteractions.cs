// using Terraria;
// using Terraria.DataStructures;
// using Terraria.GameContent.Tile_Entities;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace CTG2.Content.Functionality
// {
//     public class NoMannequinTakeSystem : ModSystem
//     {
//         public override void Load()
//         {
//             On_MessageBuffer.GetData += InterceptMannequinPacket;
//         }

//         public override void Unload()
//         {
//             On_MessageBuffer.GetData -= InterceptMannequinPacket;
//         }

//         private static void InterceptMannequinPacket(
//             On_MessageBuffer.orig_GetData orig,
//             MessageBuffer self, int start, int length, out int messageType)
//         {
//             orig(self, start, length, out messageType);

//             // MessageID 121 = DisplayDollItemSync packet
//             if (messageType == 121)
//             {
//                 // Find the TEDisplayDoll and restore its items
//                 // after the packet has been processed
//                 int playerIndex = self.whoAmI;
//                 Player player = Main.player[playerIndex];

//                 // Walk all display dolls and sync their items back
//                 foreach (var te in TileEntity.ByID.Values)
//                 {
//                     if (te is TEDisplayDoll doll)
//                     {
//                         for (int i = 0; i < 8; i++)
//                         {
//                             Item item = doll.GetItem(i, false);
//                             if (item.IsAir)
//                             {
//                                 // Item was taken — remove from player and restore
//                                 // (TShock approach: revert the packet's effect)
//                             }
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }