using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace CTG2.Content.Commands
{
    public class ItemCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "item";
        public override string Description => "Gives an item by name or ID.";
        public override string Usage => "/item <itemNameOrID> [amount]";

public override void Action(CommandCaller caller, string input, string[] args)
{
    
    var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
    if (!modPlayer.IsAdmin)
    {
    caller.Reply("You must be an admin to use this command.", Color.Red);
    return;
    }

    if (args.Length == 0)
            {
                caller.Reply("Usage: /item <itemNameOrID> [amount]", Color.Red);
                return;
            }
    

    string itemName = "";
    int amount = 1;

    
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

                itemName = string.Join(" ", parts).Trim('"');
                
                
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out int amt))
                    amount = amt;

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
        itemName = args[0];

        if (args.Length > 1 && int.TryParse(args[1], out int amt))
            amount = amt;
    }

    Player player = caller.Player;
    int itemType = -1;

    
    if (int.TryParse(itemName, out int parsedID) && ContentSamples.ItemsByType.ContainsKey(parsedID))
    {
        itemType = parsedID;
    }
    else
    {
        
        string searchName = itemName.ToLowerInvariant();
        var match = ContentSamples.ItemsByType.Values
            .FirstOrDefault(item => item.Name.ToLowerInvariant() == searchName);

        if (match != null)
            itemType = match.type;
    }

    if (itemType != -1)
    {
        player.QuickSpawnItem(null, itemType, amount);
        caller.Reply($"Gave {amount}x {Lang.GetItemNameValue(itemType)}.", Color.Green);
    }
    else
    {
        caller.Reply($"Item '{itemName}' not found. Try exact name or item ID.", Color.Red);
    }
}

    }
}