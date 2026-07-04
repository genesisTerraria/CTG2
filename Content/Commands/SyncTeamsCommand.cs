using CTG2.Content.Commands.Auth;
using CTG2.Content.ServerSide;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class SyncTeamsCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "syncteams";
        public override string Usage => "/syncteams";
        public override string Description => "Reassigns every online player to their NeatQueue roster team. Players not in the roster are set to white.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (!caller.Player.GetModPlayer<AuthPlayer>().IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
                packet.Write((byte)MessageType.RequestSyncTeams);
                packet.Send();
                return;
            }

            string resultMessage = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>()
                .SyncTeamsToRoster();
            caller.Reply(resultMessage, Color.CornflowerBlue);
        }
    }
}
