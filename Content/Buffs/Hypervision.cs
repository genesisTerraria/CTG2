using Terraria;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class Hypervision : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.15f;
        }
    }
}