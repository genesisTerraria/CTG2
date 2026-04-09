using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using System.Collections.Generic;

namespace CTG2.Content.Commands.Auth
{
    public class AuthPlayer : ModPlayer
    {
        public bool IsLoggedIn = false;
        public bool IsAdmin = false;
        public string Username = "";

        public static readonly HashSet<string> Admins = new()
        {
            "genesis", "crono", "fearghal", "brud", "tig", "chara"
        };

        public override void Initialize()
        {
            IsLoggedIn = false;
            IsAdmin = false;
            Username = "";
        }

        // Sync IsLoggedIn to all clients
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncAuthPlayer);
            packet.Write((byte)Player.whoAmI);
            packet.Write(IsLoggedIn);
            packet.Write(Username ?? "");
            packet.Send(toWho, fromWho);
        }

        public override void PostUpdate()
        {
            if (IsLoggedIn && Username != "")
                Player.name = Username;

            // Only apply webbed on the server/local — synced to other clients via packet
            // if (!IsLoggedIn && (Main.netMode != NetmodeID.MultiplayerClient || Player.whoAmI == Main.myPlayer))
            //     Player.AddBuff(BuffID.BrokenArmor, 60);
            //     Player.AddBuff(BuffID.WitheredWeapon, 60);
        }
    }
}