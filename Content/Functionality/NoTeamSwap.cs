// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;


// public class CTGPlayer : ModPlayer
// {
//     private int? lockedTeam = null;

//     public void LockTeam(int teamId)
//     {
//         lockedTeam = teamId;
//         Player.team = teamId;
//     }

// public override void PreUpdate()
// {
//     if (!lockedTeam.HasValue)
//         return;

//     if (Player.team != lockedTeam.Value)
//     {
//         Player.team = lockedTeam.Value;

//         if (Main.netMode != NetmodeID.SinglePlayer)
//         {
//             NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, Player.whoAmI, lockedTeam.Value);
//         }
//     }
// }




    
// }
