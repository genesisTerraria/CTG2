using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Localization;
using CTG2.Content.Items;

namespace CTG2.Content.Tiles
{
    public class UniversalCraftingTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            // name.SetDefault("Universal Crafting Station");
            AddMapEntry(new Color(200, 200, 200), name);

            AdjTiles = new int[]
            {
                TileID.WorkBenches,
                TileID.Sawmill,
                TileID.MythrilAnvil,
                TileID.AdamantiteForge,
                TileID.Loom,
                TileID.CookingPots,
                TileID.TinkerersWorkbench,
                TileID.ImbuingStation,
                TileID.DyeVat,
                TileID.HeavyWorkBench,
                TileID.CrystalBall,
                TileID.Autohammer,
                TileID.LunarCraftingStation,
                TileID.Campfire,
                TileID.Kegs,
                TileID.TeaKettle,
                TileID.Blendomatic,
                TileID.MeatGrinder,
                TileID.Solidifier,
                TileID.SkyMill,
                TileID.LivingLoom,
                TileID.IceMachine,
                TileID.HoneyDispenser,
                TileID.GlassKiln,
                TileID.BoneWelder,
                TileID.LesionStation,
                TileID.FleshCloningVat,
                TileID.SteampunkBoiler,
                TileID.LihzahrdFurnace,
                TileID.WaterFountain
            };
            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
            MineResist = 1f;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 48, 48), ModContent.ItemType<UniversalCraftingItem>());
        }

        public override string Texture => "Terraria/Images/MagicPixel";
    }
}
