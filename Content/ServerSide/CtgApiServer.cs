using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CTG2.Content.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.ServerSide
{
    public class CtgApiServer : ModSystem
    {
        private const string TokenHeader = "X-CTG2-Token";
        private const int DefaultPort = 9071;
        private const string SecretsFileName = "ctg2-api-secrets.json";

        private HttpListener _listener;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _listenerTask;
        private string _token;
        private string _prefix;

        public override void OnWorldLoad()
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            StartServer();
        }

        public override void Unload()
        {
            StopServer();
        }

        private void StartServer()
        {
            if (_listener != null)
                return;

            ApiSettings settings = LoadSettings();
            if (!settings.Enabled)
            {
                Mod.Logger.Info("CTG2 API is disabled.");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.Token))
            {
                Mod.Logger.Warn("CTG2 API is enabled but no token is configured. The listener will not start.");
                return;
            }

            _token = settings.Token;
            _prefix = settings.Prefix;
            _cancellationTokenSource = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);

            try
            {
                _listener.Start();
                _listenerTask = Task.Run(() => ListenAsync(_cancellationTokenSource.Token));
                Mod.Logger.Info($"CTG2 API listening on {_prefix}");
            }
            catch (Exception ex)
            {
                Mod.Logger.Error($"CTG2 API failed to bind {_prefix}: {ex.Message}");
                StopServer();
            }
        }

        private void StopServer()
        {
            _cancellationTokenSource?.Cancel();

            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"CTG2 API listener shutdown error: {ex.Message}");
            }

            _listener = null;
            _listenerTask = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _token = null;
            _prefix = null;
        }

        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context = null;

                try
                {
                    context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context, cancellationToken), cancellationToken);
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Mod.Logger.Warn($"CTG2 API listener error: {ex.Message}");
                    if (context != null)
                        await WriteJsonAsync(context, 500, new { ok = false, error = "Internal server error." });
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                string method = context.Request.HttpMethod;
                string path = context.Request.Url?.AbsolutePath?.TrimEnd('/').ToLowerInvariant() ?? string.Empty;
                if (path.Length == 0)
                    path = "/";

                if (method == "GET" && path == "/health")
                {
                    await WriteJsonAsync(context, 200, new { ok = true, service = "CTG2 API" });
                    return;
                }

                if (method == "POST" && path == "/gamemode")
                {
                    if (!IsAuthorized(context.Request.Headers[TokenHeader]))
                    {
                        await WriteJsonAsync(context, 401, new { ok = false, error = "Unauthorized." });
                        return;
                    }

                    string body;
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
                        body = await reader.ReadToEndAsync(cancellationToken);

                    GamemodeRequest request;
                    try
                    {
                        request = JsonSerializer.Deserialize<GamemodeRequest>(body, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException)
                    {
                        await WriteJsonAsync(context, 400, new { ok = false, error = "Invalid JSON." });
                        return;
                    }

                    string mode = NormalizeMode(request?.Mode);
                    if (mode == null)
                    {
                        await WriteJsonAsync(context, 400, new { ok = false, error = "Invalid mode." });
                        return;
                    }

                    bool applied = await ApplyGamemodeOnMainThreadAsync(mode);
                    if (!applied)
                    {
                        await WriteJsonAsync(context, 400, new { ok = false, error = "Invalid mode." });
                        return;
                    }

                    await WriteJsonAsync(context, 200, new { ok = true, mode });
                    return;
                }

                if (method == "POST" && path == "/neatqueue/teams")
                {
                    if (!IsAuthorized(context.Request.Headers[TokenHeader]))
                    {
                        await WriteJsonAsync(context, 401, new { ok = false, error = "Unauthorized." });
                        return;
                    }

                    string body;
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
                        body = await reader.ReadToEndAsync(cancellationToken);

                    NeatQueueTeamsRequest request;
                    try
                    {
                        request = JsonSerializer.Deserialize<NeatQueueTeamsRequest>(body, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException)
                    {
                        await WriteJsonAsync(context, 400, new { ok = false, error = "Invalid JSON." });
                        return;
                    }

                    if (request?.Assignments == null)
                    {
                        await WriteJsonAsync(context, 400, new { ok = false, error = "Missing assignments array." });
                        return;
                    }

                    Mod.Logger.Info($"[NeatQueue] Received TEAMS_CREATED match=#{request.MatchNumber?.ToString() ?? "?"} queue={request.Queue ?? "?"} count={request.Assignments.Length}");

                    var systemAssignments = new List<NeatQueueTeamAssignmentSystem.NeatQueueAssignment>(request.Assignments.Length);
                    foreach (var dto in request.Assignments)
                    {
                        if (dto == null)
                            continue;
                        systemAssignments.Add(new NeatQueueTeamAssignmentSystem.NeatQueueAssignment
                        {
                            DiscordId = dto.DiscordId,
                            Name = dto.Name,
                            Team = dto.Team,
                            TeamNum = dto.TeamNum
                        });
                    }

                    int storedCount = 0;
                    int skippedCount = 0;
                    int assignedOnlineCount = 0;
                    Exception caught = null;
                    await Main.RunOnMainThread(() =>
                    {
                        try
                        {
                            var sys = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();
                            (storedCount, skippedCount) = sys.ReplaceAssignments(systemAssignments);
                            assignedOnlineCount = sys.TryAssignAllOnline();
                        }
                        catch (Exception ex)
                        {
                            caught = ex;
                        }
                    });

                    if (caught != null)
                    {
                        Mod.Logger.Error($"CTG2 API /neatqueue/teams main-thread error: {caught}");
                        await WriteJsonAsync(context, 500, new { ok = false, error = "Internal server error." });
                        return;
                    }

                    await WriteJsonAsync(context, 200, new
                    {
                        ok = true,
                        assignment_count = storedCount,
                        assigned_online_count = assignedOnlineCount,
                        skipped_count = skippedCount
                    });
                    return;
                }

                if (method == "POST" && path == "/neatqueue/clear-teams")
                {
                    if (!IsAuthorized(context.Request.Headers[TokenHeader]))
                    {
                        await WriteJsonAsync(context, 401, new { ok = false, error = "Unauthorized." });
                        return;
                    }

                    // Drain the body but don't require any specific shape.
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8))
                        _ = await reader.ReadToEndAsync(cancellationToken);

                    Mod.Logger.Info("[NeatQueue] Received clear-teams");

                    Exception caught = null;
                    await Main.RunOnMainThread(() =>
                    {
                        try
                        {
                            ModContent.GetInstance<NeatQueueTeamAssignmentSystem>().ClearAssignments();
                        }
                        catch (Exception ex)
                        {
                            caught = ex;
                        }
                    });

                    if (caught != null)
                    {
                        Mod.Logger.Error($"CTG2 API /neatqueue/clear-teams main-thread error: {caught}");
                        await WriteJsonAsync(context, 500, new { ok = false, error = "Internal server error." });
                        return;
                    }

                    await WriteJsonAsync(context, 200, new { ok = true, cleared = true });
                    return;
                }

                await WriteJsonAsync(context, 404, new { ok = false, error = "Not found." });
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"CTG2 API request error: {ex.Message}");
                await WriteJsonAsync(context, 500, new { ok = false, error = "Internal server error." });
            }
        }

        private async Task<bool> ApplyGamemodeOnMainThreadAsync(string mode)
        {
            bool applied = false;
            Exception caughtException = null;

            await Main.RunOnMainThread(() =>
            {
                try
                {
                    applied = ModContent.GetInstance<CTG2>().ApplyGamemodeChange(mode, "CTG2 HTTP API");
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
            });

            if (caughtException != null)
                throw caughtException;

            return applied;
        }

        private bool IsAuthorized(string providedToken)
        {
            if (string.IsNullOrEmpty(providedToken) || string.IsNullOrEmpty(_token))
                return false;

            byte[] providedBytes = Encoding.UTF8.GetBytes(providedToken);
            byte[] configuredBytes = Encoding.UTF8.GetBytes(_token);

            return providedBytes.Length == configuredBytes.Length &&
                CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
        }

        private static string NormalizeMode(string mode)
        {
            string normalizedMode = mode?.Trim().ToLowerInvariant();
            return normalizedMode == "pubs" || normalizedMode == "scrims" || normalizedMode == "rng"
                ? normalizedMode
                : null;
        }

        private async Task WriteJsonAsync(HttpListenerContext context, int statusCode, object payload)
        {
            string json = JsonSerializer.Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
            context.Response.Close();
        }

        private ApiSettings LoadSettings()
        {
            ServerConfig config = ModContent.GetInstance<ServerConfig>();

            bool enabled = GetEnvBool("CTG2_API_ENABLED") ?? config.ApiEnabled;
            int port = GetEnvPort("CTG2_API_PORT") ?? (IsValidPort(config.ApiPort) ? config.ApiPort : DefaultPort);
            string token = GetEnvString("CTG2_API_TOKEN") ?? LoadTokenFromSecretsFile();
            string hostPrefix = GetEnvString("CTG2_API_HOST_PREFIX") ?? config.ApiHostPrefix;
            string prefix = string.IsNullOrWhiteSpace(hostPrefix)
                ? $"http://*:{port}/"
                : hostPrefix.Trim();

            if (!prefix.EndsWith("/", StringComparison.Ordinal))
                prefix += "/";

            return new ApiSettings(enabled, port, token, prefix);
        }

        private string LoadTokenFromSecretsFile()
        {
            string secretsPath = GetSecretsPath();

            if (!Directory.Exists(Path.GetDirectoryName(secretsPath)) || !File.Exists(secretsPath))
            {
                Mod.Logger.Warn($"CTG2 API token not found. Expected secrets file at: {secretsPath}");
                return null;
            }

            try
            {
                string json = File.ReadAllText(secretsPath);
                ApiSecrets secrets = JsonSerializer.Deserialize<ApiSecrets>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (string.IsNullOrWhiteSpace(secrets?.ApiToken))
                {
                    Mod.Logger.Warn($"CTG2 API secrets file has a blank ApiToken. Expected secrets file at: {secretsPath}");
                    return null;
                }

                return secrets.ApiToken.Trim();
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"Failed to read CTG2 API secrets file at {secretsPath}: {ex.Message}");
                return null;
            }
        }

        private static string GetSecretsPath()
        {
            return Path.Combine(Main.SavePath, "CTG2", SecretsFileName);
        }

        private static string GetEnvString(string key)
        {
            string value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private bool? GetEnvBool(string key)
        {
            string value = GetEnvString(key);
            if (value == null)
                return null;

            if (bool.TryParse(value, out bool parsed))
                return parsed;

            switch (value.ToLowerInvariant())
            {
                case "1":
                case "yes":
                case "y":
                case "on":
                    return true;
                case "0":
                case "no":
                case "n":
                case "off":
                    return false;
                default:
                    Mod.Logger.Warn($"Ignoring invalid {key} value.");
                    return null;
            }
        }

        private bool IsValidPort(int port)
        {
            return port >= 1 && port <= 65535;
        }

        private int? GetEnvPort(string key)
        {
            string value = GetEnvString(key);
            if (value == null)
                return null;

            if (int.TryParse(value, out int port) && IsValidPort(port))
                return port;

            Mod.Logger.Warn($"Ignoring invalid {key} value.");
            return null;
        }

        private class ApiSecrets
        {
            public string ApiToken { get; set; }
        }

        private class GamemodeRequest
        {
            public string Mode { get; set; }
        }

        private class NeatQueueTeamsRequest
        {
            public string Action { get; set; }

            [JsonPropertyName("match_number")]
            public int? MatchNumber { get; set; }

            public string Queue { get; set; }
            public string Guild { get; set; }
            public string Channel { get; set; }
            public NeatQueueAssignmentDto[] Assignments { get; set; }
        }

        private class NeatQueueAssignmentDto
        {
            [JsonPropertyName("discord_id")]
            public string DiscordId { get; set; }

            public string Name { get; set; }
            public string Team { get; set; }

            [JsonPropertyName("team_num")]
            public int? TeamNum { get; set; }

            public bool Captain { get; set; }
            public bool Picked { get; set; }
        }

        private readonly struct ApiSettings
        {
            public ApiSettings(bool enabled, int port, string token, string prefix)
            {
                Enabled = enabled;
                Port = port;
                Token = token;
                Prefix = prefix;
            }

            public bool Enabled { get; }
            public int Port { get; }
            public string Token { get; }
            public string Prefix { get; }
        }
    }
}
