using CTG2.Content.Commands.Auth;
using CTG2.Content.ClientSide;
using CTG2.Content.GameHooks;
using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class ForceStartBansCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "forcestartbans";
        public override string Usage => "/forcestartbans";
        public override string Description => "Force-starts the ban countdown timer.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestForceStartBans);
                packet.Send();
            }
            else if (Main.netMode == NetmodeID.Server)
            {
                Hooks.StartBanTimer();
            }
            else
            {
                BanTimer.ShowBanTimer(Hooks.BanTimerTicks);
            }

            caller.Reply("Force-started the ban timer.", Color.Green);
        }
    }
}
