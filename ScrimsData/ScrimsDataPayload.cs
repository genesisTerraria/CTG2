using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CTG2.ScrimsData;

// built on the main thread in GameManager.EndGame(). Only these plain DTOs
// may be handed to the async uploader. This payload is sent for tracking scrims
// data and is sent every round unlike the pvphub payload that is sent OnScrimEnded().
public sealed class ScrimsRoundPayload
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    [JsonPropertyName("roundId")]
    public string RoundId { get; init; }

    [JsonPropertyName("queueName")]
    public string QueueName { get; init; }

    // Serialized as a string to match discordID handling (null when NeatQueue didn't provide one)
    [JsonPropertyName("matchNumber")]
    public string MatchNumber { get; init; }

    // What round within the current scrim (scrims are best of 3)
    [JsonPropertyName("roundNumber")]
    public int RoundNumber { get; init; }

    [JsonPropertyName("endedAtUtc")]
    public string EndedAtUtc { get; init; }

    // blue, red, or none when the round ended without a decided winner
    [JsonPropertyName("winningTeam")]
    public string WinningTeam { get; init; }

    [JsonPropertyName("players")]
    public List<ScrimsPlayerRoundStats> Players { get; init; }
}

public sealed class ScrimsPlayerRoundStats
{
    // Primary key in the SQL database and the main way of identifying the player
    // Null when the player has no synced Discord identity
    [JsonPropertyName("discordId")]
    public string DiscordId { get; init; }
    
    // Current discord username is appended but not used as a primary key because
    // player discord username can change. 
    [JsonPropertyName("discordUsername")]
    public string DiscordUsername { get; init; }

    // Current Terraria character name. Also appended to a list in the DB.
    [JsonPropertyName("playerName")]
    public string PlayerName { get; init; }

    [JsonPropertyName("wonRound")]
    public bool WonRound { get; init; }

    // Class that the player used in the round.
    [JsonPropertyName("className")]
    public string ClassName { get; init; }

    [JsonPropertyName("kills")]
    public int Kills { get; init; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; init; }

    [JsonPropertyName("lavaDeaths")]
    public int LavaDeaths { get; init; }

    [JsonPropertyName("gemPickups")]
    public int GemPickups { get; init; }

    [JsonPropertyName("gemCaptures")]
    public int GemCaptures { get; init; }

    [JsonPropertyName("damageDealt")]
    public long DamageDealt { get; init; }

    [JsonPropertyName("damageTaken")]
    public long DamageTaken { get; init; }
}
