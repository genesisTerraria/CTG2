using CTG2.Content.Commands.Auth;
using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class SetTimerCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "settimer";
        public override string Usage => "/settimer <minutes>";
        public override string Description => "Sets the current match timer in minutes.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (args.Length != 1 || !int.TryParse(args[0], out int minutes) || minutes < 0)
            {
                caller.Reply("Usage: /settimer <minutes>", Color.Red);
                return;
            }

            if (minutes > int.MaxValue / 60)
            {
                caller.Reply("Timer value is too large.", Color.Red);
                return;
            }

            int timeInSeconds = minutes * 60;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestChangeTimer);
                packet.Write(timeInSeconds);
                packet.Send();
            }
            else
            {
                ModContent.GetInstance<GameManager>().changetimer(timeInSeconds);
            }

            caller.Reply($"Set match timer to {minutes}:00.", Color.Green);
        }
    }
}
