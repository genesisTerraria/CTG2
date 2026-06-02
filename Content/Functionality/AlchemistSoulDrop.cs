using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2.Content.ClientSide;

namespace CTG2.Content.Functionality
{
    public class AlchemistSoulDrop : ModPlayer
    {
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!pvp) return;

            int killerIndex = damageSource.SourcePlayerIndex;
            if (killerIndex < 0 || killerIndex >= Main.maxPlayers) return;

            Player killer = Main.player[killerIndex];
            if (!killer.active) return;

            var killerManager = killer.GetModPlayer<PlayerManager>();
            int killerClass = killerManager.currentClass.AbilityID;

            if (killerClass == 3)
            {
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    new Vector2(0f, -0.1f), // tiny upward velocity to prevent instant death
                    ProjectileID.NebulaBlaze2,
                    0,
                    0f
                );
            }
        }
    }
}