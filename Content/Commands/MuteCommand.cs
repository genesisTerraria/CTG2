using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using CTG2.Content.ClientSide;


namespace CTG2.Content.Commands
{
    public class MuteCommand : ModCommand
    {
        private static readonly Dictionary<int, int> playerTeamAssignments = new();

        public override CommandType Type => CommandType.Chat;
        public override string Command => "mute";
        public override string Usage => "/mute <playerName>";
        public override string Description => "Mutes a player.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            
            if (args.Length < 1)
            {
                caller.Reply("Usage: /mute <playerName>", Color.Red);
                return;
            }

            string targetName = args[0].ToLower();


            Player target = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName);

            if (target == null)
            {
                caller.Reply($"Player '{targetName}' not found.", Color.Red);
                return;
            }


            var mod1 = ModContent.GetInstance<CTG2>();
            ModPacket packet1 = mod1.GetPacket();
            packet1.Write((byte)MessageType.RequestMute);
            packet1.Write(target.whoAmI);
            packet1.Send();
            
            caller.Reply($"Muted {target.name}", Color.Green);
        }
    }
}
