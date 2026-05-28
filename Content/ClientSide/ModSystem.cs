using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.ClientSide;

public class DamageBoardSystem : ModSystem
{
    public override void PostUpdatePlayers()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (Main.myPlayer < 0 || Main.myPlayer >= Main.player.Length)
            return;

        if (CTG2.DamageBoardKeybind.JustPressed)
        {
            if (DamageBoardData.Visible)
            {
                DamageBoardData.Visible = false;
                return;
            }

            ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.RequestDamageBoard);
            packet.Send();
        }
    }
}
