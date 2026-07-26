using Terraria;
using Terraria.ModLoader;
using CTG2.Content.ClientSide;

namespace CTG2.Content.Buffs
{
    public class OvertimeBuff : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            int minScaleTime = 15 * 60 * 60;
            int curTime = GameInfo.matchTime - GameInfo.matchStartTime;

            if (curTime >= minScaleTime)
            {
                float bonus = (curTime - minScaleTime) / 18000f;
                if (bonus > 1)
                    bonus = 1;
                    
                player.GetDamage(DamageClass.Generic) += bonus;
            }
        }
    }
}