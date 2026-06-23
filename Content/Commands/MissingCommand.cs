using CTG2.Content.Commands.Auth;
using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class MissingCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "missing";
        public override string Usage => "/missing";
        public override string Description => "Lists queued NeatQueue players who are not in the world.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {

            // On a multiplayer client the roster lives on the server, so ask the server to build it.
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestMissing);
                packet.Send();
                return;
            }

            // Server: the roster is local, build and show it directly.
            string resultMessage = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>()
                .BuildMissingReport();
            caller.Reply(resultMessage, Color.CornflowerBlue);
        }
    }
}
