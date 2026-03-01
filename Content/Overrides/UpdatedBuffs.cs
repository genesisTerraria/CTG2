using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class NoElectrifiedDamagePlayer : ModPlayer
{
    public override void UpdateBadLifeRegen()
    {
        if (Player.HasBuff(BuffID.Electrified))
        {
            bool movingHorizontally = Player.controlLeft || Player.controlRight;

            if (movingHorizontally)
            {
                // Electrified normally applies a larger penalty while moving
                Player.lifeRegen += 16;
            }
            else
            {
                // Standing still penalty
                Player.lifeRegen += 4;
            }
        }
    }
}
