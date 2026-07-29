using System;
using System.Collections.Generic;
using System.Text;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;
using PvPHubIntegration;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.ScrimsData;

// Lives entirely on the main thread
// Only the finished DTO snapshot ever crosses onto a worker task
// While pvphub statstracking uploads payload after a queue. This is uploaded
// after every round.
public static class ScrimsDataTracker
{
    private const int TeamRed = 1;
    private const int TeamBlue = 3;

    // set by UpdateGame before it calls EndGame
    // These purposefully do NOT match Terraria team ids
    private const int WinnerNone = 0;
    private const int WinnerBlue = 1;
    private const int WinnerRed = 2;

    private const string UnknownClassName = "Unknown";

    // Minimum time (minutes) required for a match to be sent in the payload when there's a tie
    // Games that end in a tie and are less than this wont be counted
    private const int MinEndedRoundTicks = 4 * 60 * 60; 

    // Round counter for the current scrims queue
    // Increments by 1 per uploaded round
    private static int _roundsCaptured;

    // used for the roundId only when NeatQueue never provided a queue name
    private static DateTime _queueStartUtc;

    private static string _lastCapturedRoundId;

    // Gate for whether the round that is ending should be uploaded.
    public static bool ShouldTrackStats()
    {
        if (Main.netMode != NetmodeID.Server)
            return false;

        if (!StatsTracking.scrimsActive)
            return false;

        GameManager gameManager = ModContent.GetInstance<GameManager>();
        return gameManager.IsGameActive
            && gameManager.RealMatch
            && gameManager.HasRoundStarted;
    }

    // Called from Hooks.OnFullRosterJoined 
    public static void OnQueueStarted()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        _roundsCaptured = 0;
        _queueStartUtc = DateTime.UtcNow;
        _lastCapturedRoundId = null;
        ScrimsDataConfig.InvalidateCache();

        ModContent.GetInstance<CTG2>().Logger.Info("[ScrimsData] Scrims queue started; round counter reset.");
    }

    // Called from Hooks.OnScrimEnded 
    public static void OnQueueEnded()
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        _roundsCaptured = 0;
        _lastCapturedRoundId = null;

        ModContent.GetInstance<CTG2>().Logger.Info("[ScrimsData] Scrims queue ended; round counter cleared.");
    }

    // winnerCode is GameManager.winner
    public static void CaptureAndQueueRound(GameManager gameManager, int winnerCode)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        var logger = ModContent.GetInstance<CTG2>().Logger;

        if (!ShouldTrackStats())
        {
            if (StatsTracking.scrimsActive)
            {
                logger.Info("[ScrimsData] Round not tracked: " +
                    $"gameActive={gameManager.IsGameActive}, realMatch={gameManager.RealMatch}, roundStarted={gameManager.HasRoundStarted}.");
            }
            return;
        }

        if (winnerCode != WinnerBlue && winnerCode != WinnerRed)
        {
            int playedTicks = gameManager.MatchTime - gameManager.matchStartTime;
            if (playedTicks < MinEndedRoundTicks)
            {
                logger.Warn($"[ScrimsData] Round not tracked: /end after only {playedTicks / 60}s of play (winner={winnerCode}).");
                return;
            }

            if (gameManager.blueCaptures > gameManager.redCaptures)
                winnerCode = WinnerBlue;
            else if (gameManager.redCaptures > gameManager.blueCaptures)
                winnerCode = WinnerRed;
            else
            {
                // Winner is the team carrying a gem since /end here means an admin forfeited
                // Maybe make a forfeit command in the future
                // Both teams carrying (or neither) stays a tie.
                bool blueCarrying = IsTeamCarryingGem(gameManager, TeamBlue);
                bool redCarrying = IsTeamCarryingGem(gameManager, TeamRed);

                if (blueCarrying && !redCarrying)
                    winnerCode = WinnerBlue;
                else if (redCarrying && !blueCarrying)
                    winnerCode = WinnerRed;
            }

            logger.Info($"[ScrimsData] /end after {playedTicks / 60}s of play; uploading with " +
                $"winner={WinnerCodeToTeamName(winnerCode)} (captures {gameManager.blueCaptures}-{gameManager.redCaptures}).");
        }

        List<ScrimsPlayerRoundStats> players = SnapshotPlayers(gameManager, winnerCode, logger);
        if (players.Count == 0)
        {
            logger.Warn("[ScrimsData] No valid red/blue participants at round end; round not uploaded.");
            return;
        }

        var queueSystem = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();
        string queueName = queueSystem.CurrentQueueName;
        int? matchNumber = queueSystem.CurrentMatchNumber;

        int roundNumber = _roundsCaptured + 1;
        string roundId = BuildRoundId(roundNumber, queueName, matchNumber);

        if (roundId == _lastCapturedRoundId)
        {
            logger.Warn($"[ScrimsData] Duplicate round capture suppressed for roundId={roundId}");
            return;
        }

        var payload = new ScrimsRoundPayload
        {
            RoundId = roundId,
            QueueName = string.IsNullOrWhiteSpace(queueName) ? null : queueName,
            MatchNumber = matchNumber?.ToString(),
            RoundNumber = roundNumber,
            EndedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
            WinningTeam = WinnerCodeToTeamName(winnerCode),
            Players = players
        };

        _roundsCaptured = roundNumber;
        _lastCapturedRoundId = roundId;

        logger.Info($"[ScrimsData] Captured round payload: roundId={roundId}, players={players.Count}, winner={payload.WinningTeam}");

        ScrimsDataUpload.QueueUpload(payload);
    }

    private static List<ScrimsPlayerRoundStats> SnapshotPlayers(GameManager gameManager, int winnerCode, log4net.ILog logger)
    {
        var players = new List<ScrimsPlayerRoundStats>();
        var queueSystem = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player == null || !player.active)
                continue;

            if (player.team != TeamRed && player.team != TeamBlue)
                continue;

            if (gameManager.IsPlayerSpectator(i))
                continue;

            PlayerManager playerManager = player.GetModPlayer<PlayerManager>();

            string className = playerManager.currentClass?.Name;
            if (string.IsNullOrWhiteSpace(className))
            {
                logger.Warn($"[ScrimsData] No server-side class resolved for {player.name}; uploading className={UnknownClassName}.");
                className = UnknownClassName;
            }

            string discordId = null;
            string discordUsername = null;
            if (queueSystem.TryGetDiscordIdentity(i, out string mappedId, out string mappedUsername))
            {
                discordId = mappedId;
                discordUsername = string.IsNullOrWhiteSpace(mappedUsername) ? null : mappedUsername;
            }
            else
            {
                logger.Warn($"[ScrimsData] No Discord identity for {player.name}; uploading with playerName only.");
            }

            players.Add(new ScrimsPlayerRoundStats
            {
                DiscordId = discordId,
                DiscordUsername = discordUsername,
                PlayerName = player.name,
                WonRound = DidPlayerWinRound(player.team, winnerCode),
                ClassName = className,
                Kills = playerManager.kills,
                Deaths = playerManager.deaths,
                LavaDeaths = playerManager.lavaDeaths,
                GemPickups = playerManager.gemPickups,
                GemCaptures = playerManager.gemCaptures,
                DamageDealt = playerManager.damage,
                DamageTaken = playerManager.damageTaken
            });
        }

        return players;
    }


    // True when any held gem's carrier is on the given team
    private static bool IsTeamCarryingGem(GameManager gameManager, int team)
    {
        return GemCarrierTeam(gameManager.BlueGem) == team
            || GemCarrierTeam(gameManager.RedGem) == team;
    }

    // Terraria team of the gem's carrier, or -1 when the gem isn't held
    private static int GemCarrierTeam(Gem gem)
    {
        if (gem == null || !gem.IsHeld)
            return -1;

        if (gem.HeldBy < 0 || gem.HeldBy >= Main.maxPlayers)
            return -1;

        Player holder = Main.player[gem.HeldBy];
        if (holder == null || !holder.active)
            return -1;

        return holder.team;
    }

    private static bool DidPlayerWinRound(int playerTeam, int winnerCode)
    {
        return (winnerCode == WinnerBlue && playerTeam == TeamBlue)
            || (winnerCode == WinnerRed && playerTeam == TeamRed);
    }

    private static string WinnerCodeToTeamName(int winnerCode)
    {
        return winnerCode switch
        {
            WinnerBlue => "blue",
            WinnerRed => "red",
            _ => "none"
        };
    }
    
    // Fallback in case queue num isn't sent
    private static string BuildRoundId(int roundNumber, string queueName, int? matchNumber)
    {
        string queueSegment = SanitizeIdSegment(queueName);
        if (queueSegment.Length == 0)
            queueSegment = "queue";

        string matchSegment = matchNumber.HasValue
            ? matchNumber.Value.ToString()
            : $"adhoc-{_queueStartUtc:yyyyMMddHHmmss}";

        return $"ctg2/{queueSegment}/{matchSegment}/{roundNumber}";
    }

    private static string SanitizeIdSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        foreach (char c in value.Trim().ToLowerInvariant())
            builder.Append(char.IsLetterOrDigit(c) ? c : '-');

        return builder.ToString().Trim('-');
    }

    // Mod reload cleanup for the static state in this subsystem
    internal static void Reset()
    {
        _roundsCaptured = 0;
        _queueStartUtc = default;
        _lastCapturedRoundId = null;
    }
}

public class ScrimsDataSystem : ModSystem
{
    public override void Unload()
    {
        ScrimsDataTracker.Reset();
        ScrimsDataUpload.Unload();
        ScrimsDataConfig.InvalidateCache();
    }
}
