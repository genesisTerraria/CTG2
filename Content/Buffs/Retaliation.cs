using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.Buffs
{
    public class Retaliation : ModBuff
    {
        public override LocalizedText Description => base.Description.WithFormatArgs("You see a gap in your opponent's defenses!");

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.20f;
        }
    }
}