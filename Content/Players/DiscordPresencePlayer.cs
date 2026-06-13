using CTG2.Content.ServerSide;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Players;

public class DiscordPresencePlayer : ModPlayer
{
    // This file simply removes the player identity if they disconnect and are in a queue
    public override void PlayerDisconnect()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            ModContent.GetInstance<NeatQueueTeamAssignmentSystem>()
                .UnregisterWhoAmI(Player.whoAmI);
        }
    }
}
