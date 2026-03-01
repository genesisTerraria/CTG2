// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace CTG2.Content.ServerSide
// {
//     public enum MusicStage
//     {
//         NoGame,
//         OngoingGame,
//         LastTwoMinutes,
//         Overtime
//     }

//     public class MusicManager : ModSystem
//     {
//         private MusicStage _lastStage = MusicStage.NoGame;

//         public override void PostUpdateWorld()
//         {
//             if (Main.netMode != NetmodeID.Server)
//                 return;

//             MusicStage currentStage = GetCurrentStage();

//             if (currentStage != _lastStage)
//             {
//                 string musicPathToPlay = GetMusicPathForStage(currentStage);

//                 var packet = Mod.GetPacket();
//                 packet.Write((byte)MessageType.UpdateMusic);
//                 packet.Write(musicPathToPlay);
//                 packet.Send();

//                 _lastStage = currentStage;
//             }
//         }

//         private MusicStage GetCurrentStage()
//         {
//             var gameManager = ModContent.GetInstance<GameManager>();
//             if (gameManager == null || !gameManager.IsGameActive)
//                 return MusicStage.NoGame;

//             if (gameManager.isOvertime)
//                 return MusicStage.Overtime;

//             int totalMatchDurationInTicks = 15 * 60 * 60;
//             int twoMinutesInTicks = 2 * 60 * 60;
//             if (gameManager.MatchTime >= gameManager.matchStartTime + totalMatchDurationInTicks - twoMinutesInTicks)
//                 return MusicStage.LastTwoMinutes;

//             return MusicStage.OngoingGame;
//         }

//         private string GetMusicPathForStage(MusicStage stage)
//         {
//             switch (stage)
//             {
//                 case MusicStage.OngoingGame:
//                     return "CTG2/Assets/Music/clashroyaleOT"; // Replace with your file
//                 case MusicStage.LastTwoMinutes:
//                     return "CTG2/Assets/Music/clashroyaleOT"; // Replace with your file
//                 case MusicStage.Overtime:
//                     return "CTG2/Assets/Music/clashroyaleOT";
//                 case MusicStage.NoGame:
//                 return "CTG2/Assets/Music/MysteriousMystery";
//                 default:
//                     return ""; // Empty string tells clients to stop custom music
//             }
//         }
//     }
// }
