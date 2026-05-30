using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class TealCurrentTile : ModTile
    {
        public static readonly Color TealColor = new Color(0, 190, 180);

        public override string Texture => "Terraria/Images/MagicPixel";

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileMergeDirt[Type] = false;

            DustType = DustID.Water;
            HitSound = SoundID.Splash;

            AddMapEntry(TealColor, CreateMapEntryName());
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Rectangle tileArea = new Rectangle(
                (int)(i * 16 - Main.screenPosition.X + offScreen.X),
                (int)(j * 16 - Main.screenPosition.Y + offScreen.Y),
                16,
                16);

            Color drawColor = Color.Lerp(Lighting.GetColor(i, j), new Color(0, 210, 200, 190), 0.85f);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, tileArea, drawColor * 0.85f);

            Rectangle highlightArea = new Rectangle(tileArea.X, tileArea.Y, tileArea.Width, 3);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, highlightArea, new Color(150, 255, 245, 110) * 0.6f);

            return false;
        }
    }
}
