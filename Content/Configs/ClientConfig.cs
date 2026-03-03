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

        [Label("Advanced Binoculars Zoom Speed (Seconds)")]
        [Tooltip("How long it takes for binocular camera to reach full offset.")]
        [DefaultValue(0.5f)]
        [Range(0f, 5f)]
        [Increment(0.1f)]
        public float CameraLerpSeconds;
    }
}