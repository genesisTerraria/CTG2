using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class Glory: ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            // if falling and holding up
            if (player.velocity.Y >= 0 && player.controlUp)
            {
                player.slowFall = true;
            }
        }
    }
}