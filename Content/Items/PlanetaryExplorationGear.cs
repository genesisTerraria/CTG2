using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.IO;
using CTG2.Content.Buffs;

namespace CTG2.Content.Items
{
    [AutoloadEquip(EquipType.Shield)] // dash shadow-effect equip slot
    public class PlanetaryExplorationGear : ModItem
    {
        // --- Jetpack tunables -----------------------------------------
        public const int FlightTimeMax = 30;       // ticks
        public const float FlightSpeed = 4.5f;        // max horizontal air speed
        public const float FlightAcceleration = 0.4f;
        public const float AscentSpeed = 5f;        // vertical speed holding jump
        public const float GroundRegenRate = 1f;  // fuel regained per tick while grounded
        public const bool InstantRegenOnGround = false;


        public override void SetDefaults()
        {
            // Copies stats AND wingSlot from vanilla Jetpack, so the equipped sprite
            // and flight animation are identical without registering our own EquipTexture.
            Item.CloneDefaults(ItemID.Jetpack);

            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<PlanetaryExplorationGearPlayer>();
            modPlayer.hasJetpack = true;
            modPlayer.DashAccessoryEquipped = true;
        }

        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
        {
            var gear = player.GetModPlayer<PlanetaryExplorationGearPlayer>();

            if (gear.DashTimer > 0)
            {
                speed = 0f;
                acceleration = 0f;
                return;
            }

            if (gear.flightTimeRemaining <= 0f)
                return;

            if (player.HasBuff(BuffID.Dazed) || player.HasBuff(BuffID.OgreSpit))
                return;

            speed = FlightSpeed;
            acceleration = FlightAcceleration;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
            ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            var gear = player.GetModPlayer<PlanetaryExplorationGearPlayer>();

            if (gear.DashTimer > 0)
            {
                ascentWhenFalling = 0f;
                ascentWhenRising = 0f;
                maxCanAscendMultiplier = 0f;
                maxAscentMultiplier = 0f;
                constantAscend = 0f;
                return;
            }

            if (gear.flightTimeRemaining <= 0f || player.HasBuff(BuffID.Dazed) || player.HasBuff(BuffID.OgreSpit))
            {
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
        // --- Jetpack state ----------------------------------------------
        private const float FuelPerTick = 1f;
        private const float ThrustRampUp = 0.05f;

        public bool hasJetpack;
        public float flightTimeRemaining;
        private bool hadJetpackLastFrame;

        // --- Dash state ---------------------------------------------------
        private bool playedSound = false;
        public const int DashCooldown = 90;
        public const int DashDuration = 15;
        public float DashVelocity = 13.5f;
        public float DashDecay = 0.95f; // multiplier applied to dash velocity every tick
        public bool dashKeybindActive = false;
        public bool DashAccessoryEquipped;
        public int DashDelay = 0;
        public int DashTimer = 0;
        public bool recentlyEnded = false;

        // Current dash velocity vector, re-applied every tick while dashing
        private Vector2 currentDashVelocity;

        public void SendDash(Vector2 velocity, int toWho = -1, int fromWho = -1)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.BlessingOfTheDragons);
            packet.Write((byte)Player.whoAmI);
            packet.WriteVector2(velocity);
            packet.Write(DashDelay);
            packet.Send(toWho, fromWho);
        }

        public void ReceiveDash(Player player, BinaryReader reader)
        {
            Vector2 velocity = reader.ReadVector2();
            int dashDelay = reader.ReadInt32();

            player.velocity = velocity;
            player.eocDash = DashTimer;
        }

        public override void OnEnterWorld()
        {
            flightTimeRemaining = PlanetaryExplorationGear.FlightTimeMax;
        }

        public override void ResetEffects()
        {
            hasJetpack = false;

            if (Player.whoAmI == Main.myPlayer)
            {
                DashAccessoryEquipped = false;

                if (recentlyEnded && DashTimer == 0)
                    recentlyEnded = false;

                dashKeybindActive = CTG2.BlessingOfTheDragonsKeybind.JustPressed;
            }
        }

        public override void PostUpdateEquips()
        {
            if (hadJetpackLastFrame && !hasJetpack)
            {
                OnJetpackUnequipped();
            }

            hadJetpackLastFrame = hasJetpack;
        }

        private void OnJetpackUnequipped()
        {
            Player.wingTime = 0;
            flightTimeRemaining = 0;
        }

        public override void PreUpdateMovement()
        {
            if (DashDelay == 0 && !playedSound)
            {
                SoundEngine.PlaySound(SoundID.Item130.WithVolumeScale(2f));
                playedSound = true;
            }

            if (Player.whoAmI == Main.myPlayer && !Player.HasBuff(BuffID.Webbed) &&
                !Player.HasBuff(BuffID.OgreSpit) && !Player.HasBuff(BuffID.Dazed) &&
                !Player.HasBuff(ModContent.BuffType<Transmutated>()))
            {
                bool canThrust = flightTimeRemaining > 0f;

                if (dashKeybindActive && DashDelay == 0 && DashAccessoryEquipped && canThrust)
                {
                    Vector2 direction = Main.MouseWorld - Player.Center;
                    if (direction.Length() == 0)
                        return;

                    direction.Normalize();

                    // Full dash velocity applied equally in all directions —
                    // no horizontal-reduction, no jetpack interference.
                    float flightTimeMultiplier = 0.67f + 0.33f * (flightTimeRemaining / 30);
                    currentDashVelocity = direction * DashVelocity * flightTimeMultiplier;
                    Player.velocity = currentDashVelocity;

                    DashDelay = DashCooldown;
                    DashTimer = DashDuration;
                    flightTimeRemaining = 0;
                    recentlyEnded = true;
                    playedSound = false;

                    SoundEngine.PlaySound(SoundID.Item45.WithVolumeScale(2f));
                }

                if (DashDelay > 0) DashDelay--;

                Player.eocDash = DashTimer;

                if (DashTimer > 0)
                {
                    Player.armorEffectDrawShadowEOCShield = true;
                    DashTimer--;

                    // Re-apply every tick BEFORE gravity/movement processing,
                    // so gravity's effect from the previous tick never accumulates.

                    int direction = Player.velocity.X > 0 ? 1 : -1;

                    Point tileAheadUpper = (Player.Center + new Vector2(direction * Player.width / 2f + 2f, Player.gravDir * (-Player.height) / 2f + Player.gravDir * 2f)).ToTileCoordinates();
                    Point tileAheadMid = (Player.Center + new Vector2(direction * Player.width / 2f + 2f, 0f)).ToTileCoordinates();

                    if (WorldGen.SolidOrSlopedTile(tileAheadUpper.X, tileAheadUpper.Y) || WorldGen.SolidOrSlopedTile(tileAheadMid.X, tileAheadMid.Y))
                    {
                        DashTimer = 0;
                    }
                    else
                    {
                        Player.velocity = currentDashVelocity;
                        currentDashVelocity *= DashDecay;
                    }

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        SendDash(Player.velocity);
                    }
                }
                else
                {
                    Player.armorEffectDrawShadowEOCShield = false;
                }
            }
        }

        public override void PostUpdate()
        {
            if (!hasJetpack)
                return;

            if (Player.HasBuff(BuffID.Dazed) || Player.HasBuff(BuffID.OgreSpit)
             || Player.HasBuff(BuffID.Webbed) || Player.HasBuff(ModContent.BuffType<Transmutated>()))
                return;

            if (DashTimer > 0)
                return;

            bool grounded = Player.velocity.Y == 0f && !Player.mount.Active;

            if (grounded)
            {
                flightTimeRemaining = PlanetaryExplorationGear.InstantRegenOnGround
                    ? PlanetaryExplorationGear.FlightTimeMax
                    : Math.Min(PlanetaryExplorationGear.FlightTimeMax, flightTimeRemaining + PlanetaryExplorationGear.GroundRegenRate);
                return;
            }

            bool canThrust = flightTimeRemaining > 0f && Player.controlJump;

            if (canThrust)
            {
                flightTimeRemaining = Math.Max(0f, flightTimeRemaining - FuelPerTick);
                float targetVelocity = -PlanetaryExplorationGear.AscentSpeed;
                Player.velocity.Y = MathHelper.Lerp(Player.velocity.Y, targetVelocity, ThrustRampUp);
            }

            Player.wingTime = (int)flightTimeRemaining;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (DashTimer > 0)
            {
                // Cancel the dash entirely — let knockback own the velocity from here.
                DashTimer = 0;
                currentDashVelocity = Vector2.Zero;
                Player.armorEffectDrawShadowEOCShield = false;
            }
        }
    }
}