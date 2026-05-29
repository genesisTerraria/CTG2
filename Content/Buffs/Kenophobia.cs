using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CTG2.Content.Buffs
{
    public class Kenophobia : ModBuff
    {
        private const int cooldown = 2;
        private int count = 0;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;

            // Reuse the Webbed buff texture
            BuffID.Sets.IsATagBuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (count % cooldown == 0 && !player.HasBuff(BuffID.Invisibility))
            {
                int dust = Dust.NewDust(
                    player.position,
                    player.width,
                    player.height,
                    DustID.Shadowflame,
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