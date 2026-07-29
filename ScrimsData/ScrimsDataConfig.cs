using System;
using System.IO;
using System.Text.Json;
using CTG2.Content.Configs;
using Terraria;
using Terraria.ModLoader;

namespace CTG2.ScrimsData;


public static class ScrimsDataConfig
{
    private const string UploadUrlEnvVar = "CTG2_SCRIMSDATA_UPLOAD_URL";
    private const string ApiKeyEnvVar = "CTG2_SCRIMSDATA_API_KEY";
    private const string SecretsFileName = "ctg2-api-secrets.json";

    private static bool _secretsLoaded;
    private static string _secretsApiKey;

    public static string UploadUrl
    {
        get
        {
            string envUrl = GetEnvString(UploadUrlEnvVar);
            if (envUrl != null)
                return envUrl;

            return ModContent.GetInstance<ServerConfig>()?.ScrimsDataUploadUrl?.Trim() ?? "";
        }
    }

    public static string ApiKey
    {
        get
        {
            string envKey = GetEnvString(ApiKeyEnvVar);
            if (envKey != null)
                return envKey;

            return LoadApiKeyFromSecretsFile() ?? "";
        }
    }

    // Called when a new scrims queue starts so a freshly added key is picked up without a restart.
    public static void InvalidateCache()
    {
        _secretsLoaded = false;
        _secretsApiKey = null;
    }

    private static string LoadApiKeyFromSecretsFile()
    {
        if (_secretsLoaded)
            return _secretsApiKey;

        _secretsLoaded = true;
        _secretsApiKey = null;

        string secretsPath = Path.Combine(Main.SavePath, "CTG2", SecretsFileName);
        if (!File.Exists(secretsPath))
            return null;

        try
        {
            string json = File.ReadAllText(secretsPath);
            ScrimsDataSecrets secrets = JsonSerializer.Deserialize<ScrimsDataSecrets>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!string.IsNullOrWhiteSpace(secrets?.ScrimsDataApiKey))
                _secretsApiKey = secrets.ScrimsDataApiKey.Trim();
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<CTG2>().Logger.Warn(
                $"[ScrimsData] Failed to read secrets file at {secretsPath}: {ex.Message}");
        }

        return _secretsApiKey;
    }

    private static string GetEnvString(string key)
    {
        string value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private class ScrimsDataSecrets
    {
        public string ScrimsDataApiKey { get; set; }
    }
}
