using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using System.IO;

namespace CTG2.Content.NPCs
{
    public class SlimerClone : ModNPC
    {
        public override string Texture => "Terraria/Images/NPC_" + NPCID.CorruptSlime;
        public int team = 0;
        public int spawner = -1;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.CorruptSlime];
            AnimationType = NPCID.CorruptSlime;
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.Slimer2); // Slimer2 is the wingless ground version
            AIType = NPCID.Slimer2;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter >= 6.0)
            {
                NPC.frameCounter = 0.0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                    NPC.frame.Y = 0;
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(team);
            writer.Write(spawner);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            team = reader.ReadInt32();
            spawner = reader.ReadInt32();
        }

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            // Never damage the owner
            return target.whoAmI != spawner;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (team == 1)
            {
                Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                Rectangle frame = NPC.frame;
                Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
                Color teamColor = new Color(255, 0, 0, 180);

                Vector2 drawPosition = NPC.Center - screenPos + new Vector2(0, 2.8f);

                spriteBatch.Draw(
                    texture,
                    drawPosition,
                    frame,
                    teamColor,
                    NPC.rotation,
                    origin,
                    NPC.scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}