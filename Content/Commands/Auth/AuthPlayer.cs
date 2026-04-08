using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CTG2.Content.Commands.Auth
{
    public class AuthPlayer : ModPlayer
    {
        public bool IsLoggedIn = false;
        public bool IsAdmin = false;
        public string Username = "";
        public static readonly System.Collections.Generic.HashSet<string> Admins = new()
        {
            "genesis", "crono", "fearghal", "brud", "tig"
        };

        public override void Initialize()
        {
            IsLoggedIn = false;
            IsAdmin = false;
            Username = "";
        }

        public override void PostUpdate()
        {
            // Continuously enforce the name so Terraria can't overwrite it
            if (IsLoggedIn && Username != "")
                Player.name = Username;

            if (!IsLoggedIn)
            {
                Player.AddBuff(BuffID.Webbed, 60);
            }
        }
    }
}