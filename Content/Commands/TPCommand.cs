using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Collections.Generic;

namespace CTG2.Content.Commands
{
    public class TPCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "tp";
        public override string Description => "Teleport to the specified location";
        public override string Usage => "/tp";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AdminPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }
            Player player = caller.Player;
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet1 = mod.GetPacket();
            if (args.Length == 1)
            {
                if (args[0] == "spawn")
                {
                    packet1.Write((byte)MessageType.RequestTeleport);
                    packet1.Write(player.whoAmI);
                    packet1.Write((int)13317);
                    packet1.Write((int)10855);
                    packet1.Send();
                }
                else if (args[0] == "bluebase")
                {
                    packet1.Write((byte)MessageType.RequestTeleport);
                    packet1.Write(player.whoAmI);
                    packet1.Write((int)12349);
                    packet1.Write((int)10882);
                    packet1.Send();
                }
                else if (args[0] == "redbase")
                {
                    packet1.Write((byte)MessageType.RequestTeleport);
                    packet1.Write(player.whoAmI);
                    packet1.Write((int)20381);
                    packet1.Write((int)10881);
                    packet1.Send();
                }
            }
            else if (args.Length == 2)
            {

                string targetName = args[0].ToLower();
                string targetName2 = args[1].ToLower();

                Player tpthisplayer = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName);
                Player tothisplayer = Main.player.FirstOrDefault(p => p.active && p.name.ToLower() == targetName2);
                //Vector2 tempPosition = tpthisplayer.position;

                if (tpthisplayer == null || tothisplayer == null)
                {
                    caller.Reply("One or both players not found.", Color.Red);
                    return;
                }
                else
                {
                    Vector2 tempPosition2 = tothisplayer.position;
                    packet1.Write((byte)MessageType.RequestTeleport);
                    packet1.Write(tpthisplayer.whoAmI);
                    packet1.Write((int)tempPosition2.X);
                    packet1.Write((int)tempPosition2.Y);
                    packet1.Send();
                }

                
            }


            else { Main.NewText("Wrong usage"); }
            


        }
    }
}