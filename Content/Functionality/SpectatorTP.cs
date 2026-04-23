using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class SpectatorModePlayer : ModPlayer
    {



        public override void PreUpdate()
        {
            if (Player.whoAmI != Main.myPlayer || !Player.ghost) //probably better to check if player is spectator rather than ghost
            {
                return;
            }

            if (
                Main.gameMenu ||
                Player.dead ||
                Player.mouseInterface ||
                !Main.mouseLeft ||
                !Main.mouseLeftRelease
            )
            {
                return;
            }

            Main.mouseLeftRelease = false;
            TeleportToMouse(Player);
        }

        private static void TeleportToMouse(Player player)
        {
            Vector2 destination = Main.MouseWorld - player.Size / 2f;
            player.position = destination;
            player.velocity = Vector2.Zero;
            player.fallStart = (int)(player.position.Y / 16f);

            //use this teleport logic since built int tmodloader logic forces effects when teleporting

            /* if (Main.netMode != NetmodeID.SinglePlayer) 
            { //No need to sync spectator positions and add extra network strain
                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
            } */
        }
    }

    public class GhostEmoteBlocker : ModSystem
    {
        private MethodInfo makePlayerEmoteMethod;

        public override void Load()
        {
            makePlayerEmoteMethod = typeof(EmoteBubble).GetMethod(
                "MakePlayerEmote",
                BindingFlags.Public | BindingFlags.Static
            );

            if (makePlayerEmoteMethod == null)
            {
                return;
            }

            MonoModHooks.Add(makePlayerEmoteMethod, BlockGhostEmotes);
        }

        public override void Unload()
        {
            makePlayerEmoteMethod = null;
        }

        private delegate void MakePlayerEmoteDelegate(Player player, int emoteId, bool syncBetweenClients);

        private void BlockGhostEmotes(
            MakePlayerEmoteDelegate orig,
            Player player,
            int emoteId,
            bool syncBetweenClients
        )
        {
            if (player.ghost == true)
            {
                return;
            }

            orig(player, emoteId, syncBetweenClients);
        }
    }

    public class GhostEmoteDrawBlocker : GlobalEmoteBubble
    {
        public override bool PreDraw(
            EmoteBubble emoteBubble,
            SpriteBatch spriteBatch,
            Texture2D texture,
            Vector2 position,
            Rectangle frame,
            Vector2 origin,
            SpriteEffects spriteEffects
        )
        {
            if (emoteBubble.anchor?.entity is Player player && player.ghost)
            {
                return false;
            }

            return true;
        }
    }

}
