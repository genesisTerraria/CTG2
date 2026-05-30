using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class RisingSun : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 20;

            int dust = Dust.NewDust(
                player.position,
                player.width,
                player.height,
                DustID.Pixie, // nice green dust
                0f,
                0f,
                150
            );

            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.5f;
        }
    }
}