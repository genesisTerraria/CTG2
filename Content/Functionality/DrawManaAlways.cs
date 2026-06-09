using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality;

public sealed class LowManaStarILEditSystem : ModSystem
{
    private ILHook _playerStatsSnapshotCtorHook;
    private bool _patchApplied;
    private string _patchFailureReason;

    public override void Load()
    {
        // No reason to patch ui on a server
        if (Main.dedServ)
            return;

        ConstructorInfo ctor = typeof(PlayerStatsSnapshot).GetConstructor(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: new[] { typeof(Player) },
            modifiers: null
        );

        if (ctor is null)
        {
            Mod.Logger.Warn("Low-mana UI patch disabled: could not find PlayerStatsSnapshot(Player) constructor.");
            return;
        }

        ILHook hook = null;

        try
        {
            _patchApplied = false;
            _patchFailureReason = null;
            hook = new ILHook(ctor, PatchPlayerStatsSnapshotCtor);

            if (!_patchApplied)
            {
                hook.Dispose();
                Mod.Logger.Warn($"Low-mana UI patch disabled: {_patchFailureReason ?? "the constructor IL did not match the expected shape."}");
                return;
            }

            _playerStatsSnapshotCtorHook = hook;
            Mod.Logger.Info("Low-mana UI patch applied to PlayerStatsSnapshot(Player). ManaMax values from 1 to 19 will display one mana segment in snapshot-based resource styles, including Classic, Fancy, and Horizontal Bars.");
        }
        catch (Exception exception)
        {
            hook?.Dispose();
            _patchApplied = false;
            Mod.Logger.Warn($"Low-mana UI patch disabled after an IL hook error: {exception}");
        }
    }

    public override void Unload()
    {
        _playerStatsSnapshotCtorHook?.Dispose();
        _playerStatsSnapshotCtorHook = null;
        _patchApplied = false;
        _patchFailureReason = null;
    }

    private void PatchPlayerStatsSnapshotCtor(ILContext il)
    {
        int returnCount = 0;

        foreach (Instruction instruction in il.Body.Instructions)
        {
            if (instruction.MatchRet())
                returnCount++;
        }

        if (returnCount != 1)
        {
            _patchFailureReason = $"expected exactly one constructor return, but found {returnCount}.";
            return;
        }

        ILCursor c = new ILCursor(il);

        //Insert right before the constructor returns
        if (!c.TryGotoNext(MoveType.Before, i => i.MatchRet()))
        {
            _patchFailureReason = "could not find the PlayerStatsSnapshot constructor return.";
            return;
        }

        // this is a struct constructor so ldarg.0 is a ref PlayerStatsSnapshot
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(FixLowManaStarCount);
        _patchApplied = true;
    }

    private static void FixLowManaStarCount(ref PlayerStatsSnapshot snapshot)
    {
        // Terraria gives 0 stars for 1-19 max mana
        // We want at least 1 visible star if the player actually has mana.
        if (snapshot.ManaMax > 0 && snapshot.AmountOfManaStars <= 0)
            snapshot.AmountOfManaStars = 1;
    }
}
