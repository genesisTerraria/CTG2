using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CTG2.ScrimsData;

// Fire-and-forget JSON POST of a finished round payload, modeled on the
// PvPHub MatchUpload flow: serialize on the main thread, upload on a worker
// task, and never let a backend problem touch the game.
public static class ScrimsDataUpload
{
    private static readonly object ClientLock = new();
    private static HttpClient _client;

    // Round ids that have been queued (in flight or completed) this session.
    // Guards against double uploads if the same round is ever captured twice.
    private static readonly ConcurrentDictionary<string, byte> QueuedRoundIds = new();

    // Called on the main thread with a fully built payload. Does no network work itself.
    public static void QueueUpload(ScrimsRoundPayload payload)
    {
        var logger = ModContent.GetInstance<CTG2>().Logger;

        if (payload == null || string.IsNullOrWhiteSpace(payload.RoundId))
        {
            logger.Warn("[ScrimsData] QueueUpload called without a valid payload/roundId; nothing uploaded.");
            return;
        }

        string uploadUrl = ScrimsDataConfig.UploadUrl;
        if (string.IsNullOrWhiteSpace(uploadUrl))
        {
            logger.Warn($"[ScrimsData] Upload disabled: no upload URL configured. Payload created for roundId={payload.RoundId} but not sent.");
            return;
        }

        if (!QueuedRoundIds.TryAdd(payload.RoundId, 0))
        {
            logger.Warn($"[ScrimsData] Duplicate upload suppressed for roundId={payload.RoundId}");
            return;
        }

        string json;
        try
        {
            json = JsonSerializer.Serialize(payload);
        }
        catch (Exception ex)
        {
            QueuedRoundIds.TryRemove(payload.RoundId, out _);
            logger.Error($"[ScrimsData] Failed to serialize round payload for roundId={payload.RoundId}: {ex}");
            return;
        }

        string apiKey = ScrimsDataConfig.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            logger.Info("[ScrimsData] No API key configured; uploading without an Authorization header.");

        string roundId = payload.RoundId;
        int playerCount = payload.Players?.Count ?? 0;

        Task.Run(() => UploadAsync(uploadUrl, apiKey, json, roundId, playerCount));
    }

    private static async Task UploadAsync(string uploadUrl, string apiKey, string json, string roundId, int playerCount)
    {
        var logger = ModContent.GetInstance<CTG2>().Logger;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using HttpResponseMessage response = await GetClient().SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                logger.Info($"[ScrimsData] Uploaded round successfully: roundId={roundId}, players={playerCount}, status={(int)response.StatusCode}");
                return;
            }

            string body = await ReadBodySafeAsync(response);
            logger.Warn($"[ScrimsData] Upload failed: roundId={roundId}, status={(int)response.StatusCode}, response={body}");
            QueuedRoundIds.TryRemove(roundId, out _);
        }
        catch (Exception ex)
        {
            // Single attempt only; the backend dedupes by roundId so a lost round is recoverable later.
            logger.Error($"[ScrimsData] Upload failed: roundId={roundId}, exception: {ex}");
            QueuedRoundIds.TryRemove(roundId, out _);
        }
    }

    private static async Task<string> ReadBodySafeAsync(HttpResponseMessage response)
    {
        try
        {
            string body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return "(empty)";

            return body.Length > 500 ? body.Substring(0, 500) + "..." : body;
        }
        catch
        {
            return "(unreadable)";
        }
    }

    private static HttpClient GetClient()
    {
        lock (ClientLock)
        {
            _client ??= new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            return _client;
        }
    }

    // Called from ScrimsDataSystem.Unload so mod reloads don't leak the client or the dedupe set.
    public static void Unload()
    {
        lock (ClientLock)
        {
            _client?.Dispose();
            _client = null;
        }

        QueuedRoundIds.Clear();
    }
}
