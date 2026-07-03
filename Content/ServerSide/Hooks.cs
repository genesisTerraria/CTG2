using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CTG2.Content.ServerSide;

//This class will store various timing dependant hooks
public static class Hooks
{
    // This hook is fired the instant every queued player is simultaneously in the world
    // It is called by NeatQueueTeamAssignmentSystem.PlayersReady
    // Future match-ready logic (maps/bans/pick timers) should be called from here
    public static void OnFullRosterJoined() //this is called on the server
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("All players in the queue have arrived."),
            Color.LightGreen);

        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral($"Both teams have 3 minutes to ban. ({GetEasternTimeStamp()} EST)"),
            Color.LightGreen);

        ModContent.GetInstance<CTG2>().Logger.Info(
            "[NeatQueue] OnFullRosterJoined fired. All queued players are currently in the world.");

        var assignmentSystem = ModContent.GetInstance<NeatQueueTeamAssignmentSystem>();
        var captainWhoAmIs = assignmentSystem.GetOnlineCaptainWhoAmIs();

        List<string> captainNames = new();
        foreach (int whoAmI in captainWhoAmIs)
        {captainNames.Add(Main.player[whoAmI].name);}
        // umm check if the list is empty first????
        ChatHelper.BroadcastChatMessage(
            NetworkText.FromLiteral("The captains are " + string.Join(", ", captainNames) + "."),
            Color.LightGreen); //make sure this format is good

        int banTimer = 3 * 60 * 60; // 3 minutes in ticks (60 ticks per second)
        //use one universal timer for bans

        //tell the captains to show the banUI
        foreach (int whoAmI in captainWhoAmIs)
        {
            ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
            packet.Write((byte)MessageType.SendBanUI);
            packet.Write(true); // true for showing the ban UI
            packet.Send(toClient: whoAmI);
        }


        // rollRandomMap(MapsAllowedInScrimsDict)

        // A system for storing the score of the scrim has to be made and has to
        // account for things like admins ending the round themselves

        // substitutes currently aren't accounted for the api has to be setup to
        // recieve the substitute event and then substitute has to be replace in
        // the mapping
    }

    private static string GetEasternTimeStamp()
    {
        DateTime easternNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GetEasternTimeZone());
        return easternNow.ToString("h:mm tt", CultureInfo.InvariantCulture);
    }

    private static TimeZoneInfo GetEasternTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
    }

    //Perhaps we can check if PlayersReady = true and make a new hook that is called from
    // GameManager.EndGame(). Here we would check if PlayersReady = true. If they are then
    // A scrim is active then increase the score within the hook
}
