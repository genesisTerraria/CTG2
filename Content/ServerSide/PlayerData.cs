 using Terraria.ID;
using Terraria.Chat;
using Terraria.Localization;
using CTG2.Content.ClientSide;
using Terraria.Enums;
using CTG2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ClassesNamespace;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.Tracing;


namespace CTG2.Content.ServerSide;


public class PlayerData
{
    public string myUUID;
    public bool myAdminStatus;
    public int chatFlairItemId = 58;

    public PlayerData() { }  
    public PlayerData(string myUUID)
    {
        this.myUUID = myUUID;
    }
    public void setAdmin()
    {
        myAdminStatus = true;
    }

    public void NetSend(BinaryWriter w)
    {
        w.Write(myUUID);
        w.Write(myAdminStatus);
        w.Write(chatFlairItemId);
    }
    public static PlayerData NetReceive(BinaryReader r) {
        return new PlayerData
        {
            myUUID = r.ReadString(),
            myAdminStatus = r.ReadBoolean(),
            chatFlairItemId = r.ReadInt32()
        };
    }

}

public class TempModPlayerDupe : ModPlayer{
    public override void OnEnterWorld()
    {
        ModPacket packet = ModContent.GetInstance<CTG2>().GetPacket();
        packet.Write((byte)MessageType.RequestDictAddAndSync);
        packet.Write((string)Main.clientUUID);
        packet.Send();

        ModPacket p = ModContent.GetInstance<CTG2>().GetPacket();
        p.Write((byte)MessageType.RegisterUuid);
        p.Write(Main.clientUUID);
        p.Send();
    }
}
