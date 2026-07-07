using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace PvPHubIntegration;

public class StatsTracking : ModPlayer
{
    public static bool scrimsActive = false;

    public int accumKills = 0;
    public int accumDeaths = 0;
    public int accumDamage = 0;
    public int accumDamageTaken = 0;

    public static void StartScrimTracking()
    {
        if (!scrimsActive)
            ResetAllAccumulatedStats();

        scrimsActive = true;
    }

    public static void StopScrimTracking()
    {
        scrimsActive = false;
    }

    public static void AnnounceScrimStats()
    {
        if (Main.netMode != NetmodeID.Server || !scrimsActive)
            return;

        foreach (Player player in Main.player)
        {
            if (player == null || !player.active)
                continue;

            StatsTracking stats = player.GetModPlayer<StatsTracking>();
            if (player.team == 1)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[c/FF0000:{player.name}]: {stats.accumKills} Kills, {stats.accumDeaths} Deaths, {stats.accumDamage} Damage, {stats.accumDamageTaken} Damage Taken"), Color.Yellow);
            }
            else if (player.team == 3)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[c/0077B6:{player.name}]: {stats.accumKills} Kills, {stats.accumDeaths} Deaths, {stats.accumDamage} Damage, {stats.accumDamageTaken} Damage Taken"), Color.Yellow);
            }
        }
    }

    public static void ResetAllAccumulatedStats()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player == null || !player.active)
                continue;

            player.GetModPlayer<StatsTracking>().ResetAccumulatedStats();
        }
    }

    public void ResetAccumulatedStats()
    {
        accumKills = 0;
        accumDeaths = 0;
        accumDamage = 0;
        accumDamageTaken = 0;
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (!ShouldTrackStats())
            return;

        int killerIndex = damageSource.SourcePlayerIndex;
        if (pvp && IsValidPlayerIndex(killerIndex))
        {
            Player killer = Main.player[killerIndex];
            if (killer.active)
                killer.GetModPlayer<StatsTracking>().accumKills++;
        }

        accumDeaths++;
    }

    public override void OnHurt(Player.HurtInfo info)
    {
        if (!ShouldTrackStats())
            return;

        int attackerIndex = info.DamageSource.SourcePlayerIndex;
        if (IsValidPlayerIndex(attackerIndex))
        {
            Player attacker = Main.player[attackerIndex];
            if (attacker.active)
                attacker.GetModPlayer<StatsTracking>().accumDamage += Math.Min(Player.statLife, info.Damage);
        }

        accumDamageTaken += info.Damage;
    }

    private static bool ShouldTrackStats()
    {
        return scrimsActive && Main.netMode == NetmodeID.Server;
    }

    private static bool IsValidPlayerIndex(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < Main.maxPlayers;
    }
}
