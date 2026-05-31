using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2.Content.ClientSide;

namespace CTG2.Content
{
    public class UnbreakableTiles : GlobalTile
    {
        public static bool AllowBreaking = false;

        private static bool IsAlwaysBreakableTile(int type)
        {
            return type == TileID.Dirt
                || type == TileID.Bubble
                || type == TileID.Mud
                || type == TileID.Grass
                || type == TileID.BreakableIce
                || type == 127
                || type == TileID.Rope;
        }

        // public override bool CanPlace(int i, int j, int type)
        // {
        //     Tile existing = Main.tile[i, j];

        //     //prevent block replace
        //     if (existing.HasTile)
        //         return false;

        //     return base.CanPlace(i, j, type);
        // }

        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            if (AllowBreaking || IsAlwaysBreakableTile(type))
            {
                return base.CanKillTile(i, j, type, ref blockDamaged);
            }
            else
            {
                return false;
            }
        }

        public override bool CanExplode(int i, int j, int type)
        {
            if (AllowBreaking || type == TileID.Dirt || type == TileID.Mud || type == TileID.Grass || type == TileID.BreakableIce || type == 127 || type == TileID.Rope)
            {
                return base.CanExplode(i, j, type);
            }
            else if (type == TileID.Bubble)
            {
                return !ProtectedRegionTile.IsInProtectedRegion(i, j);
            }
            else
            {
                return false;
            }
        }
    }

    public class NoExplosionWall : GlobalWall
    {
        public override bool CanExplode(int i, int j, int type)
        {
            return false;
        }
    }
    
}
