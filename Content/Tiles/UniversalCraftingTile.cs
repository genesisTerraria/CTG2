using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.addTile(Type);

            AnimationFrameHeight = 72;

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

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            // Increases the frame counter by 1 every game tick
            if (++frameCounter >= 9) 
            {
                frameCounter = 0;
                // Cycles through 4 animation frames (0, 1, 2, 3)
                frame = ++frame % 4; 
            }
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 64, 64), ModContent.ItemType<UniversalCraftingItem>());
        }
    }
}
