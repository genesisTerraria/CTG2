using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CTG2.Content.Buffs
{
    public class FreezeStats : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<FreezeStatsPlayer>();

            // Freeze hp
            if (modPlayer.frozenHP == -1)
            {
                modPlayer.frozenHP = player.statLife;
            }

            player.statLife = modPlayer.frozenHP;
            modPlayer.frozenHP = player.statLife;

            // Freeze mana
            if (modPlayer.frozenMana == -1)
            {
                modPlayer.frozenMana = player.statMana;
            }

            player.statMana = modPlayer.frozenMana;
            modPlayer.frozenMana = player.statMana;

            // Freeze buffs
            if (!modPlayer.buffsCaptured)
            {
                modPlayer.frozenBuffs.Clear();

                for (int i = 0; i < Player.MaxBuffs; i++)
                {
                    int type = player.buffType[i];
                    int time = player.buffTime[i];

                    if (type > 0 && time > 0 && type != Type) // don't include this buff
                    {
                        modPlayer.frozenBuffs.Add((type, time));
                    }
                }

                modPlayer.buffsCaptured = true;
            }

            foreach (var (type, time) in modPlayer.frozenBuffs)
            {
                player.AddBuff(type, time);
            }
        }
    }

    public class FreezeStatsPlayer : ModPlayer
    {
        public int frozenHP = -1;
        public int frozenMana = -1;
        public int frozenRespawn = -1;
        public List<(int type, int time)> frozenBuffs = new();
        public bool buffsCaptured = false;

        public override void UpdateDead()
        {
            if (Player.HasBuff(ModContent.BuffType<FreezeStats>()))
            {
                // Freeze respawn timer
                if (frozenRespawn == -1)
                {
                    frozenRespawn = Player.respawnTimer;
                }

                Player.respawnTimer = frozenRespawn;
                frozenRespawn = Player.respawnTimer;
            }
        }

        public override void ResetEffects()
        {
            if (!Player.HasBuff(ModContent.BuffType<FreezeStats>()))
            {
                frozenHP = -1;
                frozenMana = -1;
                frozenRespawn = -1;
                frozenBuffs.Clear();
                buffsCaptured = false;
            }
        }
    }
}