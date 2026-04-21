using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class GhostPatch : ModSystem
    {
        private MethodInfo drawPlayersMethod;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            drawPlayersMethod = typeof(LegacyPlayerRenderer).GetMethod(
                "DrawPlayers",
                BindingFlags.Instance | BindingFlags.Public
            );

            if (drawPlayersMethod == null)
            {
                return;
            }

            MonoModHooks.Add(drawPlayersMethod, FilterGhostPlayers);
        }

        public override void Unload()
        {
            drawPlayersMethod = null;
        }

        private delegate void DrawPlayersDelegate(
            LegacyPlayerRenderer self,
            Camera camera,
            IEnumerable<Player> players
        );

        private void FilterGhostPlayers(
            DrawPlayersDelegate orig,
            LegacyPlayerRenderer self,
            Camera camera,
            IEnumerable<Player> players
        )
        {
            IEnumerable<Player> filtered = players.Where(p =>
                p != null &&
                (
                    !p.ghost || p.whoAmI == Main.myPlayer
                )
            );

            orig(self, camera, filtered);
        }
    }
}