
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework.Input;
using Terraria.Chat;
using Terraria.Localization;


namespace CTG2.Content
{
    public class ChatPlayer : ModPlayer
    {
        public bool IsMuted = false;


        public override void PreUpdate()
        {
            if (IsMuted && !string.IsNullOrEmpty(Main.chatText))
            {
                Main.chatText = "";
                Main.NewText("You are muted and cannot chat.", Microsoft.Xna.Framework.Color.Red);
            }
           
        }
    }
} 
