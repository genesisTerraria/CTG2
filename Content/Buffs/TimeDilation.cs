using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class TimeDilation : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed *= 0.75f;
            player.accRunSpeed *= 0.75f;
            player.maxRunSpeed *= 0.75f;

            int dust = Dust.NewDust(
                player.position,
                player.width,
                player.height,
                DustID.ShimmerTorch,
                0f,
                0f,
                255
            );

            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.5f;
        }
    }
}
