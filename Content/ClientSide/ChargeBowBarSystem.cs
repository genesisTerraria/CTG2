using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace CTG2.Content.ClientSide;

// Client-only charge indicator for the Rancor charge bow.
// Draws a PlayerBars-styled bar under the local player while they are charging.
// Purely visual: it reads the existing ChargedBowProjectile charge state and
// never touches charge logic, damage, shooting, or networking.
[Autoload(Side = ModSide.Client)]
public class ChargeBowBarSystem : ModSystem
{
    // Max charge value the bow caps at (see ChargedBowProjectile, charge is clamped to 40f everywhere).
    private const float MaxCharge = 40f;

    // PlayerBars-like defaults used when PlayerBars is not loaded / reflection fails.
    private const float DefaultScale = 1.2f;
    private const int DefaultOffset = 28;

    // Cached reflection handles into PlayerBars.UI.BarsContainer (resolved lazily, once).
    private static bool _reflectionInitialized;
    private static bool _playerBarsAvailable;
    private static PropertyInfo _scaleProp;
    private static PropertyInfo _offsetProp;
    private static PropertyInfo _showHealthProp;
    private static PropertyInfo _showManaProp;
    private static PropertyInfo _autoHideProp;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.dedServ)
            return;

        // Prefer to sit right after the PlayerBars container so we stack beneath its bars.
        int index = layers.FindIndex(layer => layer.Name == "Player Bars: Bars Container");

        // Otherwise fall back to where PlayerBars itself inserts.
        if (index == -1)
            index = layers.FindIndex(layer => layer.Name == "Vanilla: Interface Logic 2");

        if (index == -1)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "CTG2: Charge Bow Bar",
            DrawChargeBar,
            InterfaceScaleType.UI
        ));
    }

    private bool DrawChargeBar()
    {
        if (Main.dedServ || Main.gameMenu)
            return true;

        Player player = Main.LocalPlayer;
        if (player == null || !player.active || player.dead)
            return true;

        if (!TryGetChargeBowProgress(player, out float progress))
            return true;

        if (progress <= 0f)
            return true;

        Texture2D front = TextureAssets.Hb1.Value;
        Texture2D back = TextureAssets.Hb2.Value;

        var settings = GetBarsSettings();
        float scale = settings.scale;
        int offset = settings.offset;

        Vector2 screenPos = player.Center.ToScreenPosition();
        float x = screenPos.X - front.Width / 2f * scale;
        float y = screenPos.Y + offset * Main.GameZoomTarget;

        // Stack beneath whatever PlayerBars is actually drawing.
        if (settings.playerBars)
        {
            float healthPercent = player.statLifeMax2 > 0 ? player.statLife / (float)player.statLifeMax2 : 0f;
            float manaPercent = player.statManaMax2 > 0 ? player.statMana / (float)player.statManaMax2 : 0f;

            bool healthDraws = settings.showHealth
                && player.statLife > 0
                && (!settings.autoHide || healthPercent < 1f);

            bool manaDraws = settings.showMana
                && player.statLife > 0
                && (!settings.autoHide || manaPercent < 1f);

            // PlayerBars always reserves slot 1 for mana, even when health is hidden.
            int slotIndex = 0;
            if (healthDraws)
                slotIndex = 1;
            if (manaDraws)
                slotIndex = 2;

            y += slotIndex * (front.Height * scale + 1f);
        }

        Color backgroundColor = new Color(60, 40, 90, 160);
        Color chargeColor;
        if (progress >= 1f)
        {
            // Subtle pulse on full charge, kept readable.
            float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f);
            chargeColor = new Color(255, 220, 80, 220) * pulse;
        }
        else
        {
            chargeColor = new Color(170, 80, 230, 200);
        }

        Main.spriteBatch.Draw(
            back,
            new Vector2(x, y),
            new Rectangle(0, 0, back.Width, back.Height),
            backgroundColor,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );

        Main.spriteBatch.Draw(
            front,
            new Vector2(x, y),
            new Rectangle(0, 0, (int)(front.Width * progress), front.Height),
            chargeColor,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );

        return true;
    }

    // Reads the active ChargedBowProjectile owned by the player. Returns false when the
    // bow isn't being charged (no projectile, or already released/fired).
    private static bool TryGetChargeBowProgress(Player player, out float progress)
    {
        progress = 0f;

        int type = ModContent.ProjectileType<ChargedBowProjectile>();
        if (player.ownedProjectileCounts[type] <= 0)
            return false;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.owner == player.whoAmI && p.ModProjectile is ChargedBowProjectile bow)
            {
                if (bow.GetReleased())
                    return false;

                progress = Utils.Clamp(bow.GetCharge() / MaxCharge, 0f, 1f);
                return true;
            }
        }

        return false;
    }

    private struct BarsSettings
    {
        public float scale;
        public int offset;
        public bool playerBars;
        public bool showHealth;
        public bool showMana;
        public bool autoHide;
    }

    private static BarsSettings GetBarsSettings()
    {
        BarsSettings result = new BarsSettings
        {
            scale = DefaultScale,
            offset = DefaultOffset,
            playerBars = false,
            showHealth = false,
            showMana = false,
            autoHide = false
        };

        if (!_reflectionInitialized)
            InitReflection();

        if (!_playerBarsAvailable)
            return result;

        try
        {
            result.scale = Convert.ToSingle(_scaleProp.GetValue(null));
            result.offset = Convert.ToInt32(_offsetProp.GetValue(null));
            result.showHealth = _showHealthProp != null && (bool)_showHealthProp.GetValue(null);
            result.showMana = _showManaProp != null && (bool)_showManaProp.GetValue(null);
            result.autoHide = _autoHideProp != null && (bool)_autoHideProp.GetValue(null);
            result.playerBars = true;
        }
        catch
        {
            // PlayerBars changed shape or threw; fall back to defaults and behave as if absent.
            result.scale = DefaultScale;
            result.offset = DefaultOffset;
            result.playerBars = false;
        }

        return result;
    }

    private static void InitReflection()
    {
        _reflectionInitialized = true;

        try
        {
            if (!ModLoader.TryGetMod("PlayerBars", out Mod playerBarsMod))
                return;

            Type type = playerBarsMod.GetType().Assembly.GetType("PlayerBars.UI.BarsContainer");
            if (type == null)
                return;

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            _scaleProp = type.GetProperty("Scale", flags);
            _offsetProp = type.GetProperty("Offset", flags);
            _showHealthProp = type.GetProperty("ShowHealthBar", flags);
            _showManaProp = type.GetProperty("ShowManaBar", flags);
            _autoHideProp = type.GetProperty("AutoHideOnFull", flags);

            // Scale + Offset are the minimum needed to position correctly.
            if (_scaleProp != null && _offsetProp != null)
                _playerBarsAvailable = true;
        }
        catch
        {
            _playerBarsAvailable = false;
        }
    }
}
