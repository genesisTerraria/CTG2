using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class Restoration : ModBuff
    {
        private int count = 0;

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 8;

            if (!player.HasBuff(BuffID.Invisibility) && count % 2 == 0)
            {
                int dust = Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.Water_Hallowed, // nice green dust
                    0f,
                    0f,
                    255
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }

            count++;
        }
    }
}