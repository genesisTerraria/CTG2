using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class GemBlessing : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 1;
        }
    }
}