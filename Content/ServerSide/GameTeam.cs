using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using CTG2.Content;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace CTG2.Content.ServerSide;

public class GameTeam
{   
    public List<Player> Players { get; set; }
    public Vector2 ClassLocation { get; set; }
    public Vector2 BaseLocation { get; set; }
    
    private int TeamColor { get; set; }
    
    public GameTeam(Vector2 classLocation, Vector2 baseLocation, int teamColor)
    {
        ClassLocation = classLocation;
        BaseLocation = baseLocation;
        TeamColor = teamColor;
        Players = new List<Player>();
    }

    public void UpdateTeam()
    {   
        Players.Clear();
        foreach (Player ply in Main.player)
        {   
            if (ply.team == TeamColor) Players.Add(ply);
        }
    }
    
    // locks team/pvp
    public void EnforceTeam()
    {
        foreach (Player ply in Players)
        {
            if (!ply.active) continue;
            
            ply.team = TeamColor;
            
            // Force PvP on for all team members
            if (!ply.hostile)
            {
                ply.hostile = true;
                NetMessage.SendData(MessageID.TogglePVP, -1, -1, null, ply.whoAmI);
                Console.WriteLine($"GameTeam: Enabled PvP for {ply.name} on team {TeamColor}");
            }
            
            NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, ply.whoAmI, TeamColor);
        }
    }

    public void StartMatch()
    {   
        var mod = ModContent.GetInstance<CTG2>();
        ModPacket packet = mod.GetPacket();
        packet.Write((byte)MessageType.ServerGameStart);
        packet.Send();
        
        
        
        foreach (Player ply in Players)
        {   
            ply.Teleport(ClassLocation);
            ply.position = ClassLocation;
            
            int tpX = (int)ClassLocation.X;
            int tpY = (int)ClassLocation.Y;
            CTG2.WebPlayer(ply.whoAmI,60);
            ModPacket packet2 = mod.GetPacket();
            packet2.Write((byte)MessageType.ServerTeleport);
            packet2.Write(ply.whoAmI);
            packet2.Write(tpX);
            packet2.Write(tpY);
            packet2.Send(toClient: ply.whoAmI);
            Console.WriteLine($"teleported {ply.name} to {ply.position}!");
        }
    }

    public void SendToBase()
    {   
        // all this does now is handle spawn
        var mod = ModContent.GetInstance<CTG2>();
        var manager = ModContent.GetInstance<GameManager>();
        
        foreach (Player ply in Players)
        {
            // Call endPlayerClassSelection directly on the server instead of sending a packet
            manager.endPlayerClassSelection(ply.whoAmI);
            
            Console.WriteLine($"GameTeam.SendToBase: Called endPlayerClassSelection for player {ply.whoAmI}");
            
            int tpX = (int)BaseLocation.X;
            int tpY = (int)BaseLocation.Y;
            
            int spawnPosX = tpX / 16;
            int spawnPosY = tpY / 16;

            ply.SpawnX = spawnPosX;
            ply.SpawnY = spawnPosY;
            
            ModPacket packet3 = mod.GetPacket();
            packet3.Write((byte)MessageType.ServerSetSpawn);
            packet3.Write(spawnPosX);
            packet3.Write(spawnPosY);
            packet3.Send(toClient: ply.whoAmI);
        }
        
    }

    // public void PauseTeam()
    // {
    //     foreach (Player ply in Players)
    //     {
    //         int buffTicks = 15 * 60;
    //         ply.AddBuff(BuffID.Webbed, buffTicks);
    //         NetMessage.SendData(MessageID.AddPlayerBuff, -1, -1, null, ply.whoAmI, BuffID.Webbed, buffTicks);
    //     }
    // }
    
}