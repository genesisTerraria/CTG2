using System;
using System.Net.Http;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace PvPHubIntegration;

// Quick pvphub patch to make payload timeouts more generous

[Autoload(Side = ModSide.Server)]
public class PvPHubTimeoutPatch : ModSystem
{
    private static readonly TimeSpan UploadTimeout = TimeSpan.FromMinutes(5);

    public override void PostSetupContent()
    {
        if (!Main.dedServ)
            return;

        if (!ModLoader.TryGetMod("PvPHub", out Mod pvpHub))
            return;

        var logger = ModContent.GetInstance<CTG2.CTG2>().Logger;

        try
        {
            Type apiClientType = pvpHub.Code.GetType("PvPHub.Common.MainMenu.API.ApiClient");
            FieldInfo clientField = apiClientType?.GetField("Client", BindingFlags.NonPublic | BindingFlags.Static);

            if (clientField?.GetValue(null) is not HttpClient client)
            {
                logger.Warn("[MatchUpload] Could not find PvPHub's ApiClient.Client field; replay uploads keep the 10s timeout.");
                return;
            }

            client.Timeout = UploadTimeout; // throws InvalidOperationException if a request already went out
            logger.Info($"[MatchUpload] Raised PvPHub's API timeout to {UploadTimeout.TotalMinutes:0} minutes so replay uploads can finish.");
        }
        catch (Exception e)
        {
            logger.Warn("[MatchUpload] Failed to raise PvPHub's API timeout; replay uploads may still time out. " + e);
        }
    }
}
