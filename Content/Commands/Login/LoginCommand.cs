using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria.ID;

namespace CTG2.Content.Commands
{
    public class LoginCommand : ModCommand
    {
        private const string Password = "prayrisarat"; 
        public override CommandType Type => CommandType.Chat;
        public override string Command => "login";
        public override string Description => "Log in as admin.";
        public override string Usage => "/login <password>";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();

            if (args.Length != 1)
            {
                caller.Reply("Usage: /login <password>", Color.Red);
                return;
            }

            if (modPlayer.IsAdmin)
            {
                caller.Reply("You are already logged in!", Color.Red);
                return;
            }

            if (args[0] == Password)
            {
                modPlayer.IsAdmin = true;
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                var pkt = ModContent.GetInstance<CTG2>().GetPacket();
                pkt.Write((byte)MessageType.RequestMakeMeAdmin);
                pkt.Write(Main.clientUUID);   // caller’s UUID
                pkt.Send();}
                caller.Reply("You are now logged in as admin.", Color.Green);
            }
            else
            {
                string inputPassword = args[0];
                var mod = ModContent.GetInstance<CTG2>();
                using (var stream = mod.GetFileStream("Content/Commands/Login/password.hash"))
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
                        caller.Player.GetModPlayer<AdminPlayer>().IsAdmin = true;
                        caller.Reply("You are now logged in as admin.", Color.Green);
                    }
                    else
                    {
                        caller.Reply("Wrong password.", Color.Red);
                    }
                }
            }
        }
    }
}
