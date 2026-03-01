using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;


namespace CTG2.Content.Functionality
{
    public class CameraSystem : ModSystem
    {
        int state = 0;
        bool recentlyPressed = false;
        Vector2 mouseOffset;
        Vector2 offset;
        Vector2 targetPosition;


        public override void ModifyScreenPosition()
        {
            Player player = Main.LocalPlayer;

            AdvancedBinocularsAI(player);
        }


        public void AdvancedBinocularsAI(Player player)
        {
            switch(state)
            {
                case 0:
                    if (CTG2.AdvancedBinocularsKeybind.JustPressed && !recentlyPressed)
                    {
                        state = 1;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }

                    break;
                
                case 1:
                    mouseOffset = Main.MouseWorld - player.Center;
                    state = 2;

                    break;
                
                case 2:
                    offset = 0.9f * mouseOffset;
                    targetPosition = player.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f + offset;

                    Main.SetCameraLerp(0, 0);
                    Main.screenPosition = Vector2.Lerp(Main.screenPosition, targetPosition, 1f);
                    
                    if (CTG2.AdvancedBinocularsKeybind.JustPressed && !recentlyPressed)
                    {
                        state = 0;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    
                    break;
            }

            recentlyPressed = CTG2.AdvancedBinocularsKeybind.JustPressed;
        }
    }
}
