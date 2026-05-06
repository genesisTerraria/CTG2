using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using CTG2.Content.ServerSide;
using CTG2.Content.Commands.Auth;

namespace CTG2.Content
{
    public class ClearMapQueueCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "clearmapqueue";
        public override string Usage => "clearmapqueue";
        public override string Description => "Clears the map queue.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (args.Length != 0)
            {
                caller.Reply("Usage: /clearmapqueue", Color.Red);
                return;
            }

            ModPacket myPacket = Mod.GetPacket();
            myPacket.Write((byte)MessageType.ClearMapQueue);
            myPacket.Send();

            caller.Reply("Cleared map queue.", Color.Green);
        }
    }
}