using ClassesNamespace;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace CTG2.Content
{
    /// <summary>
    /// Context passed to OnHurt callbacks so abilities can inspect attacker/damage info.
    /// </summary>
    public class AbilityHurtContext
    {
        public Player.HurtInfo HurtInfo { get; }
        public Player Attacker { get; }        // null if not a player source
        public int ProjectileType { get; }
        public int Damage { get; }

        public AbilityHurtContext(Player.HurtInfo info)
        {
            HurtInfo = info;
            Damage = info.Damage;
            ProjectileType = info.DamageSource.SourceProjectileType;

            int attackerIndex = info.DamageSource.SourcePlayerIndex;
            if (attackerIndex >= 0 && attackerIndex < Main.maxPlayers)
                Attacker = Main.player[attackerIndex];
        }
    }

    /// <summary>
    /// Context passed to OnHit callbacks.
    /// </summary>
    public class AbilityHitContext
    {
        public Entity Victim { get; }
        public float X { get; }
        public float Y { get; }

        public AbilityHitContext(float x, float y, Entity victim)
        {
            X = x;
            Y = y;
            Victim = victim;
        }
    }

    /// <summary>
    /// A named timer that counts down and fires an optional callback on expiry.
    /// </summary>
    public class AbilityTimer
    {
        public int Value { get; private set; } = -1;
        public bool IsRunning => Value >= 0;
        public bool JustExpired => Value == 0;

        private readonly Action _onExpire;

        public AbilityTimer(Action onExpire = null)
        {
            _onExpire = onExpire;
        }

        public void Start(int ticks) => Value = ticks;
        public void Stop() => Value = -1;

        /// <summary>Call once per tick (PostUpdate). Returns true when it hits 0.</summary>
        public bool Tick()
        {
            if (Value < 0) return false;
            Value--;
            if (Value == 0)
            {
                _onExpire?.Invoke();
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// A generic key/value store so each ability can stash arbitrary state
    /// without needing new fields on the base class.
    /// e.g.  State.Set("isHellfire", true);
    ///        State.Get&lt;bool&gt;("isHellfire");
    /// </summary>
    public class AbilityStateStore
    {
        private readonly Dictionary<string, object> _data = new();

        public void Set<T>(string key, T value) => _data[key] = value;

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out object val) && val is T typed)
                return typed;
            return defaultValue;
        }

        public bool Has(string key) => _data.ContainsKey(key);
        public void Remove(string key) => _data.Remove(key);
        public void Clear() => _data.Clear();
    }

    public class Ability : ModPlayer
    {
        // ── Lifecycle ────────────────────────────────────────────────────────────

        private bool _active = false;
        public bool IsActive => _active;

        public void Activate() => _active = true;
        public void Deactivate() => _active = false;

        // ── Cooldown ─────────────────────────────────────────────────────────────

        public int Cooldown { get; private set; } = 0;
        public bool OnCooldown => Cooldown > 0;

        public void SetCooldown(int seconds) => Cooldown = seconds * 60;
        public void SetCooldownTicks(int ticks) => Cooldown = ticks;

        // ── Arbitrary state ───────────────────────────────────────────────────────

        /// <summary>Store any per-ability data here instead of adding new fields.</summary>
        public AbilityStateStore State { get; } = new();

        // ── Timers ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Create a timer and register it so it auto-ticks and auto-expires.
        /// Pass an optional callback that fires when the timer hits 0.
        /// </summary>
        public AbilityTimer CreateTimer(Action onExpire = null)
        {
            var t = new AbilityTimer(onExpire);
            _timers.Add(t);
            return t;
        }

        private readonly List<AbilityTimer> _timers = new();

        // ── Sounds ────────────────────────────────────────────────────────────────

        public SoundStyle? UseSound { get; set; }           // played on activation
        public SoundStyle? EndSound { get; set; }           // played when main timer expires
        public SoundStyle? PassiveSound { get; set; }       // played by passive logic as needed
        public float SoundVolumeScale { get; set; } = 1f;

        public void PlayUseSound()
        {
            if (UseSound.HasValue)
                SoundEngine.PlaySound(UseSound.Value.WithVolumeScale(Main.soundVolume * SoundVolumeScale), Player.Center);
        }

        public void PlayEndSound()
        {
            if (EndSound.HasValue)
                SoundEngine.PlaySound(EndSound.Value.WithVolumeScale(Main.soundVolume * SoundVolumeScale), Player.Center);
        }

        public void PlayPassiveSound()
        {
            if (PassiveSound.HasValue)
                SoundEngine.PlaySound(PassiveSound.Value.WithVolumeScale(Main.soundVolume * SoundVolumeScale), Player.Center);
        }

        // ── Callbacks ─────────────────────────────────────────────────────────────

        /// <summary>Called every tick regardless of active state (for passives like Ninja stealth).</summary>
        public Action<Ability> PassiveTick { get; set; }

        /// <summary>Called every tick while active (post-status logic, e.g. Beast spawn delay, Clown swap delay).</summary>
        public Action<Ability> ActiveTick { get; set; }

        /// <summary>Called once when the ability is used.</summary>
        public Action<Ability> OnUseAction { get; set; }

        /// <summary>Called on hit. Receives hit context (victim, position).</summary>
        public Action<Ability, AbilityHitContext> OnHitAction { get; set; }

        /// <summary>Called when the player is hurt. Receives full hurt context.</summary>
        public Action<Ability, AbilityHurtContext> OnHurtAction { get; set; }

        /// <summary>Called when the player dies.</summary>
        public Action<Ability> OnDeathAction { get; set; }

        /// <summary>Called when CanUseItem is evaluated (return false to block).</summary>
        public Func<Ability, Item, bool> CanUseItemFunc { get; set; }

        /// <summary>Called when mana is consumed.</summary>
        public Action<Ability, Item, int> OnConsumeManaAction { get; set; }

        public Func<bool> CanActivate { get; set; }  // set per-class during setup

        private bool IsLocalActivePlayer =>
            Player.whoAmI == Main.myPlayer &&
            !Player.dead &&
            !Player.ghost;

        private bool MeetsActivationCondition =>
            CanActivate == null || CanActivate();

        // ── Use ───────────────────────────────────────────────────────────────────

        /// <summary>
        /// Call this to fire the ability: plays sound, sets active, invokes OnUseAction.
        /// Returns false and does nothing if on cooldown.
        /// </summary>
        public bool TryUse()
        {
            if (!IsLocalActivePlayer || OnCooldown || !MeetsActivationCondition)
                return false;

            Activate();
            PlayUseSound();
            OnUseAction?.Invoke(this);
            return true;
        }

        // ── ModPlayer hooks ───────────────────────────────────────────────────────

        public override void PostUpdate()
        {
            if (!IsLocalActivePlayer) return;

            // Passive always runs
            PassiveTick?.Invoke(this);

            // Active tick (post-status style logic)
            if (_active)
                ActiveTick?.Invoke(this);

            // Tick all registered timers
            foreach (var t in _timers)
                t.Tick();

            // Cooldown countdown
            if (Cooldown > 0)
                Cooldown--;
        }

        public override void OnHitAnything(float x, float y, Entity victim)
        {
            if (Player.whoAmI != Main.myPlayer || !_active) return;

            OnHitAction?.Invoke(this, new AbilityHitContext(x, y, victim));
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.whoAmI != Main.myPlayer) return;

            // OnHurt fires regardless of _active — many abilities (Ninja deequip,
            // attacker-class responses) need to react even when the buff isn't up.
            OnHurtAction?.Invoke(this, new AbilityHurtContext(info));
        }

        public override bool CanUseItem(Item item)
        {
            if (Player.whoAmI == Main.myPlayer && CanUseItemFunc != null)
                return CanUseItemFunc(this, item) && base.CanUseItem(item);

            return base.CanUseItem(item);
        }

        public override void OnConsumeMana(Item item, int manaConsumed)
        {
            if (Player.whoAmI != Main.myPlayer) return;

            OnConsumeManaAction?.Invoke(this, item, manaConsumed);
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (Player.whoAmI != Main.myPlayer) return;

            OnDeathAction?.Invoke(this);
        }
    }
}