using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;


namespace CTG2.Content.Buffs
{
    public class Netted : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Webbed;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;

            // Reuse the Webbed buff texture
            BuffID.Sets.IsATagBuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<NettedPlayer>().isNetted = true;
        }
    }

    public class NettedPlayer : ModPlayer
    {
        public bool isNetted = false;

        public override void ResetEffects()
        {
            isNetted = false;
        }

        public override void SetControls()
        {
            if (!isNetted)
                return;

            // Kill all movement inputs before the engine reads them
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;

            // Kill grapple
            Player.controlHook = false;

            // Prevent dashes and mounts from moving the player
            Player.dashDelay = -1;

            // Kill velocity so they stop instantly (like webbed does)
            Player.velocity.X = 0f;
            Player.velocity.Y = 0f;

            // Prevent knockback
            Player.noKnockback = true;

            // Item use is intentionally NOT blocked here
        }
    }
}