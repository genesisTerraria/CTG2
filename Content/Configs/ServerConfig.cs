using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace CTG2.Content.Configs
{
    public class ServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Label("Enable CTG2 HTTP API")]
        [Tooltip("Enables the dedicated-server-only CTG2 HTTP API. CTG2_API_ENABLED overrides this value when set.")]
        [DefaultValue(false)]
        public bool ApiEnabled;

        [Label("CTG2 HTTP API Port")]
        [Tooltip("Port used by the CTG2 HTTP API. CTG2_API_PORT overrides this value when set.")]
        [DefaultValue(9071)]
        [Range(1, 65535)]
        public int ApiPort;

        [Label("CTG2 HTTP API Host Prefix")]
        [Tooltip("Optional HttpListener prefix. CTG2_API_HOST_PREFIX overrides this value when set. Leave blank to use http://*:<port>/.")]
        [DefaultValue("")]
        public string ApiHostPrefix;

        [Label("ScrimsData Upload URL")]
        [Tooltip("Backend endpoint that receives per-round scrims stats. Leave blank to disable uploads. CTG2_SCRIMSDATA_UPLOAD_URL overrides this value when set.")]
        [DefaultValue("")]
        public string ScrimsDataUploadUrl;
    }
}
