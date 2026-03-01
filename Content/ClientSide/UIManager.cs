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


namespace CTG2.Content.ClientSide;

public class UIManager : ModSystem
{   
    private UserInterface classInterface;
    private ClassUI classUIState;
    
    
    public override void OnWorldLoad()
    {
        // 1) Create your UIState
        classUIState = new ClassUI();

        // 2) Create a UserInterface and attach your state
        classInterface = new UserInterface();
        classInterface.SetState(classUIState);
    }
    
    public override void UpdateUI(GameTime gameTime)
    {
        // Only update the class interface when ShowClassUI is true
        if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowClassUI)
        {
            classInterface?.Update(gameTime);
        }
    }
    
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (index != -1)
        {
            
            if (Main.LocalPlayer.GetModPlayer<PlayerManager>().ShowClassUI)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "CTG2: Class Selection UI",
                    delegate
                    {
                        // Draw your UI
                        classInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
            
            layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                "CTG2: Match Timer",
                delegate
                {
                    if (true)
                    {
                        DrawMatchTimer();
                    }
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }


    private void DrawMatchTimer()
    {
        string timeText = "";
        Vector2 timeRow = new Vector2(Main.screenWidth - 320, 350);
        Vector2 blueGemRow = new Vector2(Main.screenWidth - 320, 500);
        Vector2 blueGemRow2 = new Vector2(Main.screenWidth - 320, 550);
        Vector2 redGemRow = new Vector2(Main.screenWidth - 320, 600);
        Vector2 redGemRow2 = new Vector2(Main.screenWidth - 320, 650);
        
        Color textColor = Color.White;
        int matchStage = GameInfo.matchStage;
        if (matchStage == 0) return;
        
        int matchTime = GameInfo.matchTime;
        if (matchTime < GameInfo.matchStartTime)
        {
            timeText = $"Class selection ends in: {(int) (GameInfo.matchStartTime / 60) - matchTime / 60}s";
        }
        else if (Main.LocalPlayer.GetModPlayer<PlayerManager>().classSelectionTimer > 0 && Main.LocalPlayer.GetModPlayer<PlayerManager>().playerState == PlayerManager.PlayerState.ClassSelection)
        {
            timeText = $"Class selection ends in: {(int) (Main.LocalPlayer.GetModPlayer<PlayerManager>().classSelectionTimer) / 60}s";
        }
        else if (GameInfo.overtime)
        {
            timeText = $"Next capture wins the game!";
        }
        else
        {
            int secondsElapsed = matchTime / 60 - GameInfo.matchStartTime / 60;
            int secondsLeft = 60 * 10 - secondsElapsed;
            int minutesLeft = secondsLeft / 60;
            int remainder = secondsLeft % 60;
            timeText = $"Time left in match: {minutesLeft}:{remainder.ToString("D2")}";
        }
        /*GameInfo.blueGemCarrier;*/
        string blueGemStatus = GameInfo.blueGemCarrier;
        var redGemStatus = GameInfo.redGemCarrier;
        var blueGemPosition = GameInfo.blueGemX;
        var redGemPosition = GameInfo.redGemX;

        var totalBars = 20;
        var blueBars = (int)Math.Clamp(Math.Round(blueGemPosition / 100f * totalBars), 0, totalBars);
        var redBars = (int)Math.Clamp(Math.Round(redGemPosition / 100f * totalBars), 0, totalBars);
        // Draw Gem Position like in CTG
        string blueGemIndicator  = "[c/0077B6:⬢"+ new string('▮', blueBars) + "]" + "[i:1524]" + "[c/FFFFFF:"+ new string('▮', totalBars-blueBars) + "⬢]";

        string redGemIndicator = "[c/FFFFFF:⬢"+ new string('▮', totalBars-redBars) + "]" + "[i:1526]" + "[c/FF0000:"+ new string('▮', redBars) + "⬢]";
        
        if (!string.IsNullOrEmpty(timeText))
        {
            Utils.DrawBorderString(Main.spriteBatch, timeText, timeRow, textColor);
        }
        if (!string.IsNullOrEmpty(blueGemStatus))
        {
            Color blueColor = new Color(0, 119, 182);
            Utils.DrawBorderString(Main.spriteBatch, blueGemStatus, blueGemRow, blueColor);
        }
        if (!string.IsNullOrEmpty(redGemStatus))
        {
            Utils.DrawBorderString(Main.spriteBatch, redGemStatus, redGemRow, Color.Red);
        }
        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            blueGemIndicator,
            blueGemRow2,
            Color.White,
            0,
            Vector2.Zero,
            Vector2.One
        );
        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            redGemIndicator,
            redGemRow2,
            Color.White,
            0,
            Vector2.Zero,
            Vector2.One
        );
        // // Show gem carrier HP if gem is captured
        // Vector2 carrierHpPos = new Vector2(Main.screenWidth - 320, 700);

        // if (GameInfo.blueGemCarrier != "At Base" && !string.IsNullOrEmpty(GameInfo.blueGemCarrier))
        // {
    
        //     for (int i = 0; i < Main.maxPlayers; i++)
        //     {
        //         Player carrier = Main.player[i];
    
        //         if (carrier.active && carrier.name == GameInfo.blueGemCarrier)
        //         {
                
        //             string hpText = $"{carrier.name}: {carrier.statLife}/{carrier.statLifeMax2}";
        //             Utils.DrawBorderString(Main.spriteBatch, hpText, carrierHpPos, Color.Cyan);
                  
        //             carrierHpPos.Y += 40; 
                    
        //             break; 
        //         }
        //     }
        // }


        // if (GameInfo.redGemCarrier != "At Base" && !string.IsNullOrEmpty(GameInfo.redGemCarrier))
        // {
        //     for (int i = 0; i < Main.maxPlayers; i++)
        //     {
        //         Player carrier = Main.player[i];
        //         if (carrier.active && carrier.name == GameInfo.redGemCarrier)
        //         {
        //             string hpText = $"{carrier.name}: {carrier.statLife}/{carrier.statLifeMax2}";
        //             Utils.DrawBorderString(Main.spriteBatch, hpText, carrierHpPos, Color.Red);
        //             break; 
        //         }
        //     }
        // }

        // draw ability timer
        int cooldown = Main.LocalPlayer.GetModPlayer<Abilities>().cooldown;
        string abilText = $"Ability cooldown: {cooldown / 60}s";
        Vector2 abilPos = new Vector2(Main.screenWidth - 320, 450);
        Color abilCol = Color.Yellow;

        if (cooldown == 0)
        {
            Utils.DrawBorderString(Main.spriteBatch, "Ability ready!", abilPos, abilCol);
        }
        else
        {
            Utils.DrawBorderString(Main.spriteBatch, abilText, abilPos, abilCol);
        }

        //draw map name
        string mapText = $"Map: {GameInfo.mapName}";
        Vector2 mapPos = new Vector2(Main.screenWidth - 320, 400);
        Color mapCol = Color.White;

        Utils.DrawBorderString(Main.spriteBatch, mapText, mapPos, mapCol);

        //draw team sizes
        string teamText = $"Team size: [c/0077B6:{GameInfo.blueTeamSize}] v [c/FF0000:{GameInfo.redTeamSize}]";
        Vector2 teamTextPos = new Vector2(Main.screenWidth - 320, 325);

        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            teamText,
            teamTextPos,
            Color.White,
            0,
            Vector2.Zero,
            Vector2.One
        );

        //draw cap attempt counters
        string captures = $"Gem captures: [c/0077B6:{GameInfo.blueCaptures}] v [c/FF0000:{GameInfo.redCaptures}]";
        Vector2 capturesPos = new Vector2(Main.screenWidth - 320, 675);

        ChatManager.DrawColorCodedStringWithShadow(
            Main.spriteBatch,
            FontAssets.MouseText.Value,
            captures,
            capturesPos,
            Color.White,
            0,
            Vector2.Zero,
            Vector2.One
        );

        //draw cap progress counters
        // string progress = $"Furthest gem carry: [c/0077B6:{GameInfo.blueFurthest}%] v [c/FF0000:{GameInfo.redFurthest}%]";
        // Vector2 progressPos = new Vector2(Main.screenWidth - 320, 675);

        // ChatManager.DrawColorCodedStringWithShadow(
        //     Main.spriteBatch,
        //     FontAssets.MouseText.Value,
        //     progress,
        //     progressPos,
        //     Color.White,
        //     0,
        //     Vector2.Zero,
        //     Vector2.One
        // );

        //draw dirt timer
        int secondsPassed = matchTime / 60 - GameInfo.matchStartTime / 60;
        int secondsRemaining = 900 - secondsPassed;
        int dirtSeconds = secondsRemaining % 30;
        string dirtText = (secondsRemaining >= 600 && secondsRemaining < 900) ? $"Time left until dirt received: {dirtSeconds}s" : "No more dirt will be received!";
        Vector2 dirtRow = new Vector2(Main.screenWidth - 320, 375);
        if (matchStage == 2)
            Utils.DrawBorderString(Main.spriteBatch, dirtText, dirtRow, Color.White);
        //if (matchTime % 60 == 0) Main.NewText(GameInfo.blueGemCarrier);

        //draw kdr
        string kdrText = $"Kills: {Main.LocalPlayer.GetModPlayer<PlayerManager>().kills}  |  Deaths: {Main.LocalPlayer.GetModPlayer<PlayerManager>().deaths}  |  Damage: {Main.LocalPlayer.GetModPlayer<PlayerManager>().damage}";
        Vector2 kdrRow = new Vector2(Main.screenWidth - 320, 725);
        if (matchStage == 2)
            Utils.DrawBorderString(Main.spriteBatch, kdrText, kdrRow, Color.White);
    }
}
