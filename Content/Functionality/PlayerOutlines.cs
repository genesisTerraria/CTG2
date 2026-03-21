using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;
using CTG2.Content.Configs;

namespace CTG2.Content.Functionality
{
    [Autoload(Side = ModSide.Client)]
    public class PlayerOutlines : ModSystem
    {
        private delegate void CreateOutlinesDelegate(float alpha, float scale, Color borderColor);
        private CreateOutlinesDelegate _createOutlines;

        public override void Load()
        {
            // Grab Terraria's internal outline renderer
            var method = typeof(LegacyPlayerRenderer).GetMethod(
                "CreateOutlines",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            _createOutlines = (CreateOutlinesDelegate)method.CreateDelegate(
                typeof(CreateOutlinesDelegate),
                Main.PlayerRenderer
            );

            On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += Hook_DrawPlayer;
        }

        public override void Unload()
        {
            _createOutlines = null;
        }

        private void Hook_DrawPlayer(
            On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig,
            ref PlayerDrawSet drawInfo)
        {
            try
            {
                Player player = drawInfo.drawPlayer;

                // --- Basic sanity checks ---
                if (drawInfo.shadow != 0f) return;
                if (drawInfo.headOnlyRender) return;
                if (!player.active || player.dead) return;
                if (player == Main.LocalPlayer && !ModContent.GetInstance<CTG2Config>().EnabledLocalPlayerTeamOutline) return;

                // Only outline players on a team
                if (player.team == 0) return;
                //if (player == Main.LocalPlayer) return;

                // Skip off-screen players (perf)
                Rectangle screen = new Rectangle(
                    (int)Main.screenPosition.X,
                    (int)Main.screenPosition.Y,
                    Main.screenWidth,
                    Main.screenHeight
                );

                if (!player.getRect().Intersects(screen))
                    return;

                // --- Team color ---
                Color teamColor = Main.teamColor[player.team];

                // Apply lighting so it matches world brightness
                // Color litColor = teamColor.MultiplyRGBA(
                //     Lighting.GetColor(drawInfo.Center.ToTileCoordinates())
                // );
                Color litColor = Main.teamColor[player.team];

                // Respect stealth (like vanilla does)
                float alpha = player.stealth;

                // --- DRAW OUTLINE ---
                _createOutlines(alpha, 1f, litColor);
            }
            finally
            {
                // Always draw player normally
                orig(ref drawInfo);
            }
        }
    }
}