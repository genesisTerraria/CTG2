using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Chat;
using System.Collections.Generic;
using System;
using Terraria.DataStructures;


namespace CTG2.Content.Functionality
{
    public class DoubleNPCDamage : ModPlayer
    {
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            modifiers.FinalDamage *= 2f;
        }
    }
    
    public class MobHP : GlobalNPC
    {
        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (npc.aiStyle == NPCAIStyleID.Slime)
                modifiers.FinalDamage *= 0.5f;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile proj, ref NPC.HitModifiers modifiers)
        {
            if (npc.aiStyle == NPCAIStyleID.Slime)
                modifiers.FinalDamage *= 0.5f;
        }
    }
}
