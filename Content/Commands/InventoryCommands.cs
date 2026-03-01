using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using ClassesNamespace;

namespace CTG2.Content.Commands
{
    public class SaveInventoryCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "saveinventory";
        public override string Usage => "/saveinventory";
        public override string Description => "Saves current inventory to a JSON file";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            Player player = caller.Player;
            var inventoryData = new List<ItemData>();
            int count = 0;
            bool modded = false;

            foreach (Item item in player.inventory)
            {
                if (item != null)
                {
                    modded = item.type >= 5546;

                    inventoryData.Add(new ItemData
                    {
                        Name = item.Name,
                        Type = item.type,
                        Stack = item.stack,
                        Prefix = item.prefix,
                        Slot = count,
                        Modded = modded
                    });
                }

                count++;
            }

            foreach (Item item in player.armor)
            {
                if (item != null)
                {
                    modded = item.type >= 5546;

                    inventoryData.Add(new ItemData
                    {
                        Name = item.Name,
                        Type = item.type,
                        Stack = item.stack,
                        Prefix = item.prefix,
                        Slot = count,
                        Modded = modded
                    });
                }

                count++;
            }

            foreach (Item item in player.miscEquips)
            {
                if (item != null)
                {
                    modded = item.type >= 5546;

                    inventoryData.Add(new ItemData
                    {
                        Name = item.Name,
                        Type = item.type,
                        Stack = item.stack,
                        Prefix = item.prefix,
                        Slot = count,
                        Modded = modded
                    });
                }

                count++;
            }

            foreach (Item item in player.miscDyes)
            {
                if (item != null)
                {
                    modded = item.type >= 5546;

                    inventoryData.Add(new ItemData
                    {
                        Name = item.Name,
                        Type = item.type,
                        Stack = item.stack,
                        Prefix = item.prefix,
                        Slot = count,
                        Modded = modded
                    });
                }

                count++;
            }

            string[] inputSplit = input.Split(' ');

            var inventory = new CtgClass();
            inventory.HealthPoints = player.statLifeMax2;
            inventory.ManaPoints = player.statManaMax2;
            inventory.InventoryItems = inventoryData;

            string json = JsonSerializer.Serialize(inventory, new JsonSerializerOptions { WriteIndented = true });
            string path = Path.Combine(Main.SavePath, "ModSources", "CTG2", "Content", "Classes");
            Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, $"{inputSplit[1]}.json");
            File.WriteAllText(filePath, json);

            Main.NewText($"Inventory saved to {filePath}", Color.LightGreen);
        }
    }


    public class LoadInventoryCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "loadinventory";
        public override string Usage => "/loadinventory";
        public override string Description => "Loads saved inventory from a JSON file";

        private int GetItemIDByName(string itemName)
        {
            for (int i = 5546; i < ItemLoader.ItemCount; i++)
            {
                ModItem modItem = ItemLoader.GetItem(i);
                if (modItem != null && modItem.Name.Equals(itemName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        public override void Action(CommandCaller caller, string input, string[] args) {

            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            string[] inputSplit = input.Split(' ');
            string modifiedInput = inputSplit[1];

            Player player = caller.Player;
            string path = Path.Combine(Main.SavePath, "ModSources", "CTG2", "Content", "Classes");
            string filePath = Path.Combine(path, $"{modifiedInput}.json");

            if (!File.Exists(filePath))
            {
                Main.NewText($"Inventory file not found.", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            string json = File.ReadAllText(filePath);
            CtgClass inventoryData;

            try
            {
                inventoryData = JsonSerializer.Deserialize<CtgClass>(json);
            }
            catch
            {
                Main.NewText("Failed to load or parse inventory file.", Microsoft.Xna.Framework.Color.Red);
                return;
            }

            List<ItemData> allItemData = inventoryData.InventoryItems;

            for (int b = 0; b < player.inventory.Length; b++)
            {
                var itemData = allItemData[b];
                Item newItem = new Item();
                if (itemData.Modded)
                    newItem.SetDefaults(GetItemIDByName(itemData.Name));
                else
                    newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                player.inventory[b] = newItem;
            }

            for (int c = 0; c < player.armor.Length; c++)
            {
                var itemData = allItemData[player.inventory.Length + c];
                Item newItem = new Item();
                if (itemData.Modded)
                    newItem.SetDefaults(GetItemIDByName(itemData.Name));
                else
                    newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                player.armor[c] = newItem;
            }

            for (int d = 0; d < player.miscEquips.Length; d++)
            {
                var itemData = allItemData[player.inventory.Length + player.armor.Length + d];
                Item newItem = new Item();
                if (itemData.Modded)
                    newItem.SetDefaults(GetItemIDByName(itemData.Name));
                else
                    newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                player.miscEquips[d] = newItem;
            }

            for (int e = 0; e < player.miscDyes.Length; e++)
            {
                var itemData = allItemData[player.inventory.Length + player.armor.Length + player.miscEquips.Length + e];
                Item newItem = new Item();
                if (itemData.Modded)
                    newItem.SetDefaults(GetItemIDByName(itemData.Name));
                else
                    newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                player.miscDyes[e] = newItem;
            }

            player.trashItem = new Item();
            Main.mouseItem = new Item();

            player.GetModPlayer<ClassSystem>().currentHP = inventoryData.HealthPoints;
            player.GetModPlayer<ClassSystem>().currentMana  = inventoryData.ManaPoints;

            Main.NewText("Inventory loaded successfully.", Microsoft.Xna.Framework.Color.LightGreen);
        }
    }
}
