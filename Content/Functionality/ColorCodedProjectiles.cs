
// using Terraria.ModLoader.IO;
// using Terraria;
// using Terraria.ModLoader;
// using Terraria.ID;
// using Terraria.ModLoader.IO;
// using Microsoft.Xna.Framework;
// using Terraria.DataStructures;
// using Microsoft.Xna.Framework.Graphics;
// using Terraria.GameContent;
// using Microsoft.Xna.Framework.Audio;
// using Terraria.Audio;
// using CTG2.Content.Items;
// using System.IO;
// using CTG2.Content.Configs;



// namespace CTG2.Content
// {
//     public class ColorCodedProjectiles : GlobalProjectile
//     {
//         public override bool InstancePerEntity => true;

//         public sbyte OwnerTeam = -1;

//         public override void OnSpawn(Projectile p, IEntitySource s)
//         {
//             if (Main.netMode == NetmodeID.Server)
//             {
//                 int o = p.owner;
//                 if (o >= 0 && o < Main.maxPlayers && Main.player[o].active)
//                     OwnerTeam = (sbyte)Main.player[o].team;
//                 p.netUpdate = true; 
//             }
//         }

//         public override void SendExtraAI(Projectile p, BitWriter _, BinaryWriter w) => w.Write(OwnerTeam);
//         public override void ReceiveExtraAI(Projectile p, BitReader _, BinaryReader r) => OwnerTeam = r.ReadSByte();

//         public override bool PreDraw(Projectile projectile, ref Color lightColor)
//         {
//             var config = ModContent.GetInstance<CTG2Config>();
//             if (!config.EnableProjectileTeamColoring)
//                 return true;

//             sbyte team = projectile.GetGlobalProjectile<ColorCodedProjectiles>().OwnerTeam;
//             if (team < 0)
//             {
//                 int o = projectile.owner;
//                 team = (o >= 0 && o < Main.maxPlayers && Main.player[o].active)
//                     ? (sbyte)Main.player[o].team : (sbyte)0;
//             }

//             if (team == 0)
//                 return true;

//             if (projectile.type == 153 || projectile.type == 699 || projectile.type == 228 || projectile.type == 480
//                 || projectile.type == ModContent.ProjectileType<ChargedBowProjectile>()
//                 || projectile.type == ModContent.ProjectileType<AmalgamatedHandProjectile1>()
//                 || projectile.type == ModContent.ProjectileType<AmalgamatedHandProjectile2>()
//                 || projectile.type == 80)
//                 return true;

//             Texture2D texture = TextureAssets.Projectile[projectile.type].Value;
//             Rectangle rectangle = new(0, 0, texture.Width, texture.Height);
//             Vector2 origin = rectangle.Size() / 2f;
//             Vector2 drawPosition = projectile.position - Main.screenPosition
//                 + new Vector2(projectile.width / 2f, projectile.height / 2f);

//             Color teamColor = Color.Gray;
//             if (team == 1)
//                 teamColor = new Color(255, 0, 0, 255);
//             else if (team == 3)
//                 teamColor = (projectile.type == ProjectileID.ThornChakram || projectile.type == ProjectileID.Flamarang
//                              || projectile.type == 15 || projectile.type == ProjectileID.Bananarang || projectile.type == 304)
//                     ? new Color(0, 0, 255, 255)
//                     : new Color(50, 50, 255, 255);

//             Main.EntitySpriteDraw(texture, drawPosition, rectangle, teamColor,
//                                   projectile.rotation, origin, projectile.scale, SpriteEffects.None);
//             return false; 
//         }
//     }
// }
