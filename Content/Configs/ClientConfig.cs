using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace CTG2.Content.Configs
{
    public class CTG2Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // [Label("Enable clash royale OT music")]
        // [DefaultValue(false)]
        // public bool ClashRoyaleOTMusic;

        // [Label("Enable projectile team coloring")]
        // [DefaultValue(false)]
        // public bool EnableProjectileTeamColoring;


        // [Label("Selected Track: 0=None, 1=Clash, 2=Mystery")]
        // public int SelectedMusicIndex { get; set; } = 0;

        [Label("Advanced Binoculars Outward Zoom Speed (Seconds)")]
        [Tooltip("How long it takes for binocular camera to reach full offset.")]
        [DefaultValue(0.5f)]
        [Range(0f, 5f)]
        [Increment(0.05f)]
        public float CameraLerpSecondsOutward;

        [Label("Advanced Binoculars Inward Zoom Speed (Seconds)")]
        [Tooltip("How long it takes for binocular camera to return to centered position.")]
        [DefaultValue(0.5f)]
        [Range(0f, 5f)]
        [Increment(0.05f)]
        public float CameraLerpSecondsInward;

        [Label("Advanced Binoculars Camera Lock")]
        [Tooltip("If enabled, the first Advanced Binoculars press locks the camera angle in place.")]
        [DefaultValue(false)]
        public bool EnabledCameraLock;

        [Label("Local Player Team Outline")]
        [Tooltip("If enabled, the local player will have a team outline.")]
        [DefaultValue(true)]
        public bool EnabledLocalPlayerTeamOutline;

        [Label("Enable vanilla double-tap dash")]
        [Tooltip("When enabled, the vanilla double-tap dash works as normal. When disabled, only the Dash keybind triggers dashes.")]
        [DefaultValue(true)]
        public bool IsVanillaDashEnabled;
    }
}