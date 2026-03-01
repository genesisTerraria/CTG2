using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Chat;
using System.Collections.Generic;
using System;


namespace CTG2.Content.Functionality
{
    public class NoTombsAndKillMessages : ModSystem
    {
        public override void Load()
        {
            Terraria.Chat.On_ChatHelper.DisplayMessage += (orig, text, color, messageAuthor) =>
            {
                var x = ModContent.GetInstance<CTG2>();
                if (CTG2.UuidByWhoAmI.TryGetValue(messageAuthor, out var uuid) && x.PlayerDataDictionary.TryGetValue(uuid, out var pd) && pd.myAdminStatus)
                {
                    color = new Color(140, 43, 79, 255);
                }

                else
                {
                    if (Main.player[messageAuthor].team == 3) { color = Color.PowderBlue; }
                    if (Main.player[messageAuthor].team == 1) { color = Color.OrangeRed; }
                }
                
                orig(text, color, messageAuthor);
            };
            Terraria.GameContent.UI.Chat.On_NameTagHandler.GenerateTag += (orig, name) =>
            {
                var p = Array.Find(Main.player, pl => pl.active && pl.name == name);
                if (p != null &&
                    CTG2.UuidByWhoAmI.TryGetValue(p.whoAmI, out var uuid) && ModContent.GetInstance<CTG2>().PlayerDataDictionary.TryGetValue(uuid, out var data))
                {
                    if (data.myAdminStatus && data.chatFlairItemId ==58) 
                        return "[i:5554] [Admin] " + name + ":";

                    if (data.chatFlairItemId >= 0)
                        return "[i:" + data.chatFlairItemId + "] " + name + ":";
                }

                return name + ":"; // fallback
            };

        
            Terraria.On_Player.DropTombstone += (orig, self, coins, deathText, hitDir) => { };

            Terraria.DataStructures.On_PlayerDeathReason.GetDeathText += (orig, self, deadPlayerName) =>
        {
            return NetworkText.FromLiteral("");
            
        };

            Terraria.On_Player.KillMe += (orig, self, damageSource, dmg, hitDirection, pvp) => {
                // Get killer info
                int killerIndex = damageSource.SourcePlayerIndex;

                // Get victim team and name
                string victimName = self.team == 1 ? $"[c/FF0000:{self.name}]" : 
                                    self.team == 3 ? $"[c/0000FF:{self.name}]" :
                                    self.name;

                // Check if killer is a player and if so get name and team
                if (killerIndex >= 0)
                {
                    string killerName = "???";
                    Player killer = Main.player[killerIndex];
                    killerName = killer.team == 1 ? $"[c/FF0000:{killer.name}]" : //Red team color (color is a little off rn)
                                killer.team == 3 ? $"[c/0000FF:{killer.name}]" :  //Blue team color (off as well)
                                killer.name;

                    // Try to get damage source wep ID if null (most cases for some reason) just use held item like vanilla
                    int? itemID = damageSource.SourceItem?.netID;
                    if (itemID == null && killerIndex >= 0)
                        itemID = Main.player[killerIndex].HeldItem?.netID;

                    // Msg to send
                    string msg = $"{killerName} ([i:{itemID ?? 0}]) {victimName}";

                    // Run on server only and catch null statements caused by desync
                    if (Main.netMode == NetmodeID.Server && itemID != 0 && itemID.HasValue)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.White);
                }
                else if (self.HasBuff(BuffID.OnFire))
                {
                    string msg = $"[i:3184] {victimName}";
                    if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.OrangeRed);
                }
                else if (damageSource.SourceOtherIndex >= 0) //lava death
                {
                    string msg = $"[i:207] {victimName}";
                    if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.White);
                }
                else if (damageSource.SourceNPCIndex >= 0 && damageSource.SourceNPCIndex < Main.maxNPCs) // Killed by an NPC (slimer)
                {
                    string msg = $"[i:5091] {victimName}";
                    if (Main.netMode == NetmodeID.Server)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), Color.White);
                }

                //This makes sure death is still processed normally 
                orig(self, damageSource, dmg, hitDirection, pvp);
            };
        }
    }
}
