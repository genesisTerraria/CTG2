// PlanetaryExplorationGear.cs
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items;

public class PlanetaryExplorationGear : ModItem
{
    // --- Tunables (edit these) -----------------------------------------
    public const int FlightTimeMax = 45;       // ticks
    public const float FlightSpeed = 5f;        // max horizontal air speed
    public const float FlightAcceleration = 0.4f;
    public const float AscentSpeed = 5f;        // vertical speed holding jump
    public const float GroundRegenRate = 1.5f;       // fuel regained per tick while grounded
    public const bool InstantRegenOnGround = false; // true = vanilla-style full refill
    // ---------------------------------------------------------------------

    // Reuse the vanilla icon texture directly — no custom art needed.
    public override string Texture => "Terraria/Images/Item_" + ItemID.Jetpack;

    public override void SetDefaults()
    {
        // Copies stats AND wingSlot from vanilla Jetpack, so the equipped sprite
        // and flight animation are identical without registering our own EquipTexture.
        Item.CloneDefaults(ItemID.Jetpack);
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<PlanetaryExplorationGearPlayer>().hasJetpack = true;
    }

    public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
    {
        var fuel = player.GetModPlayer<PlanetaryExplorationGearPlayer>();
        if (fuel.flightTimeRemaining <= 0f)
            return; // out of fuel: fall back to vanilla no-flight defaults

        if (player.HasBuff(BuffID.Dazed) || player.HasBuff(BuffID.OgreSpit))
            return;

        speed = FlightSpeed;
        acceleration = FlightAcceleration;
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
        ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        var fuel = player.GetModPlayer<PlanetaryExplorationGearPlayer>();
        if (fuel.flightTimeRemaining <= 0f || player.HasBuff(BuffID.Dazed) || player.HasBuff(BuffID.OgreSpit))
        {
            // Kill the hold-jump slow-fall assist entirely once fuel is gone.
            ascentWhenFalling = 0f;
            ascentWhenRising = 0f;
            maxCanAscendMultiplier = 0f;
            maxAscentMultiplier = 0f;
            constantAscend = 0f;
            return;
        }

        ascentWhenFalling = AscentSpeed;
        ascentWhenRising = AscentSpeed;
        maxCanAscendMultiplier = 1f;
        maxAscentMultiplier = 1f;
        constantAscend = AscentSpeed * 0.5f;
    }
}

public class PlanetaryExplorationGearPlayer : ModPlayer
{
    // Approximate vanilla gravity/terminal fall speed for the no-fuel fall.
    private const float NormalGravity = 0.15f;
    private const float TerminalFallSpeed = 10f;

    // Fuel consumed per tick of active thrust.
    private const float FuelPerTick = 1f;
    private const float ThrustRampUp = 0.3f;   // 0-1, higher = faster to reach full ascent speed
    private const float ThrustRampDown = 0.3f; // 0-1, higher = faster to stop on release

    public bool hasJetpack;
    public float flightTimeRemaining;

    // Tracks equip state across frames so PostUpdateEquips can detect the
    // equip/unequip edge. hasJetpack itself gets wiped every frame by
    // ResetEffects, so we can't compare against it directly next frame.
    private bool hadJetpackLastFrame;

    public override void OnEnterWorld()
    {
        flightTimeRemaining = PlanetaryExplorationGear.FlightTimeMax;
    }

    public override void ResetEffects()
    {
        hasJetpack = false;
    }

    public override void PostUpdateEquips()
    {
        // Runs after all UpdateAccessory hooks for this tick, so hasJetpack
        // is settled: true only if the jetpack is still equipped right now.
        if (hadJetpackLastFrame && !hasJetpack)
        {
            OnJetpackUnequipped();
        }

        hadJetpackLastFrame = hasJetpack;
    }
    private void OnJetpackUnequipped()
    {
        // Fires once, the tick the jetpack comes off.
        // Stop any active thrust immediately rather than let residual
        // velocity/animation state linger from the last equipped frame.
        Player.wingTime = 0;
        flightTimeRemaining = 0;

        // TODO: hide the fuel bar HUD element here.
        // TODO: stop the thrust sound loop here, if/when one exists.
    }

    public override void PostUpdate()
    {
        if (!hasJetpack)
            return;

        if (Player.HasBuff(BuffID.Dazed) || Player.HasBuff(BuffID.OgreSpit))
            return;

        bool grounded = Player.velocity.Y == 0f && !Player.mount.Active;

        if (grounded)
        {
            flightTimeRemaining = PlanetaryExplorationGear.InstantRegenOnGround
                ? PlanetaryExplorationGear.FlightTimeMax
                : System.Math.Min(PlanetaryExplorationGear.FlightTimeMax, flightTimeRemaining + PlanetaryExplorationGear.GroundRegenRate);
            return;
        }

        bool canThrust = flightTimeRemaining > 0f && Player.controlJump;

        if (canThrust)
        {
            flightTimeRemaining = System.Math.Max(0f, flightTimeRemaining - FuelPerTick);
            float targetVelocity = -PlanetaryExplorationGear.AscentSpeed;
            Player.velocity.Y = MathHelper.Lerp(Player.velocity.Y, targetVelocity, ThrustRampUp);
        }
        else
        {
            if (Player.velocity.Y < 0f)
                Player.velocity.Y = MathHelper.Lerp(Player.velocity.Y, 0f, ThrustRampDown);

            Player.velocity.Y = System.Math.Min(Player.velocity.Y + NormalGravity, TerminalFallSpeed);
        }

        Player.wingTime = (int)flightTimeRemaining;
    }
}