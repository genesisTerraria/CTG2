using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CTG2.Content.Buffs
{
    public class Endangered : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            if (!player.HasBuff(BuffID.Invisibility))
            {
                int dust = Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.Blood, // nice green dust
                    0f,
                    0f
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}