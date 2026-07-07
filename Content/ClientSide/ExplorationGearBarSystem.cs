using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using CTG2.Content.Items;

namespace CTG2.Content.ClientSide;

// Client-only fuel gauge for PlanetaryExplorationGear.
// Draws a PlayerBars-styled bar under the local player while the jetpack is equipped.
// Purely visual: reads PlanetaryExplorationGearPlayer.flightTimeRemaining and never touches
// flight logic, damage, or networking.
[Autoload(Side = ModSide.Client)]
public class JetpackFuelBarSystem : ModSystem
{
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

        int index = layers.FindIndex(layer => layer.Name == "Player Bars: Bars Container");

        if (index == -1)
            index = layers.FindIndex(layer => layer.Name == "Vanilla: Interface Logic 2");

        if (index == -1)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "CTG2: Jetpack Fuel Bar",
            DrawFuelBar,
            InterfaceScaleType.UI
        ));
    }

    private bool DrawFuelBar()
    {
        if (Main.dedServ || Main.gameMenu)
            return true;

        Player player = Main.LocalPlayer;
        if (player == null || !player.active || player.dead)
            return true;

        var fuel = player.GetModPlayer<PlanetaryExplorationGearPlayer>();
        if (!fuel.hasJetpack)
            return true;

        float progress = Utils.Clamp(fuel.flightTimeRemaining / PlanetaryExplorationGear.FlightTimeMax, 0f, 1f);

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

        Color backgroundColor = new Color(70, 40, 20, 160);
        Color fuelColor;
        if (progress <= 0f)
        {
            // Empty: dim red pulse as a "no fuel" warning.
            float pulse = 0.6f + 0.2f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6f);
            fuelColor = new Color(200, 60, 40, 220) * pulse;
        }
        else
        {
            fuelColor = new Color(40, 220, 220, 210);
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
            fuelColor,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );

        return true;
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

            if (_scaleProp != null && _offsetProp != null)
                _playerBarsAvailable = true;
        }
        catch
        {
            _playerBarsAvailable = false;
        }
    }
}