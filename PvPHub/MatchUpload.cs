using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CTG2.Content.ServerSide;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace PvPHubIntegration;

// Builds the official match payload at the end of a queue and uploads it
// Heavily relies on PvPHub, in the future make code more robust 
public static class MatchUpload
{
    // Terraria team ids used by CTG2
    private const int TeamRed = 1;
    private const int TeamBlue = 3;

    // Match payload player teams use Terraria/PvPHub team ids.
    private const uint PayloadTeamBlue = 3;
    private const uint PayloadTeamRed = 1;

    // PvPHub debug authentication marks synthetic session ids with bit 63 and
    // adds whoAmI. Tavernkeep cannot bind those unsigned values to SQL, so
    // restore the claimed Steam id at the upload boundary.
    private const ulong DebugSteamIdMask = 1UL << 63;

    // Starting time is stamped by OnFullRosterJoined()
    public static DateTime? MatchStartUtc { get; private set; }

    public static void MarkMatchStart()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        MatchStartUtc = DateTime.UtcNow;
    }

    // Called from main thread onHooks.OnScrimEnded
    // Snapshots stats then uploads
    public static void UploadPayload(string winner, string replayFilePath)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        var logger = ModContent.GetInstance<CTG2.CTG2>().Logger;

        if (!ModLoader.TryGetMod("PvPHub", out _))
        {
            logger.Warn("[MatchUpload] PvPHub is not loaded. skipping match upload.");
            return;
        }

        if (!StatsTracking.scrimsActive)
        {
            logger.Info("[MatchUpload] No scrim was being tracked. skipping match upload.");
            return;
        }

        if (winner != "red" && winner != "blue")
        {
            logger.Warn($"[MatchUpload] No valid queue winner (\"{winner ?? "null"}\"). skipping match upload.");
            return;
        }

        DateTime end = DateTime.UtcNow;
        DateTime start = MatchStartUtc ?? end;
        MatchStartUtc = null;

        List<PlayerSnapshot> players = SnapshotPlayers(winner, logger);
        if (players.Count == 0)
        {
            logger.Warn("[MatchUpload] No scrim players with Steam ids found; skipping match upload.");
            return;
        }

        try
        {
            PvPHubUploader.Upload(start, end, winner, players, replayFilePath);
        }
        catch (Exception e) // catch rather than crashing tmodloader if the PvPHub API changed
        {
            logger.Error("[MatchUpload] Failed to upload match to PvPHub. Is PvPHub updated? " + e);
        }
    }

    private sealed class PlayerSnapshot
    {
        public ulong SteamId;
        public string Name;
        public uint Team;
        public int Kills;
        public int Deaths;
        public uint Damage;
        public uint DamageTaken;
        public bool Winner;
    }

    private static List<PlayerSnapshot> SnapshotPlayers(string winner, log4net.ILog logger)
    {
        var players = new List<PlayerSnapshot>();

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player == null || !player.active)
                continue;

            if (player.team != TeamRed && player.team != TeamBlue)
                continue; // spectators and unassigned players aren't part of the match
                // TODO: also check if they are in the live roster before uploading

            if (!SteamAuth.steamIDMapping.TryGetValue(i, out ulong steamId) &&
                !SteamAuth.TryGetSteamId(player, out steamId))
            {
                logger.Warn($"[MatchUpload] No Steam id for {player.name}; they will be omitted from the match payload.");
                continue;
            }

            steamId = GetUploadSteamId(steamId, i);

            StatsTracking stats = player.GetModPlayer<StatsTracking>();
            string playerTeam = player.team == TeamRed ? "red" : "blue";

            players.Add(new PlayerSnapshot
            {
                SteamId = steamId,
                Name = player.name,
                Team = player.team == TeamRed ? PayloadTeamRed : PayloadTeamBlue,
                Kills = stats.accumKills,
                Deaths = stats.accumDeaths,
                Damage = (uint)Math.Max(0, stats.accumDamage),
                DamageTaken = (uint)Math.Max(0, stats.accumDamageTaken),
                Winner = playerTeam == winner
            });
        }

        return players;
    }

    private static ulong GetUploadSteamId(ulong steamId, int whoAmI)
    {
        if ((steamId & DebugSteamIdMask) == 0)
            return steamId;

        return (steamId - (ulong)whoAmI) & ~DebugSteamIdMask;
    }


    [ExtendsFromMod("PvPHub")]
    [JITWhenModsEnabled("PvPHub")]
    private static class PvPHubUploader
    {
        internal static void Upload(DateTime start, DateTime end, string winner, List<PlayerSnapshot> players, string replayFilePath)
        {
            var payloadPlayers = new Dictionary<ulong, global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.MatchPlayerPayload>();
            foreach (PlayerSnapshot player in players)
            {
                // CHECK (value > 0) a 0 stat must be omitted, not sent
                var stats = new Dictionary<string, uint>();
                if (player.Damage > 0)
                    stats["damage"] = player.Damage;
                if (player.DamageTaken > 0)
                    stats["damageTaken"] = player.DamageTaken;

                payloadPlayers[player.SteamId] = new global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.MatchPlayerPayload(
                    player.Name,
                    player.Team,
                    Reward: 10, // Amount of gems to reward
                    player.Kills,
                    player.Deaths,
                    player.Winner,
                    stats,
                    new Dictionary<string, IDictionary<int, uint>>());
            }

            var payload = new global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.MatchPayload(
                start,
                end,
                "ctg",
                payloadPlayers,
                new Dictionary<string, string>
                {
                    ["winner"] = winner,
                    ["source"] = "ctg2"
                },
                new List<global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.MatchTeamPayload?>());

            var logger = ModContent.GetInstance<CTG2.CTG2>().Logger;
            bool hasReplay = !string.IsNullOrWhiteSpace(replayFilePath) && File.Exists(replayFilePath);
            if (!hasReplay)
                logger.Warn($"[MatchUpload] Reese replay file not found (\"{replayFilePath ?? "null"}\"); uploading match without a replay.");

            Task.Run(async () =>
            {
                try
                {
                    var result = hasReplay
                        ? await global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.PostOfficialMatchV2Async(payload, replayFilePath)
                        : await global::PvPHub.Common.MainMenu.API.MatchHistory.MatchApi.PostOfficialMatchAsync(payload);

                    if (result.IsSuccess)
                        logger.Info($"[MatchUpload] Match uploaded to PvPHub. winner={winner} players={players.Count} replay={hasReplay}");
                    else
                        logger.Warn($"[MatchUpload] PvPHub rejected the match upload: {result.ErrorMessage} ({result.RequestSummary})");
                }
                catch (Exception e)
                {
                    logger.Error("[MatchUpload] Match upload to PvPHub failed: " + e);
                }
            });
        }
    }
}
