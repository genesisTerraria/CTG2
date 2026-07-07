using System;
using System.Collections.Generic;
using System.Linq;
using CTG2.Content.ServerSide;
using CTG2.Content.Classes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Chat;
using ReLogic.Graphics;


namespace CTG2.Content.ClientSide;

public class UIManager : ModSystem
{   
    private UserInterface classInterface;
    private ClassUI classUIState;
    private UserInterface damageBoardInterface;
    private DamageBoardUI damageBoardState;
    private UserInterface modAdminInterface;
    private ModAdminUI modAdminState;
    private UserInterface banInterface;
    private BanUI banUIState;


    public override void OnWorldLoad()
    {
        // 1) Create your UIState
        classUIState = new ClassUI();
        damageBoardState = new DamageBoardUI();
        modAdminState = new ModAdminUI();
        banUIState = new BanUI();

        // 2) Create a UserInterface and attach your state
        classInterface = new UserInterface();
        classInterface.SetState(classUIState);
        damageBoardInterface = new UserInterface();
        damageBoardInterface.SetState(damageBoardState);
        modAdminInterface = new UserInterface();
        modAdminInterface.SetState(modAdminState);
        banInterface = new UserInterface();
        banInterface.SetState(banUIState);
    }
    
    public override void UpdateUI(GameTime gameTime)
    {
        // Only update the class interface when ShowClassUI is true
        if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowClassUI)
        {
            classInterface?.Update(gameTime);
        }

        if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowModUI)
        {
            modAdminInterface?.Update(gameTime);
        }

        if (BanUI.Visible)
        {
            banInterface?.Update(gameTime);
        }

        if (DamageBoardData.Visible)
        {
            damageBoardInterface?.Update(gameTime);
        }
    }
    
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (index != -1)
        {
            
            int insertIndex = index;

            if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowClassUI)
            {
                layers.Insert(insertIndex, new LegacyGameInterfaceLayer(
                    "CTG2: Class Selection UI",
                    delegate
                    {
                        // Draw your UI
                        classInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                insertIndex++;
            }

            if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowModUI)
            {
                layers.Insert(insertIndex, new LegacyGameInterfaceLayer(
                    "CTG2: Admin Mod UI",
                    delegate
                    {
                        modAdminInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
                insertIndex++;
            }

            if (BanUI.Visible)
            {
                layers.Insert(insertIndex, new LegacyGameInterfaceLayer(
                    "CTG2: Class Ban UI",
                    delegate
                    {
                        banInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            
            layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                "CTG2: Match Timer",
                delegate
                {
                    if (BanTimer.Visible)
                    {
                        BanTimer.Draw();
                        return true;
                    }

                    DrawTopUI();
                    DrawAbilityCooldown();

                    if (DamageBoardData.Visible)
                    {
                        damageBoardInterface?.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    private void DrawTopUI()
    {
        int matchStage = GameInfo.matchStage;

        if (matchStage == 0 || matchStage == 3) return;

        int matchTime = GameInfo.matchTime;
        int secondsElapsed = matchTime / 60 - GameInfo.matchStartTime / 60;
        int minutesElapsed = secondsElapsed / 60;
        int remainder = secondsElapsed % 60;

        int capDifference = GameInfo.blueCaptures - GameInfo.redCaptures;

        string timeText;
        if (matchTime < GameInfo.matchStartTime)
            timeText = $"{(int)(GameInfo.matchStartTime / 60) - matchTime / 60}s";
        else if (Main.LocalPlayer.GetModPlayer<PlayerManager>().classSelectionTimer > 0
            && Main.LocalPlayer.GetModPlayer<PlayerManager>().playerState == PlayerManager.PlayerState.ClassSelection)
            timeText = $"{(int)(Main.LocalPlayer.GetModPlayer<PlayerManager>().classSelectionTimer) / 60}s";
        else if (secondsElapsed > 600 && capDifference != 0)
            timeText = "Kill gem holder!";
        else
            timeText = $"{minutesElapsed}:{remainder.ToString("D2")}";

        DynamicSpriteFont font = FontAssets.MouseText.Value;
        float scale = 1f;
        int boxPaddingX = 14;
        int boxPaddingY = 4;
        int boxGap = 6;
        int topY = 10;
        int hpBarHeight = 20;
        int hpBarPaddingTop = -4;    // gap between progress bar box bottom and hp bar top
        int hpBarPaddingBottom = 10; // gap between hp bar bottom and box bottom
        float hpTextOffsetY = 10f; // tweak to center text vertically in hp bar, generally height / 2

        // --- Gem carrier HP ---
        float blueHpFraction = 1f;
        float redHpFraction  = 1f;
        string blueHpText = "At Base";
        string redHpText  = "At Base";

        var gm = ModContent.GetInstance<GameManager>();
        if (gm?.BlueGem != null && gm.BlueGem.IsHeld)
        {
            Player carrier = Main.player[gm.BlueGem.HeldBy];
            if (carrier != null && carrier.active && carrier.statLifeMax2 > 0)
            {
                blueHpFraction = Math.Clamp((float)carrier.statLife / carrier.statLifeMax2, 0f, 1f);
                blueHpText = $"{carrier.name}: {carrier.statLife}/{carrier.statLifeMax2}";
            }
        }
        if (gm?.RedGem != null && gm.RedGem.IsHeld)
        {
            Player carrier = Main.player[gm.RedGem.HeldBy];
            if (carrier != null && carrier.active && carrier.statLifeMax2 > 0)
            {
                redHpFraction = Math.Clamp((float)carrier.statLife / carrier.statLifeMax2, 0f, 1f);
                redHpText = $"{carrier.name}: {carrier.statLife}/{carrier.statLifeMax2}";
            }
        }

        Vector2 timeSize  = font.MeasureString(timeText) * scale;
        string redStr     = $"{GameInfo.redCaptures}";
        string blueStr    = $"{GameInfo.blueCaptures}";
        Vector2 redSize   = font.MeasureString(redStr) * scale;
        Vector2 blueSize  = font.MeasureString(blueStr) * scale;

        int timeW  = (int)timeSize.X  + boxPaddingX * 2;
        int timeH  = (int)timeSize.Y  + boxPaddingY * 2;
        int redW   = (int)redSize.X   + boxPaddingX * 2;
        int blueW  = (int)blueSize.X  + boxPaddingX * 2;
        int boxH   = timeH;

        var totalBars = 20;
        var blueBars = (int)Math.Clamp(Math.Round(GameInfo.blueGemX / 100f * totalBars), 0, totalBars);
        var redBars  = (int)Math.Clamp(Math.Round(GameInfo.redGemX  / 100f * totalBars), 0, totalBars);
        string blueGemIndicator = "[c/0077B6:⬢" + new string('▮', blueBars) + "]" + "[i:1524]" + "[c/FFFFFF:" + new string('▮', totalBars - blueBars) + "⬢]";
        string redGemIndicator  = "[c/FFFFFF:⬢" + new string('▮', totalBars - redBars) + "]" + "[i:1526]"  + "[c/FF0000:" + new string('▮', redBars) + "⬢]";

        Vector2 blueBarSize = ChatManager.GetStringSize(font, blueGemIndicator, Vector2.One);
        Vector2 redBarSize  = ChatManager.GetStringSize(font, redGemIndicator,  Vector2.One);

        // Side boxes taller to fit HP bar
        int sideBoxH = boxH + hpBarPaddingTop + hpBarHeight + hpBarPaddingBottom;
        int blueBarW = (int)blueBarSize.X + boxPaddingX * 2;
        int redBarW  = (int)redBarSize.X  + boxPaddingX * 2;

        int totalW = blueBarW + boxGap + blueW + boxGap + timeW + boxGap + redW + boxGap + redBarW;
        int startX = (Main.screenWidth - totalW) / 2;

        Rectangle blueBarBox = new Rectangle(startX,                                                                        topY, blueBarW, sideBoxH);
        Rectangle blueBox    = new Rectangle(startX + blueBarW + boxGap,                                                    topY, blueW,    boxH);
        Rectangle timeBox    = new Rectangle(startX + blueBarW + boxGap + blueW + boxGap,                                   topY, timeW,    boxH);
        Rectangle redBox     = new Rectangle(startX + blueBarW + boxGap + blueW + boxGap + timeW + boxGap,                  topY, redW,     boxH);
        Rectangle redBarBox  = new Rectangle(startX + blueBarW + boxGap + blueW + boxGap + timeW + boxGap + redW + boxGap,  topY, redBarW,  sideBoxH);

        Texture2D pixel = TextureAssets.MagicPixel.Value;

        Main.spriteBatch.Draw(pixel, blueBarBox, new Color(0, 60, 140) * 0.85f);
        DrawBorder(Main.spriteBatch, pixel, blueBarBox, new Color(50, 150, 255), 2);

        Main.spriteBatch.Draw(pixel, blueBox, new Color(0, 60, 140) * 0.85f);
        DrawBorder(Main.spriteBatch, pixel, blueBox, new Color(50, 150, 255), 2);

        Main.spriteBatch.Draw(pixel, timeBox, Color.Black * 0.75f);
        DrawBorder(Main.spriteBatch, pixel, timeBox, Color.White, 2);

        Main.spriteBatch.Draw(pixel, redBox, Color.DarkRed * 0.85f);
        DrawBorder(Main.spriteBatch, pixel, redBox, Color.Red, 2);

        Main.spriteBatch.Draw(pixel, redBarBox, Color.DarkRed * 0.85f);
        DrawBorder(Main.spriteBatch, pixel, redBarBox, Color.Red, 2);

        // Progress bar text (top portion of side boxes)
        Vector2 blueBarTextPos = new Vector2(blueBarBox.X + (blueBarBox.Width - blueBarSize.X) / 2, blueBarBox.Y + (boxH - blueBarSize.Y) / 2);
        Vector2 blueTextPos    = new Vector2(blueBox.X    + (blueBox.Width    - blueSize.X)    / 2, blueBox.Y    + (blueBox.Height - blueSize.Y)    / 2);
        Vector2 timeTextPos    = new Vector2(timeBox.X    + (timeBox.Width    - timeSize.X)    / 2, timeBox.Y    + (timeBox.Height - timeSize.Y)    / 2);
        Vector2 redTextPos     = new Vector2(redBox.X     + (redBox.Width     - redSize.X)     / 2, redBox.Y     + (redBox.Height  - redSize.Y)     / 2);
        Vector2 redBarTextPos  = new Vector2(redBarBox.X  + (redBarBox.Width  - redBarSize.X)  / 2, redBarBox.Y  + (boxH - redBarSize.Y) / 2);

        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, blueGemIndicator, blueBarTextPos, Color.White, 0, Vector2.Zero, Vector2.One);
        Utils.DrawBorderString(Main.spriteBatch, blueStr,  blueTextPos, Color.White, scale);
        Utils.DrawBorderString(Main.spriteBatch, timeText, timeTextPos, Color.White, scale);
        Utils.DrawBorderString(Main.spriteBatch, redStr,   redTextPos,  Color.White, scale);
        ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, redGemIndicator, redBarTextPos, Color.White, 0, Vector2.Zero, Vector2.One);

        // HP bars
        int hpBarY = topY + boxH + hpBarPaddingTop;
        int blueHpInnerW = blueBarBox.Width - boxPaddingX * 2;
        int redHpInnerW  = redBarBox.Width  - boxPaddingX * 2;

        Rectangle blueHpBg   = new Rectangle(blueBarBox.X + boxPaddingX, hpBarY, blueHpInnerW, hpBarHeight);
        Rectangle blueHpFill = new Rectangle(blueBarBox.X + boxPaddingX, hpBarY, (int)(blueHpInnerW * blueHpFraction), hpBarHeight);
        Main.spriteBatch.Draw(pixel, blueHpBg,   Color.Black * 0.6f);
        Main.spriteBatch.Draw(pixel, blueHpFill, new Color(50, 150, 255));
        DrawBorder(Main.spriteBatch, pixel, blueHpBg, new Color(50, 150, 255) * 0.8f, 1);

        Rectangle redHpBg   = new Rectangle(redBarBox.X + boxPaddingX, hpBarY, redHpInnerW, hpBarHeight);
        Rectangle redHpFill = new Rectangle(redBarBox.X + boxPaddingX, hpBarY, (int)(redHpInnerW * redHpFraction), hpBarHeight);
        Main.spriteBatch.Draw(pixel, redHpBg,   Color.Black * 0.6f);
        Main.spriteBatch.Draw(pixel, redHpFill, new Color(220, 50, 50));
        DrawBorder(Main.spriteBatch, pixel, redHpBg, new Color(220, 50, 50) * 0.8f, 1);

        Vector2 blueHpTextSize = font.MeasureString(blueHpText) * 0.85f;
        Vector2 redHpTextSize  = font.MeasureString(redHpText)  * 0.85f;

        Vector2 blueHpTextPos = new Vector2(
            blueHpBg.X + (blueHpBg.Width  - blueHpTextSize.X) / 2,
            blueHpBg.Y + (blueHpBg.Height / 2) - hpTextOffsetY);
        Vector2 redHpTextPos = new Vector2(
            redHpBg.X  + (redHpBg.Width   - redHpTextSize.X)  / 2,
            redHpBg.Y  + (redHpBg.Height  / 2) - hpTextOffsetY);

        Utils.DrawBorderString(Main.spriteBatch, blueHpText, blueHpTextPos, Color.White, 0.85f);
        Utils.DrawBorderString(Main.spriteBatch, redHpText,  redHpTextPos,  Color.White, 0.85f);
    }

    // Helper to draw a rectangle outline
    private void DrawBorder(SpriteBatch sb, Texture2D pixel, Rectangle rect, Color color, int thickness)
    {
        // Top
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - thickness, rect.Width, thickness), color);
        // Left
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        sb.Draw(pixel, new Rectangle(rect.Right - thickness, rect.Y, thickness, rect.Height), color);
    }

    private void DrawAbilityCooldown()
    {
        int matchStage = GameInfo.matchStage;
        if (matchStage == 0 || matchStage == 3) return;

        var abilities = Main.LocalPlayer.GetModPlayer<Abilities>();
        int cooldown = abilities.cooldown;

        var playerManager = Main.LocalPlayer.GetModPlayer<PlayerManager>();
        int selectedClass = playerManager.currentClass.AbilityID;

        int iconSize;
        int centerX;
        int centerY;
        Rectangle destRect;

        if (selectedClass == 1)
        {
            int cooldown2 = abilities.cooldown2;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/ArcherAbilityIcons/ArcherAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/ArcherAbilityIcons/ArcherAbility2").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 48;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else if (selectedClass == 4)
        {
            int cooldown2 = abilities.cooldown2;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/GladiatorAbilityIcons/GladiatorAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/GladiatorAbilityIcons/GladiatorAbility2").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 48;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else if (selectedClass == 2)
        {
            int cooldown2 = abilities.cooldown2;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/NinjaAbilityIcons/NinjaAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/NinjaAbilityIcons/NinjaAbility2").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 48;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else if (selectedClass == 3)
        {
            int cooldown2 = abilities.cooldown2;
            int cooldown3 = abilities.cooldown3;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/AlchemistAbilityIcons/AlchemistAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/AlchemistAbilityIcons/AlchemistAbility2").Value;
            Texture2D Ability3Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/AlchemistAbilityIcons/AlchemistAbility3").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 72;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw third ability icon
            if (cooldown3 <= 0)
            {
                Main.spriteBatch.Draw(Ability3Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability3Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown3 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else if (selectedClass == 4)
        {
            int cooldown2 = abilities.cooldown2;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/GladiatorAbilityIcons/GladiatorAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/GladiatorAbilityIcons/GladiatorAbility2").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 48;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else if (selectedClass == 15)
        {
            int cooldown2 = abilities.cooldown2;

            Texture2D Ability1Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/TreeAbilityIcons/TreeAbility1").Value;
            Texture2D Ability2Icon = ModContent.Request<Texture2D>("CTG2/Content/Classes/TreeAbilityIcons/TreeAbility2").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 48;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw first ability icon
            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability1Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            // Draw second ability icon
            if (cooldown2 <= 0)
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(Ability2Icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown2 / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }
        else
        {
            Texture2D icon = ModContent.Request<Texture2D>("CTG2/Content/ClientSide/AbilityIcon").Value;

            iconSize = 32;
            centerX = Main.screenWidth / 2 - 24;
            centerY = Main.screenHeight - 32;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);

            if (cooldown <= 0)
            {
                Main.spriteBatch.Draw(icon, destRect, Color.White);
            }
            else
            {
                Main.spriteBatch.Draw(icon, destRect, Color.Gray * 0.6f);

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                string cdText = $"{(int)Math.Ceiling(cooldown / 60f)}";
                Vector2 textSize = font.MeasureString(cdText);
                Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
                Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
            }

            centerX += 48;
            destRect = new Rectangle(centerX - iconSize / 2, centerY - iconSize / 2, iconSize, iconSize);
        }

        Texture2D potionIcon = ModContent.Request<Texture2D>("CTG2/Content/Classes/GeneralAbilityIcons/PotionSickness").Value;

        // Draw potion cooldown timer
        int buffIndex = Main.LocalPlayer.FindBuffIndex(BuffID.PotionSickness);
        float secondsRemaining = 0;

        if (buffIndex != -1)
        {
            int ticksRemaining = Main.LocalPlayer.buffTime[buffIndex];
            // 60 ticks = 1 second
            secondsRemaining = ticksRemaining;
        }

        if (secondsRemaining <= 0)
        {
            Main.spriteBatch.Draw(potionIcon, destRect, Color.White);
        }
        else
        {
            Main.spriteBatch.Draw(potionIcon, destRect, Color.Gray * 0.6f);

            DynamicSpriteFont font = FontAssets.MouseText.Value;
            string cdText = $"{(int)Math.Ceiling(secondsRemaining / 60f)}";
            Vector2 textSize = font.MeasureString(cdText);
            Vector2 textPos = new Vector2(centerX - textSize.X / 2f, centerY - textSize.Y / 2f);
            Utils.DrawBorderString(Main.spriteBatch, cdText, textPos, Color.White);
        }
    }
}
