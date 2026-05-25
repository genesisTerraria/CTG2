using CTG2.Content.Buffs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class OverloadedMountsPlayer : ModPlayer
{
    private bool lastActive = false;

    public override void PostUpdateMiscEffects()
    {
        bool active = Player.mount.Active && Player.mount.Type == MountID.Slime;

        if (active)
        {
            Player.AddBuff(BuffID.Dazed, 1);
            Player.gravity *= 0.65f;
        }
        else if (!active && lastActive)
        {
            Player.AddBuff(BuffID.Sunflower, 20);
            Player.AddBuff(BuffID.Swiftness, 20);
        }

        lastActive = active;
    }
}