using Terraria;
using Terraria.ModLoader;
using Terraria.Chat;
using Microsoft.Xna.Framework;
using CTG2.Content.Commands.Auth;

namespace CTG2.Content.Commands
{
    public class GodModeCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "godmode";
        public override string Description => "Toggles god mode (invulnerability) for the player.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            var godModePlayer = caller.Player.GetModPlayer<GodModePlayer>();
            godModePlayer.IsGodMode = !godModePlayer.IsGodMode;

            string status = godModePlayer.IsGodMode ? "enabled" : "disabled";
            caller.Reply($"God mode is now {status}.", Color.Green);
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[Admin] {caller.Player.name} turned {status} god mode."), Color.Yellow);
        }
    }

    public class GodModePlayer : ModPlayer
    {
        public bool IsGodMode = false;

        public override void OnHurt(Player.HurtInfo info)
        {
            if (IsGodMode)
            {
                info.Damage = 0;
            }
        }
    }
}
