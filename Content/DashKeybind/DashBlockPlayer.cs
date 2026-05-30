using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class DashBlockPlayer : ModPlayer
{
    public override void PreUpdateMovement()
    {
        if (Player.HasBuff(BuffID.Dazed) || Player.HasBuff(BuffID.OgreSpit))
        {
            if (Player.dashDelay == -1)
            {
                Player.dashDelay = 0;
                Player.dash = 0;
                Player.velocity.X = Player.velocity.X; // keep current velocity untouched
            }
        }
    }
}