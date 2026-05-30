using Terraria;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class TealCurrentSpeed : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed += 0.15f;
            player.accRunSpeed += 0.5f;
            player.maxRunSpeed += 0.5f;
        }
    }
}
