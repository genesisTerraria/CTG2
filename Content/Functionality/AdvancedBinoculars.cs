using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using CTG2.Content.Configs;


namespace CTG2.Content.Functionality
{
    public class CameraSystem : ModSystem
    {
        int state = 0;
        bool recentlyPressed = false;
        Vector2 mouseOffset;
        Vector2 offset;
        Vector2 targetPosition;
        private int lerpTimer;
        private int lerpDurationTicks;
        private Vector2 finalPosition;


        public override void ModifyScreenPosition()
        {
            Player player = Main.LocalPlayer;

            AdvancedBinocularsAI(player);
        }


        public void AdvancedBinocularsAI(Player player)
        {
            bool pressedOnce = CTG2.AdvancedBinocularsKeybind.JustPressed && !recentlyPressed;
            bool isHoldingKey = CTG2.AdvancedBinocularsKeybind.Current;

            switch (state)
            {
                case 0:
                    ApplyCameraLerpInward();

                    if (pressedOnce)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        
                        finalPosition = Main.screenPosition;

                        float actualDistance = finalPosition.Length();
                        float totalDistance = targetPosition.Length();

                        float ratio = (totalDistance > 0) ? (actualDistance / totalDistance) : 0;

                        float seconds = ModContent.GetInstance<CTG2Config>().CameraLerpSecondsOutward * ratio;
                        lerpDurationTicks = (int)(seconds * 60f);
                        lerpTimer = 0;

                        state = 1;
                    }
                    break;

                case 1:
                    mouseOffset = Main.MouseWorld - player.Center;
                    
                    offset = 0.9f * mouseOffset;
                    targetPosition = player.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f + offset;

                    ApplyCameraLerpOutward();

                    if (!isHoldingKey)
                    {
                        state = 2; 
                    }
                    break;

                case 2:
                    targetPosition = player.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f + offset;

                    ApplyCameraLerpOutward();

                    if (pressedOnce || !ModContent.GetInstance<CTG2Config>().EnabledCameraLock)
                    {
                        SoundEngine.PlaySound(SoundID.MenuTick);

                        finalPosition = Main.screenPosition;

                        float actualDistance = finalPosition.Length();
                        float totalDistance = targetPosition.Length();

                        float ratio = (totalDistance > 0) ? (actualDistance / totalDistance) : 0;

                        float seconds = ModContent.GetInstance<CTG2Config>().CameraLerpSecondsInward * ratio;
                        lerpDurationTicks = (int)(seconds * 60f);
                        lerpTimer = 0;

                        state = 0;

                        break;
                    }

                    break;
            }

            recentlyPressed = isHoldingKey;
        }


        private void ApplyCameraLerpOutward()
        {
            if (lerpDurationTicks <= 0)
            {
                Main.screenPosition = targetPosition;
            }
            else
            {
                lerpTimer++;
                // Use a simple Lerp for smoothness, or just snap if timer finishes
                float progress = MathHelper.Clamp((float)lerpTimer / lerpDurationTicks, 0f, 1f);
                Main.screenPosition = Vector2.Lerp(finalPosition, targetPosition, progress);
            }
        }

        private void ApplyCameraLerpInward()
        {
            if (lerpDurationTicks > 0)
            {
                lerpTimer++;
                // Use a simple Lerp for smoothness, or just snap if timer finishes
                float progress = MathHelper.Clamp((float)lerpTimer / lerpDurationTicks, 0f, 1f);
                Main.screenPosition = Vector2.Lerp(finalPosition, Main.screenPosition, progress);
            }
        }
    }
}
