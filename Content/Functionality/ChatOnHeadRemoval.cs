using Terraria;
using Terraria.ModLoader;
using CTG2.Content.ClientSide;

namespace CTG2.Content.Functionality
{
    public class RemoveChatOverhead : ModSystem
    {
        
        public override void PostUpdatePlayers()
        {
            Main.LocalPlayer.chatOverhead.timeLeft = 0;


        }
        }
    }

