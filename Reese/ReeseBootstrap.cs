using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CTG2.ReeseIntegration;

// TEMPORARY BANDAID: loading into a Reese replay with CTG2 enabled can leave the local
// spectator in a broken limbo state. This system reflects into reese and fixes the bug.

[Autoload(Side = ModSide.Client)] //the server never enters playback so load on clients only
public sealed class ReeseReplayBootstrapBandAidSystem : ModSystem
{
    private const uint SeekToStartAtReplayTick = 180;

    private PropertyInfo isReplayPlaybackProperty;
    private PropertyInfo isSeekingProperty;
    private PropertyInfo currentPathProperty;
    private PropertyInfo currentTickProperty;
    private MethodInfo seekToStartMethod;

    private bool reflectionResolved;
    private bool systemDisabled;

    private bool wasInReplayPlayback;
    private bool firedForThisReplay;
    private string currentReplayPath;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        On_Main.DoUpdate += OnMainDoUpdate;
    }

    public override void Unload()
    {
        On_Main.DoUpdate -= OnMainDoUpdate;

        isReplayPlaybackProperty = null;
        isSeekingProperty = null;
        currentPathProperty = null;
        currentTickProperty = null;
        seekToStartMethod = null;

        ResetSessionState();
    }

    public override void PostSetupContent()
    {
        // Resolve Reese here instead of on load since mod load order is not guaranteed, but by
        // PostSetupContent every enabled mod has finished loading.
        if (Main.dedServ)
            return;

        if (!ModLoader.TryGetMod("Reese", out Mod reese))
        {
            // Reese isn't loaded; nothing to bandaid this session. Not worth a log line.
            systemDisabled = true;
            return;
        }

        try
        {
            Assembly reeseAssembly = reese.GetType().Assembly;
            Type replayPlaybackType = reeseAssembly.GetType("Reese.Common.Replayer.ReplayPlayback");

            if (replayPlaybackType is null)
            {
                systemDisabled = true;
                Mod.Logger.Warn("[Reese compat] Replay bootstrap bandaid disabled: type Reese.Common.Replayer.ReplayPlayback was not found.");
                return;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            isReplayPlaybackProperty = replayPlaybackType.GetProperty("IsReplayPlayback", flags);
            isSeekingProperty = replayPlaybackType.GetProperty("IsSeeking", flags);
            currentPathProperty = replayPlaybackType.GetProperty("CurrentPath", flags);
            currentTickProperty = replayPlaybackType.GetProperty("CurrentTick", flags);
            seekToStartMethod = replayPlaybackType.GetMethod("SeekToStart", flags, binder: null, Type.EmptyTypes, modifiers: null);

            if (isReplayPlaybackProperty is null || isSeekingProperty is null || currentPathProperty is null
                || currentTickProperty is null || seekToStartMethod is null)
            {
                systemDisabled = true;
                Mod.Logger.Warn("[Reese compat] Replay bootstrap bandaid disabled: one or more ReplayPlayback members (IsReplayPlayback, IsSeeking, CurrentPath, CurrentTick, SeekToStart) were not found.");
                return;
            }

            reflectionResolved = true;
            Mod.Logger.Info($"[Reese compat] Replay bootstrap bandaid armed: polling ReplayPlayback.CurrentTick from On_Main.DoUpdate; will auto-SeekToStart at replay tick {SeekToStartAtReplayTick}.");
        }
        catch (Exception exception)
        {
            systemDisabled = true;
            Mod.Logger.Warn($"[Reese compat] Replay bootstrap bandaid disabled: reflection against Reese failed: {exception}");
        }
    }

    private void OnMainDoUpdate(On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
    {
        orig(self, ref gameTime);

        if (systemDisabled || !reflectionResolved)
            return;

        try
        {
            TryRunSeekToStartBandAid();
        }
        catch (Exception exception)
        {
            // Never let the bandaid take down the game update loop
            systemDisabled = true;
            Mod.Logger.Warn($"[Reese compat] Replay bootstrap bandaid disabled after an unexpected error: {exception}");
        }
    }

    private void TryRunSeekToStartBandAid()
    {
        if (!(bool)isReplayPlaybackProperty.GetValue(null))
        {
            // Playback ended re-arm so the same replay file can be played again later.
            if (wasInReplayPlayback)
                ResetSessionState();
            return;
        }

        string replayPath = (string)currentPathProperty.GetValue(null);

        if (!wasInReplayPlayback)
        {
            wasInReplayPlayback = true;
            Mod.Logger.Info($"[Reese compat] Reese replay playback detected (replay \"{replayPath}\"); will auto-SeekToStart at replay tick {SeekToStartAtReplayTick}.");
        }

        // Path-change re-arm must happen before the fired check, or a new replay file
        // could stay stuck as "already fired". This and playback ending are the ONLY ways
        // the one-shot re-arms: SeekToStart() resets Reese's tick to 0, so the tick value
        // going backwards must never re-arm anything.
        if (!string.Equals(replayPath, currentReplayPath, StringComparison.Ordinal))
        {
            currentReplayPath = replayPath;
            firedForThisReplay = false;
        }

        if (firedForThisReplay)
            return;

        uint currentTick = (uint)currentTickProperty.GetValue(null);
        if (currentTick < SeekToStartAtReplayTick)
            return;

        if ((bool)isSeekingProperty.GetValue(null))
            return;

        // One attempt per replay session, even if the call throws - retrying a failing
        // SeekToStart every frame would just spam the log.
        firedForThisReplay = true;

        try
        {
            seekToStartMethod.Invoke(null, null);
            Mod.Logger.Info($"[Reese compat] Auto-called ReplayPlayback.SeekToStart() at replay tick {currentTick}, replay \"{replayPath}\".");
        }
        catch (Exception exception)
        {
            Mod.Logger.Warn($"[Reese compat] ReplayPlayback.SeekToStart() threw; manual \"Go to Start\" may still be needed: {exception}");
        }
    }

    private void ResetSessionState()
    {
        wasInReplayPlayback = false;
        firedForThisReplay = false;
        currentReplayPath = null;
    }
}
