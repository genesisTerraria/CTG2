using System;
using Terraria;
using Terraria.ModLoader;
using CTG2.Content;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;
using System.Collections;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Chat;
using Terraria.Localization;
using CTG2.Content.ClientSide;
using CTG2.Content.Buffs;
using Terraria.Enums;
using ClassesNamespace;

namespace CTG2.Content.Functionality;

public class WebUnloggedPlayers : ModSystem
{
    public override void PostUpdateWorld()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            var player = Main.player[i];

            var modPlayer = player.GetModPlayer<Content.Commands.Auth.AuthPlayer>();

            if (!modPlayer.IsLoggedIn)
            {
                var mod = ModContent.GetInstance<CTG2>();

                ModPacket packet = mod.GetPacket();
                packet.Write((byte)MessageType.SyncAddBuff);
                packet.Write(player.whoAmI);
                packet.Write(BuffID.Webbed);
                packet.Write(120);
                packet.Send();
            }
        }
    }
}