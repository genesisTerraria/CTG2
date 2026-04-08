using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CTG2.Content.Commands.Auth
{
    public class AuthPlayer : ModPlayer
    {
        public bool IsLoggedIn = false;
        public string Username = "";

        public override void Initialize()
        {
            IsLoggedIn = false;
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