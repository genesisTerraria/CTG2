using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.Chat;
using Terraria.Localization;
using CTG2.Content.ServerSide;
using CTG2.Content.Commands.Auth;

namespace CTG2.Content.Commands
{
    public class GamemodeCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "gamemode";
        public override string Description => "Sets the gamemode (pubs/scrims/rng)";
        public override string Usage => "/gamemode <mode>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (args.Length != 1)
            {
                caller.Reply("Usage: /gamemode <mode>", Color.Red);
                return;
            }

            string mode = args[0].ToLowerInvariant();

            if (mode == "pubs" || mode == "scrims" || mode == "rng")
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestGamemodeChange);
                packet.Write(mode);
                packet.Send();

                return;
            }
            
            caller.Reply($"Unknown gamemode: {args[0]}", Color.Red);
        }
    }
}