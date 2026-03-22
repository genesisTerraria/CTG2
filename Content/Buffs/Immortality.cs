using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CTG2.Content.Buffs
{
    public class Immortality : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 100;

            int dust = Dust.NewDust(
                player.position,
                player.width,
                player.height,
                DustID.GoldFlame, // nice green dust
                0f,
                0f,
                150,
                Color.Green,
                1.6f
            );

            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.5f;
        }
    }
}