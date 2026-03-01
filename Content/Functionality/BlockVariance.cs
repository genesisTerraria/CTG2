using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;

public class DamageVariance : ModSystem
{

    public override void Load()
    {
        var damageVarMethodInfo = typeof(Main).GetMethod("DamageVar", new[] { typeof(float), typeof(int), typeof(float) });
        if (damageVarMethodInfo != null)
        {
            IL_Main.DamageVar_float_int_float += DisableDamageVariance_Hook;
        }
        else
        {
            Mod.Logger.Error("Could not find Main.DamageVar(float, float) method.");
        }

        //On_Main.DamageVar_float_int_float += RemoveVariance;

    }
    public override void Unload()
    {
        var damageVarMethodInfo = typeof(Main).GetMethod("DamageVar", new[] { typeof(float), typeof(int), typeof(float) });
        if (damageVarMethodInfo != null)
        {
            IL_Main.DamageVar_float_int_float -= DisableDamageVariance_Hook;
        }
        else
        {
            Mod.Logger.Error("Could not find Main.DamageVar(float, float) method.");
        }
        base.Unload();
    }
    private void DisableDamageVariance_Hook(ILContext il)
    {
        var c = new ILCursor(il);
        while (c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<Terraria.Utilities.UnifiedRandom>("Next")))
        {
            c.Emit(OpCodes.Pop);
            c.Emit(OpCodes.Ldc_I4_0);
        }
    }

}