using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Music
{
    public class AreaMusicScene : ModSceneEffect
    {
        // Define your bounds in world coordinates (pixels)
        // Note: 1 tile = 16 pixels
        private static readonly Rectangle ArenaArea = new Rectangle(12208, 10704, 8304, 1312);
        private static readonly Rectangle BlueSelectionArea = new Rectangle(38640, 9344, 1712, 752);
        private static readonly Rectangle RedSelectionArea = new Rectangle(26608, 9424, 1712, 752);

        public override bool IsSceneEffectActive(Player player)
        {
            // Returns true if the player's center is inside the rectangle
            return ArenaArea.Contains(player.Center.ToPoint())
                || BlueSelectionArea.Contains(player.Center.ToPoint())
                || RedSelectionArea.Contains(player.Center.ToPoint());
        }

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Content/Music/First Star");

        // Optional: Give this music priority over standard biomes
        public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;
    }
}