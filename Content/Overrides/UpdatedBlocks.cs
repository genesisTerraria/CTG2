using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2.Content.ClientSide;

namespace CTG2.Content
{
    public class UnbreakableTiles : GlobalTile
    {
        public static bool AllowBreaking = false;
        public override bool CanPlace(int i, int j, int type)
        {
            Tile existing = Main.tile[i, j];

            //prevent block replace
            if (existing.HasTile)
                return false;

            return base.CanPlace(i, j, type);
        }

        public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
        {
            if (AllowBreaking)
            {
                return base.CanKillTile(i, j, type, ref blockDamaged);
            }
            else
            {
                if (type == TileID.LihzahrdBrick)
                    return false;

                if (type == TileID.Platforms)
                    return false;

                if (type == TileID.EchoBlock)
                    return false;

                if (type == TileID.CrystalBlock)
                    return false;

                if (type == TileID.PalladiumColumn)
                    return false;

                if (type == TileID.Grate)
                    return false;

                if (type == TileID.ConveyorBeltLeft)
                    return false;

                if (type == TileID.ConveyorBeltRight)
                    return false;

                if (type == TileID.IridescentBrick)
                    return false;

                if (type == TileID.HangingLanterns)
                    return false;

                if (type == TileID.Banners)
                    return false;

                if (type == TileID.AncientPinkBrick)
                    return false;

                if (type == TileID.PaintedArrowSign)
                    return false;

                if (type == TileID.MusicBoxes)
                    return false;

                if (type == TileID.AncientGreenBrick)
                    return false;

                if (type == TileID.MarbleColumn)
                    return false;

                if (type == TileID.AlphabetStatues)
                    return false;

                if (type == TileID.Statues)
                    return false;

                if (type == 128)
                    return false;

                if (type == TileID.ItemFrame)
                    return false;

                if (type == TileID.Signs)
                    return false;

                if (type == TileID.RubyGemspark)
                    return false;

                if (type == TileID.WaterFountain)
                    return false;

                if (type == TileID.Benches)
                    return false;

                if (type == TileID.SapphireGemspark)
                    return false;

                if (type == TileID.DynastyWood)
                    return false;

                if (type == TileID.PinkDungeonBrick)
                    return false;

                if (type == TileID.GreenDungeonBrick)
                    return false;

                if (type == TileID.DemoniteBrick)
                    return false;

                if (type == TileID.CrimstoneBrick)
                    return false;

                if (type == TileID.Candles)
                    return false;

                if (type == TileID.Asphalt)
                    return false;

                if (type == TileID.Campfire)
                    return false;

                if (type == TileID.WaterCandle)
                    return false;

                return base.CanKillTile(i, j, type, ref blockDamaged);
            }
        }

        public override bool CanExplode(int i, int j, int type)
        {

            if (type != TileID.Dirt && type != TileID.Bubble && type != TileID.Mud)
                return false;

            return base.CanExplode(i, j, type);
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
