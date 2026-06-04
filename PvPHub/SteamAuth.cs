using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Chat;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Mono.Cecil.Cil;

namespace PvPHubIntegration;


[Autoload(Side = ModSide.Server)] //class should be loaded on the server only
public static class SteamAuth
{
    public static Dictionary<int, ulong> steamIDMapping = new Dictionary<int, ulong>(); //store player whoAmI and their steamid
    [JITWhenModsEnabled("PvPHub")] //only load this method if PvPHub is enabled
    public static bool TryGetSteamId(Player player, out ulong steamId)
    {
        steamId = 0;

        if (!ModLoader.TryGetMod("PvPHub", out _))
            return false;

        //pvphub already has steamauth implemented, so we can just get the steamid from there
        try{
                var steamPlayer =
            player.GetModPlayer<global::PvPHub.Common.Authentication.AuthenticatedPlayer>();
                if (steamPlayer.SteamId.HasValue)
        {
            steamId = steamPlayer.SteamId.Value;
            return true;
        }
        }catch (Exception e) //throw rather than crashing tmodloader
        {
            ModContent.GetInstance<CTG2.CTG2>().Logger.Error("Failed to get AuthenticatedPlayer from PvPHub. Is PvPHub updated? " + e);
            return false;
        }
        return false;
    }

}

public class SteamAuthHelper : ModPlayer
{
        private const int CheckIntervalTicks = 60; // Check every 60 ticks (1 second)
        private const int MaxChecks = 30;
        private bool foundSteamId;
        private int checksAttempted;
        private int timer;

    public override void PostUpdate()
    {
        if(!Main.dedServ) //only run on server
            return;

        if (foundSteamId) //don't check if we already have it
            return;

        if (checksAttempted >= MaxChecks) //if we get to MaxChecks there is likely some other issue
            return;

        timer++; 
        if (timer < CheckIntervalTicks) //check in intervals only
            return;
        timer = 0; //reset timer
        checksAttempted++; //increment checks attempted

        foreach (Player player in Main.player)
        {
            if (player.active)
            {
                if (SteamAuth.TryGetSteamId(player, out ulong steamId))
                {
                    foundSteamId = true;

                    /*
                    ChatHelper.BroadcastChatMessage(
                    NetworkText.FromLiteral($"Server: found SteamID {steamId} for {player.name}"),
                    Color.Yellow
                ); only use for debugging, don't want everybody to see steamID */ 

                SteamAuth.steamIDMapping[player.whoAmI] = steamId;
                    // Do steamid logic here
                    
                }
            }
        }
    }

    public override void PlayerDisconnect()
    {
        if(!Main.dedServ) //only run on server
            return;

        foundSteamId = false;
        SteamAuth.steamIDMapping.Remove(Player.whoAmI); //remove player from mapping on disconnect
        checksAttempted = 0;
        timer = 0;
    }
}