using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;


namespace CTG2.Content.Commands
{
    public class JoinCommand : ModCommand
    {
        private static readonly Dictionary<int, int> playerTeamAssignments = new();

        public override CommandType Type => CommandType.Chat;
        public override string Command => "join";
        public override string Usage => "/join";
        public override string Description => "Join an ongoing game.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var gameManager = ModContent.GetInstance<GameManager>();

            if (gameManager.scrimsConfig)
            {
                caller.Reply("You cannot join the game at this time!", Color.Red);
                return;
            }

            if (args.Length != 0)
            {
                caller.Reply("Usage: /join", Color.Red);
                return;
            }

            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestTeamChange);
            packet.Write(caller.Player.whoAmI);
            packet.Write(-1);
            packet.Send();
        }
    }
}
