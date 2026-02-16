using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using CTG2.Content.Items;

namespace CTG2.Content.Classes
{
    public class AllNpcs : GlobalNPC
    {
        public int team = 3;
        private int previousLife = 0;

        SoundStyle totemCrumble = new SoundStyle("CTG2/Content/Classes/TotemCrumble");

        public override bool InstancePerEntity => true;

        public override void AI(NPC npc)
        {
            if (npc.life <= 0 && previousLife > 0) //use this for future sounds to play ondeath for all npcs that decay (no death reason)
            {
                if (npc.type == ModContent.NPCType<TikiTotem>())
                    SoundEngine.PlaySound(totemCrumble.WithVolumeScale(Main.soundVolume * 3f), npc.Center);
                else if (npc.type == ModContent.NPCType<StationaryBeast>())
                    SoundEngine.PlaySound(SoundID.NPCDeath1, npc.Center);
            }

            previousLife = npc.life;
        }
    }


    public class TikiTotem : ModNPC
    {
        private float healFrameGap = 30;
        private int hitCounter = 0;
        private float frameCount = 0;
        private int totemTeam = 0;
        private int maxHP = 600;

        private bool spawnPositionRecorded = false;
        private Vector2 spawnPosition = Vector2.Zero;

        SoundStyle totemCrumble = new SoundStyle("CTG2/Content/Classes/TotemCrumble");


        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }


        public override void SetDefaults()
        {   
            totemTeam = (int)NPC.ai[0];
            NPC.width = 32;
            NPC.height = 48;
            NPC.damage = 0; 
            NPC.defense = 0;
            NPC.lifeMax = maxHP;
            NPC.knockBackResist = 0; //make this higher for more knockback

            NPC.aiStyle = -1; 
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.chaseable = false;
        }
        
        
        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
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


        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            int tikiTeam = (int)NPC.ai[0];

            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];

                if (tikiTeam == player.team)
                    return false;
            }

            return true;
        }


        public override bool? CanBeHitByItem (Player player, Item item)
        {
            int tikiTeam = (int)NPC.ai[0];

            if (player.whoAmI >= 0 && player.whoAmI < Main.maxPlayers && tikiTeam == player.team)
                return false;

            return true;
        }


        public override void HitEffect(NPC.HitInfo hit)
        {
            int tikiTeam = (int)NPC.ai[0];

            if (NPC.life <= 0)
            {
                if (tikiTeam == 1)
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 90);
                else if (tikiTeam == 1)
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 88);
                
                SoundEngine.PlaySound(totemCrumble.WithVolumeScale(Main.soundVolume * 3f), NPC.Center);
            }
            else
            {
                if (tikiTeam == 1)
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 60);
                else if (tikiTeam == 1)
                    for (int i = 0; i < 5; i++)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, 59);
            }

            SoundEngine.PlaySound(SoundID.NPCHit16, NPC.Center);

            hitCounter = 1;
        }


        public override void AI()
        {
            if (frameCount % 2 == 0) NPC.life--;

            float friction = 0f; //update this to change friction

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

            if (!spawnPositionRecorded)
            {
                spawnPositionRecorded = true;
                spawnPosition = NPC.Center;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            NPC.Center = spawnPosition;
            NPC.velocity = Vector2.Zero;

            foreach (Player player in Main.player)
            {
                if (!player.active || player.dead)
                    continue;
                // ai[0] stores tiki's team
                if (player.team != (int)NPC.ai[0])
                    continue;

                if (Vector2.Distance(NPC.Center, player.Center) <= 14 * 16 && frameCount % healFrameGap == 0) // 14 block radius
                {
                    player.Heal(1);
                }
            }

            frameCount++;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
            Color teamColar = Color.Gray;

            int tikiTeam = (int)NPC.ai[0];
            if (hitCounter > 0 && hitCounter <= 50)
            {
                if (tikiTeam == 1)
                    teamColar = new Color(255, hitCounter * 5, hitCounter * 5, 155 + hitCounter * 2);
                else if (tikiTeam == 3)
                    teamColar = new Color(hitCounter * 5, hitCounter * 5, 255, 155 + hitCounter * 2);
            }
            else if (hitCounter > 50)
            {
                if (tikiTeam == 1)
                    teamColar = new Color(255, 250 - (hitCounter - 50) * 5, 250 - (hitCounter - 50) * 5, 255 - (hitCounter - 50) * 2);
                else if (tikiTeam == 3)
                    teamColar = new Color(250 - (hitCounter - 50) * 5, 250 - (hitCounter - 50) * 5, 255, 255 - (hitCounter - 50) * 2);
            }
            else
            {
                if (tikiTeam == 1)
                    teamColar = new Color(255, 0, 0, 155);
                else if (tikiTeam == 3)
                    teamColar = new Color(0, 0, 255, 155);
            }

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

            if (hitCounter > 0 && hitCounter < 100) hitCounter++;
            else if (hitCounter >= 100) hitCounter = 0;
        }
    }
}
