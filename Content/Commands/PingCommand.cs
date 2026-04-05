using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Net;
using System;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.Audio;
using Terraria.GameContent;
using System.IO;
using CTG2.Content;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;




namespace CTG2.Content.Commands
{
    public class PingCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "ping";
        public override string Usage => "/ping <playerName>";
        public override string Description => "Shows a player's current ping.";
        public Player lastPlayer = null;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                if (lastPlayer == null)
                {
                    lastPlayer = caller.Player;
                }

                bool found = false;

                for (int player = 0; player < Main.maxPlayers; player++)
                {
                    if (Main.player[player].active && Main.player[player].name == lastPlayer.name)
                    {
                        found = true;
                        lastPlayer = Main.player[player];
                        break;
                    }
                }

                if (found)
                {
                    var mod = ModContent.GetInstance<CTG2>();
                    var packet = mod.GetPacket();
                    packet.Write((byte)MessageType.RequestPlayerPing);
                    packet.Write(lastPlayer.whoAmI);
                    //packet.Write(true);
                    packet.Send();
                    return;
                }
                else
                {
                    caller.Reply("Last player no longer valid.", Color.Red);
                    return;
                }
            }

            string targetName = "";

            if (args[0].StartsWith("\""))
            {
                List<string> parts = new List<string>();
                bool foundClosingQuote = false;

                for (int i = 0; i < args.Length; i++)
                {
                    parts.Add(args[i]);
                    
                    if (args[i].EndsWith("\""))
                    {
                        foundClosingQuote = true;

                        targetName = string.Join(" ", parts).Trim('"');

                        break;
                    }
                }

                if (!foundClosingQuote)
                {
                    caller.Reply("Missing closing quote for item name.", Color.Red);
                    return;
                }
            }
            else
            {
                targetName = args[0];
            }

            foreach (Player player in Main.player)
            {
                if (player == null || !player.active)
                    continue;

                if (player.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    lastPlayer = player;

                    var mod = ModContent.GetInstance<CTG2>();
                    var packet = mod.GetPacket();
                    packet.Write((byte)MessageType.RequestPlayerPing);
                    packet.Write(player.whoAmI);
                    //packet.Write(true);
                    packet.Send();
                    return;
                }
            }

            caller.Reply($"No player named '{targetName}' was found.", Color.Red);
        }
    }
}