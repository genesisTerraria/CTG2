using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Linq;

namespace CTG2.Content.Commands
{
    public class TeamChatCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "p";
        public override string Usage => "/p <message>";
        public override string Description => "Send a message to your team only.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                caller.Reply("Usage: /p <message>", Color.Red);
                return;
            }

            // Join all arguments back into a single message
            string message = string.Join(" ", args);
            
            if (string.IsNullOrWhiteSpace(message))
            {
                caller.Reply("Message cannot be empty!", Color.Red);
                return;
            }

            // Send team chat request to server
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.RequestTeamChat);
            packet.Write(caller.Player.whoAmI);
            packet.Write(message);
            packet.Send();
        }
    }
}