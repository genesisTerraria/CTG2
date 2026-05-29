using System;
using System.Linq;
using System.Text.RegularExpressions;
using CTG2.Content.Commands.Auth;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.Commands
{
    public class GiveCommand : ModCommand
    {
        private const int DefaultAmount = 9999;

        public override CommandType Type => CommandType.Chat;
        public override string Command => "give";
        public override string Description => "Gives an item by name or ID to a player.";
        public override string Usage => "/give \"<playerName>\" <itemNameOrID>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            var match = Regex.Match(input, @"^/give\s+""(.+?)""\s+(.+)$");
            if (!match.Success)
            {
                caller.Reply("Usage: /give \"<playerName>\" <itemNameOrID>", Color.Red);
                return;
            }

            string targetName = match.Groups[1].Value.ToLower();
            string itemName = match.Groups[2].Value.Trim().Trim('"');

            Player target = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName);
            if (target == null)
            {
                caller.Reply($"Player '{targetName}' not found.", Color.Red);
                return;
            }

            if (!TryGetItemType(itemName, out int itemType))
            {
                caller.Reply($"Item '{itemName}' not found. Try exact name or item ID.", Color.Red);
                return;
            }

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                CTG2.GiveItemToPlayer(target, itemType, DefaultAmount, 0);
            }
            else
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.GiveItemToKiller);
                packet.Write((byte)target.whoAmI);
                packet.Write(itemType);
                packet.Write(DefaultAmount);
                packet.Write((byte)0);
                packet.Send();
            }

            caller.Reply($"Gave {DefaultAmount}x {Lang.GetItemNameValue(itemType)} to {target.name}.", Color.Green);
        }

        private static bool TryGetItemType(string itemName, out int itemType)
        {
            itemType = -1;

            if (int.TryParse(itemName, out int parsedID) && ContentSamples.ItemsByType.ContainsKey(parsedID))
            {
                itemType = parsedID;
                return true;
            }

            string searchName = itemName.ToLowerInvariant();
            Item match = ContentSamples.ItemsByType.Values
                .FirstOrDefault(item => item.Name.ToLowerInvariant() == searchName);

            if (match == null)
                return false;

            itemType = match.type;
            return true;
        }
    }
}
