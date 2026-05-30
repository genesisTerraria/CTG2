using CTG2.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class TealCurrentPlayer : ModPlayer
    {
        private const int BuffDuration = 30 * 60;

        public override void PostUpdate()
        {
            if (Player.dead || !Player.active)
            {
                return;
            }

            if (IsInsideTealCurrent())
            {
                Player.AddBuff(ModContent.BuffType<TealCurrentSpeed>(), BuffDuration);
            }
        }

        private bool IsInsideTealCurrent()
        {
            int minX = Player.Hitbox.Left / 16;
            int maxX = (Player.Hitbox.Right - 1) / 16;
            int minY = Player.Hitbox.Top / 16;
            int maxY = (Player.Hitbox.Bottom - 1) / 16;
            int tealCurrentType = ModContent.TileType<TealCurrentTile>();

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && tile.TileType == tealCurrentType)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
