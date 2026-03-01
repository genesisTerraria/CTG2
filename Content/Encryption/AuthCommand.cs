using Terraria.ModLoader;
using Terraria;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CTG2.Content.ClientSide;

namespace CTG2.Content.Encryption
{
    public class AuthCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "auth";
        public override string Usage => "/auth <password>";
        public override string Description => "Authenticate with a passcode.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length != 1)
            {
                caller.Reply("Usage: /auth <password>");
                return;
            }

            string inputPassword = args[0];
            var mod = ModContent.GetInstance<CTG2>();
            using (var stream = mod.GetFileStream("Content/Encryption/password.hash"))
            using (var reader = new StreamReader(stream))
            {
                string storedHash = reader.ReadToEnd().Trim();


                if (storedHash == null)
                {
                    caller.Reply("Password hash not found.");
                    return;
                }

                string inputHash = PasswordHelper.HashPassword(inputPassword);

                if (inputHash == storedHash)
                {
                    caller.Player.GetModPlayer<TestPlayer>().playerAttribute = !caller.Player.GetModPlayer<TestPlayer>().playerAttribute;
                    caller.Reply("Authentication successful.");
                }
                else
                {
                    caller.Reply("Authentication failed.");
                }
            }

        }
    }
}
