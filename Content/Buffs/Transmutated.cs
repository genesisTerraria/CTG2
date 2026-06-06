using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class Transmutated : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<TransmutatedPlayer>().transmutated = true;
        }
    }

    public class TransmutatedPlayer : ModPlayer
    {
        public bool transmutated = false;

        public override void ResetEffects()
        {
            transmutated = false;
        }

        public override void PostUpdateEquips()
        {
            if (!transmutated) return;

            // Lock position completely except gravity
            Player.noKnockback = true;
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlJump = false;

            Player.statDefense += 300;

            // Spawn gold dust particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Player.position,
                    Player.width,
                    Player.height,
                    DustID.GoldCoin,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    Scale: Main.rand.NextFloat(0.8f, 1.4f)
                );
                dust.noGravity = true;
            }
        }

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (!transmutated) return;

            // Tint player gold
            r = 1f;
            g = 0.84f;
            b = 0f;
        }
    }
}