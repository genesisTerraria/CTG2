using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Chat;
using System.Collections.Generic;
using System;


namespace CTG2.Content.Functionality
{
    public class DoubleNPCDamage : ModPlayer
    {
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= 2f;
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (proj.hostile)
                modifiers.FinalDamage *= 2f;
        }
    }
}
