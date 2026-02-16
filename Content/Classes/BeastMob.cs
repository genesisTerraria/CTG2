using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using CTG2.Content.Items;


namespace CTG2.Content.Classes
{
    public class StationaryBeast : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }


        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 27;
            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = 150;
            NPC.knockBackResist = 2f; //make this higher for more knockback

            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.chaseable = false;
        }


        public override void HitEffect (NPC.HitInfo hit)
        {
            SoundEngine.PlaySound(SoundID.NPCHit1, NPC.Center);

            if (NPC.life <= 0)
                SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
        }


        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) //might have to change values later
        {
            if (item.type != ModContent.ItemType<ShardstonePickaxe>())
                NPC.immune[player.whoAmI] = 40;
        }


        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                NPC.immune[projectile.owner] = 40;
            }
        }


        public override void AI()
        {

            float friction = 0.1f; //update this to change friction

            if (NPC.velocity.X > 0f)
            {
                NPC.velocity.X -= friction;
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X = 0f;
            }
            else if (NPC.velocity.X < 0f)
            {
                NPC.velocity.X += friction;
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X = 0f;
            }


            float gravity = 0.3f;
            float maxFallSpeed = 10f;

            NPC.velocity.Y += gravity;
            if (NPC.velocity.Y > maxFallSpeed)
                NPC.velocity.Y = maxFallSpeed;


            //Slimer team logic

            int beastTeam = (int)NPC.ai[0];

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (!player.active || player.dead || player.immune || player.team == beastTeam)
                    continue;

                if (NPC.Hitbox.Intersects(player.Hitbox) && player.team != beastTeam)
                {
                    int damage = 80;

                    player.Hurt(PlayerDeathReason.LegacyEmpty(), damage, 1, false, false, -1, true,
                    100f /* Armor Penetration (Keeps damage persistent for all classes) */, 0f, 4.5f // knockback
                );

                    player.immune = true;
                    player.immuneTime = 40; // Iframes after hit

                }
            }
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
            Color teamColar = Color.Gray;

            int slimerTeam = (int)NPC.ai[0];
            if (slimerTeam == 1){ teamColar = new Color(255, 0, 0, 180); }
            if (slimerTeam == 3){ teamColar = new Color(0, 0, 255, 180); }

                Vector2 drawPosition = NPC.Center - screenPos + new Vector2(0, 2.8f);

            spriteBatch.Draw(
                texture,
                drawPosition,
                frame,
                teamColar,
                NPC.rotation,
                origin,
                NPC.scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}
