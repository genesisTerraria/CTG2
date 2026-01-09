using System;
using Terraria;
using Terraria.ModLoader;
using CTG2.Content;
using Microsoft.Xna.Framework;
using Terraria.WorldBuilding;
using System.Collections;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Chat;
using Terraria.Localization;
using CTG2.Content.ClientSide;
using Terraria.Enums;
using ClassesNamespace;

namespace CTG2.Content.ServerSide;

public class GameManager : ModSystem
{
    // True when 
    public bool IsGameActive { get; private set; }
    public bool HasRoundStarted { get; private set; }

    public int MatchTime { get; private set; }

    public GameTeam BlueTeam { get; private set; }
    public GameTeam RedTeam { get; private set; }

    public int blueTeamSize = 0;
    public int redTeamSize = 0;

    private string blueGemCarrier = "At Base";
    private string redGemCarrier = "At Base";
    private string blueCarrierName = "";
    private string redCarrierName = "";

    bool hasStartedEarly = false;

    public Dictionary<int, GameTeam> intToTeam;

    public Gem BlueGem { get; set; }
    public Gem RedGem { get; set; }

    private bool blueWasHeld = false;
    private bool redWasHeld = false;

    int intPercentageBlue = 0;
    int intPercentageRed = 0;

    public Queue<MapTypes> mapQueue = new();

    public bool pause = false;
    public bool killonce = true;

    public int matchStartTime = 1800;

    public bool pubsConfig = false; // this is used at the start of every game
    public bool scrimsConfig = false; // same as pubs, but no auto-team assignment, late joining, or auto-game starting
    public bool rngConfig = false; // same as pubs, but different classes

    public bool isOvertime = false;
    private int overtimeTimer = 0;
    private const int OVERTIME_DURATION = 60 * 2 * 60; // 2 minutes in ticks (60 ticks/sec)

    public GameMap Map { get; private set; }

    private string mapName = "";

    private int pauseTimer = 0;

    public int blueAttempts = 0;
    public int redAttempts = 0;
    public float blueFurthest = 0;
    public float redFurthest = 0;
    private int matchStage = 0;
    private bool blueHoldCounted = false;
    private bool redHoldCounted = false;

    private int winner = 0;

    private bool endGameCalled = false;
    private bool resetEndGame = false;

    // Spectator tracking
    private Dictionary<int, bool> playerSpectatorStatus = new Dictionary<int, bool>();
    private Dictionary<int, int> spectatorOriginalTeams = new Dictionary<int, int>();
    private Vector2 spectatorSpawnPoint = new Vector2((13332 + 19316) / 2, 11000); // Center area for spectators

    // New game delay tracking
    public bool isWaitingForNewGame = false;
    private int newGameTimer = 0;
    private int blueGemFireworkTimer = 0;
    private int redGemFireworkTimer = 0;
    private const int FIREWORK_INTERVAL = 120;
    private int _currentMusicChoice = 0;


    public static void FillLavaInDesignatedArea()
    {
        //hard coded coords for the right.wld if you are on the wrong wld it will spawn lava in wrong spot!!!
        int xMin = 13702 / 16;
        int yMin = 11719 / 16;
        int xMax = 19030 / 16;
        int yMax = 11814 / 16;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                Tile tile = Framing.GetTileSafely(x, y);


                if (!tile.HasTile)
                {
                    tile.LiquidAmount = 255;
                    tile.LiquidType = LiquidID.Lava;
                    Liquid.AddWater(x, y); // Schedule a liquid update at this position
                    WorldGen.SquareTileFrame(x, y, true);
                }
            }
        }

        Liquid.UpdateLiquid();
    }
    public static void FillHoneyForMap(MapTypes map)
    {
        // Define honey zones for specific maps
        List<Rectangle> honeyZones = new();

        switch (map)
        {
            case MapTypes.Kraken:
                honeyZones.Add(new Rectangle(16357 / 16, 10965 / 16, (16376 - 16357 + 1) / 16, 1));
                honeyZones.Add(new Rectangle(1022, 685, 2, 1));
                break;

            case MapTypes.Stalactite:
                honeyZones.Add(new Rectangle(16357 / 16, 10965 / 16, (16376 - 16357 + 1) / 16, 1));
                honeyZones.Add(new Rectangle(1022, 685, 2, 1));
                break;

            default:
                return;
        }

        foreach (var zone in honeyZones)
        {
            for (int x = zone.Left; x < zone.Right; x++)
            {
                for (int y = zone.Top; y < zone.Bottom; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (!tile.HasTile)
                    {
                        tile.LiquidAmount = 255;
                        tile.LiquidType = LiquidID.Honey;
                        Liquid.AddWater(x, y);
                        WorldGen.SquareTileFrame(x, y, true);
                    }
                }
            }
        }



        Liquid.UpdateLiquid();
    }
    public static void ClearHoneyForMap()
    {
        List<Rectangle> honeyZones = new();

        honeyZones.Add(new Rectangle(1022, 685, 10, 10));

        foreach (var zone in honeyZones)
        {
            for (int x = zone.Left; x < zone.Right; x++)
            {
                for (int yPos = zone.Top; yPos < zone.Bottom; yPos++)
                {
                    Tile tile = Framing.GetTileSafely(x, yPos);

                    if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Honey)
                    {
                        tile.LiquidAmount = 0;
                        tile.LiquidType = 0;
                        Liquid.AddWater(x, yPos);
                        WorldGen.SquareTileFrame(x, yPos, true);
                    }
                }
            }
        }

        Liquid.UpdateLiquid();
    }


    public override void OnWorldLoad()
    {
        // TODO: Re-Paste the Arena on world load (in case it gets destroyed by an admin).
        Main.spawnTileX = 13317 / 16; //spawn coords for the world ONLY on world load (this is changed later)
        Main.spawnTileY = 10855 / 16;

        BlueGem = new Gem(new Vector2(CTG2.config.BlueGem[0], CTG2.config.BlueGem[1]), 3);
        RedGem = new Gem(new Vector2(CTG2.config.RedGem[0], CTG2.config.RedGem[1]), 1);

        BlueTeam = new GameTeam(new Vector2(CTG2.config.BlueSelect[0], CTG2.config.BlueSelect[1]), new Vector2(CTG2.config.BlueBase[0], CTG2.config.BlueBase[1]), 3);
        RedTeam = new GameTeam(new Vector2(CTG2.config.RedSelect[0], CTG2.config.RedSelect[1]), new Vector2(CTG2.config.RedBase[0], CTG2.config.RedBase[1]), 1);

        intToTeam = new Dictionary<int, GameTeam>
        {
            { 3, BlueTeam },
            { 1, RedTeam }
        };

        // map paste takes Tile coords. spawn point takes pixel coords.
        Map = new GameMap(CTG2.config.MapPaste[0], CTG2.config.MapPaste[1]);
        GameMap.PreloadAllMaps();
        IsGameActive = false;
        MatchTime = 0;
    }

    public void StartGame()
    {
        var mod = ModContent.GetInstance<CTG2>();

        isWaitingForNewGame = false;
        IsGameActive = true;
        hasStartedEarly = false;
        endGameCalled = false;
        MatchTime = 0;
        
        ModPacket packetMatchTime = mod.GetPacket();
        packetMatchTime.Write((byte)MessageType.ServerGameUpdate);
        packetMatchTime.Write(MatchTime);
        packetMatchTime.Send();

        matchStartTime = 1800;

        ModPacket packetMatchStartTime = mod.GetPacket();
        packetMatchStartTime.Write((byte)MessageType.UpdateMatchStartTime);
        packetMatchStartTime.Write(1800);
        packetMatchStartTime.Send();

        if (pubsConfig)
        { 
            PubsConfig();
        }

        if (scrimsConfig)
        {
            ScrimsConfig();
        }

        if (rngConfig)
        {
            PubsConfig();
            RngConfig();
        }

        BlueGem.Reset();
        RedGem.Reset();
        bool isMapPicked = mapQueue.TryDequeue(out MapTypes result);

        blueGemCarrier = "At Base";
        ModPacket packetBlueGemCarrier = mod.GetPacket();
        packetBlueGemCarrier.Write((byte)MessageType.UpdateBlueGemCarrier);
        packetBlueGemCarrier.Write(blueGemCarrier);
        packetBlueGemCarrier.Write("");
        packetBlueGemCarrier.Send();

        redGemCarrier = "At Base";
        ModPacket packetRedGemCarrier = mod.GetPacket();
        packetRedGemCarrier.Write((byte)MessageType.UpdateRedGemCarrier);
        packetRedGemCarrier.Write(redGemCarrier);
        packetRedGemCarrier.Write("");
        packetRedGemCarrier.Send();

        BlueTeam.UpdateTeam();
        RedTeam.UpdateTeam();
        foreach (Player player in Main.player)
        {
            if (!player.active) continue;

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.ServerGameStart);
            packet.Send(toClient: player.whoAmI);

            if (player.team == 0)
            {
                SetPlayerSpectator(player.whoAmI, true);
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} is not on a team!"), Microsoft.Xna.Framework.Color.Olive);
                Console.WriteLine($"Auto-spectating player {player.name} (not on a team)");
                continue; // Use continue instead of return to handle all players
            }

            // Directly call startPlayerClassSelection instead of sending a packet

            startPlayerClassSelection(player.whoAmI, true);

        }

        if (isMapPicked)
        {
            ClearHoneyForMap();
            Map.LoadMap(result);
            mapName = result.ToString();

            ModPacket packetMapName = mod.GetPacket();
            packetMapName.Write((byte)MessageType.UpdateMapName);
            packetMapName.Write(mapName);
            packetMapName.Send();

            if (result == MapTypes.Kraken || result == MapTypes.Stalactite)
            {
                FillHoneyForMap(result);
            }
            FillLavaInDesignatedArea();
        }
        else
        {
            var randomMap = (MapTypes)CTG2.randomGenerator.Next(0, 7);
            ClearHoneyForMap();
            Map.LoadMap(randomMap);
            mapName = randomMap.ToString();

            ModPacket packetMapName = mod.GetPacket();
            packetMapName.Write((byte)MessageType.UpdateMapName);
            packetMapName.Write(mapName);
            packetMapName.Send();

            FillLavaInDesignatedArea();
            if (randomMap == MapTypes.Kraken || randomMap == MapTypes.Stalactite)
            {
                FillHoneyForMap(randomMap);
            }
        }
    }

    // Pauses/Unpauses game
    public void PauseGame()
    {
        pause = true;
    }
    public void UnpauseGame()
    {
        pause = false;
    }

    public void EndGame()
    {
        var mod = ModContent.GetInstance<CTG2>();

        Console.WriteLine("GameManager: Starting EndGame sequence");

        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] Game has ended!"), Microsoft.Xna.Framework.Color.Yellow);
        
        switch (winner)
        {
            case 0:
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Furthest gem carry: [c/0000FF:{blueFurthest}%] v [c/FF0000:{redFurthest}%]"), Microsoft.Xna.Framework.Color.Yellow);
                break;
            case 1:
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Furthest gem carry: [c/0000FF:100%] v [c/FF0000:{redFurthest}%]"), Microsoft.Xna.Framework.Color.Yellow);
                break;
            case 2:
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Furthest gem carry: [c/0000FF:{blueFurthest}%] v [c/FF0000:100%]"), Microsoft.Xna.Framework.Color.Yellow);
                break;
        }
        winner = 0;

        matchStage = 0;
        MatchTime = 0;
        intPercentageBlue = 0;
        intPercentageRed = 0;
        blueGemCarrier = "Waiting for new game...";
        redGemCarrier = "Waiting for new game...";
        isOvertime = false;
        overtimeTimer = 0;
        mapName = "";
        blueTeamSize = 0;
        redTeamSize = 0;
        matchStartTime = 0;
        blueAttempts = 0;
        redAttempts = 0;
        blueFurthest = 0;
        redFurthest = 0;
        blueCarrierName = "";
        redCarrierName = "";

        resetEndGame = true;
        endGameCalled = true;
        IsGameActive = false;
        HasRoundStarted = false;
        pause = false;

        // Reset gems
        BlueGem.Reset();
        RedGem.Reset();
        
        ModPacket packetIsCaptured = mod.GetPacket();
        packetIsCaptured.Write((byte)MessageType.UpdateIsCaptured);
        packetIsCaptured.Write(1);
        packetIsCaptured.Write(false);
        packetIsCaptured.Send();

        ModPacket packetIsHeld = mod.GetPacket();
        packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
        packetIsHeld.Write(1);
        packetIsHeld.Write(false);
        packetIsHeld.Send();

        ModPacket packetHeldBy = mod.GetPacket();
        packetHeldBy.Write((byte)MessageType.UpdateHeldBy);
        packetHeldBy.Write(1);
        packetHeldBy.Write(-1);
        packetHeldBy.Send();

        ModPacket packetIsCaptured2 = mod.GetPacket();
        packetIsCaptured2.Write((byte)MessageType.UpdateIsCaptured);
        packetIsCaptured2.Write(2);
        packetIsCaptured2.Write(false);
        packetIsCaptured2.Send();

        ModPacket packetIsHeld2 = mod.GetPacket();
        packetIsHeld2.Write((byte)MessageType.UpdateIsHeld);
        packetIsHeld2.Write(2);
        packetIsHeld2.Write(false);
        packetIsHeld2.Send();

        ModPacket packetHeldBy2 = mod.GetPacket();
        packetHeldBy2.Write((byte)MessageType.UpdateHeldBy);
        packetHeldBy2.Write(2);
        packetHeldBy2.Write(-1);
        packetHeldBy2.Send();

        Console.WriteLine("GameManager: Reset gems");

        ModPacket packet = mod.GetPacket();
        packet.Write((byte)MessageType.SyncGameInformation);
        packet.Write(matchStage);
        packet.Write(MatchTime);
        packet.Write(isOvertime);
        packet.Write(overtimeTimer);
        packet.Write(intPercentageBlue);
        packet.Write(intPercentageRed);
        packet.Write(blueGemCarrier);
        packet.Write(redGemCarrier);
        packet.Write(mapName);
        packet.Write(blueTeamSize);
        packet.Write(redTeamSize);
        packet.Write(matchStartTime);
        packet.Write(blueAttempts);
        packet.Write(redAttempts);
        packet.Write(blueFurthest);
        packet.Write(redFurthest);
        packet.Write(blueCarrierName);
        packet.Write(redCarrierName);
        packet.Send();

        // Handle all active players
        foreach (Player player in Main.player)
        {
            if (!player.active) continue;

            Console.WriteLine($"GameManager: Processing player {player.whoAmI} ({player.name}) for game end");

            // Reset player state to None
            PlayerManager.GetPlayerManager(player.whoAmI).playerState = PlayerManager.PlayerState.None;

            ModPacket statePacket = mod.GetPacket();
            statePacket.Write((byte)MessageType.UpdatePlayerState);
            statePacket.Write(player.whoAmI);
            statePacket.Write((byte)PlayerManager.PlayerState.None);
            statePacket.Send(toClient: player.whoAmI);

            Color teamColor = (player.team == 1) ? Color.Red : Color.Blue;
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name}'s stats: {PlayerManager.GetPlayerManager(player.whoAmI).kills} kills  |  {PlayerManager.GetPlayerManager(player.whoAmI).deaths} deaths  |  {PlayerManager.GetPlayerManager(player.whoAmI).damage} damage"), teamColor);

            PlayerManager.GetPlayerManager(player.whoAmI).kills = 0;
            PlayerManager.GetPlayerManager(player.whoAmI).deaths = 0;
            PlayerManager.GetPlayerManager(player.whoAmI).damage = 0;

            ModPacket kdrPacket = mod.GetPacket();
            kdrPacket.Write((byte)MessageType.UpdatePlayerKDR);
            kdrPacket.Write(player.whoAmI);
            kdrPacket.Write(0);
            kdrPacket.Write(0);
            kdrPacket.Write(0);
            kdrPacket.Send(toClient: player.whoAmI);

            // Clear any spectator status
            player.ghost = false;
            player.respawnTimer = 0;

            // Reset ability attributes
            player.GetModPlayer<Abilities>().cooldown = 0;
            player.GetModPlayer<Abilities>().class4BuffTimer = 0;
            player.GetModPlayer<Abilities>().class4PendingBuffs = false;
            player.GetModPlayer<Abilities>().class6ReleaseTimer = -1;
            player.GetModPlayer<Abilities>().class7HitCounter = 0;
            player.GetModPlayer<Abilities>().class8HP = 0;
            player.GetModPlayer<Abilities>().psychicActive = false;
            player.GetModPlayer<Abilities>().class12SwapTimer = -1;
            player.GetModPlayer<Abilities>().class12ClosestDist = 99999;
            player.GetModPlayer<Abilities>().class12ClosestPlayer = null;
            player.GetModPlayer<Abilities>().class15AbilityTimer = -1;
            player.GetModPlayer<Abilities>().mutantState = 1;
            
            ModPacket abilityPacket = mod.GetPacket();
            abilityPacket.Write((byte)MessageType.SyncAbilityAttributes);
            abilityPacket.Write(player.whoAmI);
            abilityPacket.Write(0);
            abilityPacket.Write(0);
            abilityPacket.Write(false);
            abilityPacket.Write(-1);
            abilityPacket.Write(0);
            abilityPacket.Write(0);
            abilityPacket.Write(false);
            abilityPacket.Write(-1);
            abilityPacket.Write(99999);
            abilityPacket.Write(-1);
            abilityPacket.Write(1);
            abilityPacket.Send(toClient: player.whoAmI);

            // Reset classsystem attributes
            player.GetModPlayer<ClassSystem>().blockCounter = 1800;
            player.GetModPlayer<ClassSystem>().bombCounter = 1200;

            ModPacket classSystemPacket = mod.GetPacket();
            classSystemPacket.Write((byte)MessageType.SyncClassSystemAttributes);
            classSystemPacket.Write(player.whoAmI);
            classSystemPacket.Write(1800);
            classSystemPacket.Write(1200);
            classSystemPacket.Send(toClient: player.whoAmI);

            // Remove from spectator tracking if they were spectating
            if (playerSpectatorStatus.GetValueOrDefault(player.whoAmI, false))
            {
                ModPacket spectatorPacket = mod.GetPacket();
                spectatorPacket.Write((byte)MessageType.ServerSpectatorUpdate);
                spectatorPacket.Write(player.whoAmI);
                spectatorPacket.Write(false);
                spectatorPacket.Send(toClient: player.whoAmI);
            }

            // Teleport all players to spectator area
            // web first
            CTG2.WebPlayer(player.whoAmI, 240);

            //can write out bool for player winner in the future to only tp losers to top
            ModPacket teleportPacket = mod.GetPacket();
            teleportPacket.Write((byte)MessageType.ServerTeleport);
            teleportPacket.Write(player.whoAmI);
            teleportPacket.Write((int)13317);
            teleportPacket.Write((int)10855);
            teleportPacket.Send(toClient: player.whoAmI);

            // Clear inventory and reset player stats
            player.statLife = player.statLifeMax;
            player.statMana = player.statManaMax;

            // Clear all player inventory and equipment
            //ClearPlayerInventory(player);
            ModPacket clearInvPacket = mod.GetPacket();
            clearInvPacket.Write((byte)MessageType.ClearInventory);
            clearInvPacket.Write(player.whoAmI);
            clearInvPacket.Send();

            Console.WriteLine($"GameManager: Reset player {player.whoAmI} state and teleported to spectator area");
        }

        // Clear all tracking dictionaries
        playerSpectatorStatus.Clear();
        spectatorOriginalTeams.Clear();

        // Update team lists
        BlueTeam.UpdateTeam();
        RedTeam.UpdateTeam();

        Console.WriteLine("GameManager: Cleared all spectator tracking and updated teams");

        // Send ServerGameEnd packet to all clients
        ModPacket endPacket = mod.GetPacket();
        endPacket.Write((byte)MessageType.ServerGameEnd);
        endPacket.Send();

        // Broadcast game end message
        //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] Game has ended! New game starting in 15 seconds..."), Microsoft.Xna.Framework.Color.Yellow);

        Console.WriteLine("GameManager: EndGame sequence completed");
        ClearFloatingItems();

        // Start the 15-second timer for new game
        isWaitingForNewGame = true;
        newGameTimer = 15 * 60; // 15 seconds * 60 ticks per second
    }

    // Runs every frame while game running. Runs all gem checks, draws timer and gem status.
    public void UpdateGame()
    {
        if (pause)
        {
            return;
        }
        // force set team/pvp
        BlueTeam.EnforceTeam();
        RedTeam.EnforceTeam();

        var mod = ModContent.GetInstance<CTG2>();

        // Additional PvP enforcement
        //EnsureAllPlayersHavePvP();

        if (MatchTime == matchStartTime)
        {
            // this code will handle class selection and stuff
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"class selection is ending"), Microsoft.Xna.Framework.Color.Olive);

            BlueTeam.SendToBase();
            RedTeam.SendToBase();
        }
        // Increase match duration by 1 tick
        MatchTime++;

        if (MatchTime >= matchStartTime)
        {
            // Updates holding/capturing status of both gems.
            BlueGem.Update(RedGem, RedTeam.Players);
            RedGem.Update(BlueGem, BlueTeam.Players);

            // fire works test?

            if (BlueGem.IsHeld)
            {
                blueGemFireworkTimer++;
                if (!redHoldCounted)
                {
                    redAttempts++;

                    ModPacket packetRedAttempts = mod.GetPacket();
                    packetRedAttempts.Write((byte)MessageType.UpdateRedAttempts);
                    packetRedAttempts.Write(redAttempts);
                    packetRedAttempts.Send();

                    redHoldCounted = true;
                }

                if (blueGemFireworkTimer >= FIREWORK_INTERVAL)
                {
                    blueGemFireworkTimer = 0;
                    var holder = Main.player[BlueGem.HeldBy];

                    int projectileIndex = Projectile.NewProjectile(
                        Entity.GetSource_NaturalSpawn(),
                        holder.Center,
                        new Vector2(0f, -8f),
                        ProjectileID.RocketFireworkBlue,
                        0,
                        0f,
                        Main.myPlayer
                    );

                    if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectileIndex);
                    }
                }
            }
            else
            {
                blueGemFireworkTimer = 0;
                redHoldCounted = false;
            }

            if (RedGem.IsHeld)
            {
                redGemFireworkTimer++;
                if (!blueHoldCounted)
                {
                    blueAttempts++;

                    ModPacket packetBlueAttempts = mod.GetPacket();
                    packetBlueAttempts.Write((byte)MessageType.UpdateBlueAttempts);
                    packetBlueAttempts.Write(blueAttempts);
                    packetBlueAttempts.Send();

                    blueHoldCounted = true;
                }

                if (redGemFireworkTimer >= FIREWORK_INTERVAL)
                {
                    redGemFireworkTimer = 0;
                    var holder = Main.player[RedGem.HeldBy];

                    int projectileIndex = Projectile.NewProjectile(
                        Entity.GetSource_NaturalSpawn(),
                        holder.Center,
                        new Vector2(0f, -8f),
                        ProjectileID.RocketFireworkRed,
                        0,
                        0f,
                        Main.myPlayer
                    );

                    if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectileIndex);
                    }
                }
            }
            else
            {
                redGemFireworkTimer = 0; // Reset
                blueHoldCounted = false;
            }
        }
        else
        {
            bool endEarly = true;
            foreach (Player plyr in Main.player)
            {
                if ((plyr.team == 1 || plyr.team == 3) && plyr.active && !PlayerManager.GetPlayerManager(plyr.whoAmI).pickedClass)
                    endEarly = false;
            }

            if (endEarly && !hasStartedEarly && MatchTime < 1500)
            {
                var modCTG = ModContent.GetInstance<CTG2>();
                
                foreach (Player pp in Main.player)
                {
                    if ((pp.team == 1 || pp.team == 3) && pp.active)
                    {
                        PlayerManager.GetPlayerManager(pp.whoAmI).classSelectionTimer = 300;
                        ModPacket selPacket = modCTG.GetPacket();
                        selPacket.Write((byte)MessageType.SetClassSelectionTime);
                        selPacket.Write(pp.whoAmI);
                        selPacket.Write(300); // class selection time
                        selPacket.Send(toClient: pp.whoAmI);
                    }
                }

                hasStartedEarly = true;
                matchStartTime = MatchTime + 300;

                ModPacket packetMatchStartTime = mod.GetPacket();
                packetMatchStartTime.Write((byte)MessageType.UpdateMatchStartTime);
                packetMatchStartTime.Write(MatchTime + 300);
                packetMatchStartTime.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"All players have selected classes. Starting the game in 5 seconds!"), Microsoft.Xna.Framework.Color.Olive);
            }
        }

        // Send updated GameInfo to clients every 6 ticks (every 0.1s)

        ModPacket packetMatchTime = mod.GetPacket();
        packetMatchTime.Write((byte)MessageType.RequestMatchTime);
        packetMatchTime.Write((int)MatchTime);
        packetMatchTime.Send();

        blueTeamSize = 0;
        redTeamSize = 0;

        if (MatchTime % 6 == 0)
        {
            foreach (Player p in Main.player)
            {
                if (p.team == 1) redTeamSize++;
                else if (p.team == 3) blueTeamSize++;
            }

            ModPacket packetRedTeamSize = mod.GetPacket();
            packetRedTeamSize.Write((byte)MessageType.UpdateRedTeamSize);
            packetRedTeamSize.Write(redTeamSize);
            packetRedTeamSize.Send();

            ModPacket packetBlueTeamSize = mod.GetPacket();
            packetBlueTeamSize.Write((byte)MessageType.UpdateBlueTeamSize);
            packetBlueTeamSize.Write(blueTeamSize);
            packetBlueTeamSize.Send();
        }

        // Determine match stage based on game state
        if (MatchTime < matchStartTime && (matchStage == 0 || matchStage == 3))
        {
            matchStage = 1; // Class Selection phase
            ModPacket packetMatchStage = mod.GetPacket();
            packetMatchStage.Write((byte)MessageType.UpdateMatchStage);
            packetMatchStage.Write(matchStage);
            packetMatchStage.Send();
        }
        else if (MatchTime >= matchStartTime && matchStage == 1)
        {
            matchStage = 2; // Active Gameplay phase
            ModPacket packetMatchStage = mod.GetPacket();
            packetMatchStage.Write((byte)MessageType.UpdateMatchStage);
            packetMatchStage.Write(matchStage);
            packetMatchStage.Send();
        }

        //packet.Write(isOvertime);
        //packet.Write(isOvertime ? overtimeTimer : 0);

        // Blue and red Gem X positions
        var distBetweenGems = Math.Abs(RedGem.Position.X - BlueGem.Position.X);
        if (BlueGem.IsHeld && MatchTime % 6 == 0)
        {
            // send % of the way the gem is to enemy base as an integer
            var distFromGem = Main.player[BlueGem.HeldBy].position.X - BlueGem.Position.X;
            intPercentageBlue = (int)Math.Round(distFromGem / distBetweenGems * 100);
            intPercentageBlue = Math.Clamp(intPercentageBlue, 0, 100);
            float floatPercentage = (float)Math.Round(Math.Clamp(distFromGem / distBetweenGems * 100, 0f, 100f), 2);
            if (floatPercentage > redFurthest)
            {
                redFurthest = floatPercentage;

                ModPacket packetRedFurthest = mod.GetPacket();
                packetRedFurthest.Write((byte)MessageType.UpdateRedFurthest);
                packetRedFurthest.Write(redFurthest);
                packetRedFurthest.Send();
            }
            ModPacket packetBlueGemX = mod.GetPacket();
            packetBlueGemX.Write((byte)MessageType.UpdateBlueGemX);
            packetBlueGemX.Write(intPercentageBlue);
            packetBlueGemX.Send();

            blueGemCarrier = $"{Main.player[BlueGem.HeldBy].name}: [c/00FFFF:{Main.player[BlueGem.HeldBy].statLife}/{Main.player[BlueGem.HeldBy].statLifeMax2}]";
            ModPacket packetBlueGemCarrier = mod.GetPacket();
            packetBlueGemCarrier.Write((byte)MessageType.UpdateBlueGemCarrier);
            packetBlueGemCarrier.Write(blueGemCarrier);
            packetBlueGemCarrier.Write(Main.player[BlueGem.HeldBy].name);
            packetBlueGemCarrier.Send();
        }
        else if (!BlueGem.IsHeld && blueWasHeld)
        {
            ModPacket packetBlueGemX = mod.GetPacket();
            packetBlueGemX.Write((byte)MessageType.UpdateBlueGemX);
            packetBlueGemX.Write(0);
            packetBlueGemX.Send();
        }

        if (RedGem.IsHeld && MatchTime % 6 == 0)
        {
            // send % of the way the gem is to enemy base as an integer
            // dist from gem is negative for red
            var distFromGem = -(Main.player[RedGem.HeldBy].position.X - RedGem.Position.X);
            intPercentageRed = (int)Math.Round(distFromGem / distBetweenGems * 100);
            intPercentageRed = Math.Clamp(intPercentageRed, 0, 100);
            float floatPercentage = (float)Math.Round(Math.Clamp(distFromGem / distBetweenGems * 100, 0f, 100f), 2);
            if (floatPercentage > blueFurthest)
            {
                blueFurthest = floatPercentage;

                ModPacket packetBlueFurthest = mod.GetPacket();
                packetBlueFurthest.Write((byte)MessageType.UpdateBlueFurthest);
                packetBlueFurthest.Write(blueFurthest);
                packetBlueFurthest.Send();
            }
            ModPacket packetRedGemX = mod.GetPacket();
            packetRedGemX.Write((byte)MessageType.UpdateRedGemX);
            packetRedGemX.Write(intPercentageRed);
            packetRedGemX.Send();

            redGemCarrier = $"{Main.player[RedGem.HeldBy].name}: [c/00FFFF:{Main.player[RedGem.HeldBy].statLife}/{Main.player[RedGem.HeldBy].statLifeMax2}]";
            ModPacket packetRedGemCarrier = mod.GetPacket();
            packetRedGemCarrier.Write((byte)MessageType.UpdateRedGemCarrier);
            packetRedGemCarrier.Write(redGemCarrier);
            packetRedGemCarrier.Write(Main.player[RedGem.HeldBy].name);
            packetRedGemCarrier.Send();
        }
        else if (!RedGem.IsHeld && redWasHeld)
        {
            ModPacket packetRedGemX = mod.GetPacket();
            packetRedGemX.Write((byte)MessageType.UpdateRedGemX);
            packetRedGemX.Write(0);
            packetRedGemX.Send();
        }

        // Blue and red gem holders
        if (!BlueGem.IsHeld && blueWasHeld)
        {
            blueGemCarrier = "At Base";
            ModPacket packetBlueGemCarrier = mod.GetPacket();
            packetBlueGemCarrier.Write((byte)MessageType.UpdateBlueGemCarrier);
            packetBlueGemCarrier.Write(blueGemCarrier);
            packetBlueGemCarrier.Write("");
            packetBlueGemCarrier.Send();
        }
        blueWasHeld = BlueGem.IsHeld;

        if (!RedGem.IsHeld && redWasHeld)
        {
            redGemCarrier = "At Base";
            ModPacket packetRedGemCarrier = mod.GetPacket();
            packetRedGemCarrier.Write((byte)MessageType.UpdateRedGemCarrier);
            packetRedGemCarrier.Write(redGemCarrier);
            packetRedGemCarrier.Write("");
            packetRedGemCarrier.Send();
        }
        redWasHeld = RedGem.IsHeld;

        //packet.Write(blueFurthest);
        //packet.Write(redFurthest);

        if (BlueGem.IsCaptured && !endGameCalled)
        {
            Console.WriteLine("Blue gem captured!");
            winner = 2;
            EndGame();
        }

        else if (RedGem.IsCaptured && !endGameCalled)
        {
            Console.WriteLine("Red gem captured!");
            winner = 1;
            EndGame();
        }

        if (!isOvertime && MatchTime >= 60 * 60 * 15 + matchStartTime)
        {

            if (BlueGem.IsHeld || RedGem.IsHeld)
            {
                isOvertime = true;
                GameInfo.overtime = true;
                
                ModPacket packetOvertime = mod.GetPacket();
                packetOvertime.Write((byte)MessageType.UpdateOvertime);
                packetOvertime.Write(true);
                packetOvertime.Send();

                overtimeTimer = OVERTIME_DURATION;

                ModPacket packetOvertimeTimer = mod.GetPacket();
                packetOvertimeTimer.Write((byte)MessageType.UpdateOvertimeTimer);
                packetOvertimeTimer.Write(OVERTIME_DURATION);
                packetOvertimeTimer.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("[OVERTIME] Overtime has started! Capture the gem or the game will end in 2 minutes."), Microsoft.Xna.Framework.Color.OrangeRed);
            }
            else if (!endGameCalled)
            {
                EndGame();
                return;
            }
        }

        if (isOvertime)
        {
            overtimeTimer--;

            if (MatchTime % 60 == 0)
            {
                ModPacket packetOvertimeTimer = mod.GetPacket();
                packetOvertimeTimer.Write((byte)MessageType.UpdateOvertimeTimer);
                packetOvertimeTimer.Write(overtimeTimer);
                packetOvertimeTimer.Send();
            }

            if (BlueGem.IsCaptured && !endGameCalled)
            {
                EndGame();
                winner = 2;
                return;
            }
            else if (RedGem.IsCaptured && !endGameCalled)
            {
                EndGame();
                winner = 1;
                return;
            }

            if (!BlueGem.IsHeld && !RedGem.IsHeld && !endGameCalled)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("[OVERTIME] Overtime ended: No gem is being held."), Microsoft.Xna.Framework.Color.Yellow);
                EndGame();
                return;
            }

            if (overtimeTimer <= 0 && !endGameCalled)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("[OVERTIME] Overtime expired! No capture."), Microsoft.Xna.Framework.Color.Red);
                EndGame();
                return;
            }
        }
        // Kill all mobs during class selection
        if (GameInfo.matchStage == 1 && killonce == true)
        {

            if (killonce)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];

                    if (!npc.active)
                        continue;

                    npc.active = false;

                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    Projectile proj = Main.projectile[i];

                    if (!proj.active)
                        continue;

                    proj.active = false;
                }

                killonce = false;
            }
        }
        if (GameInfo.matchStage == 2 && killonce == false)
        {
            killonce = true;
        }
    }

    public void queueMap(MapTypes mapType)
    { // idk what to do here i have a Queue<MapType> here
        mapQueue.Enqueue(mapType);
    }

    public bool IsPlayerSpectator(int playerIndex)
    {
        return playerSpectatorStatus.GetValueOrDefault(playerIndex, false);
    }

    public void SetPlayerSpectator(int playerIndex, bool isSpectator)
    {
        if (!Main.player[playerIndex].active) return;

        var player = Main.player[playerIndex];
        if (isSpectator)
        {
            // drop gem if they have it 
            if (BlueGem.IsHeld && BlueGem.HeldBy == playerIndex)
            {
                BlueGem.Reset();
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} dropped the Blue Gem when entering spectator mode"), Microsoft.Xna.Framework.Color.Blue);
                Console.WriteLine($"Player {player.name} dropped Blue Gem when entering spectator mode");
            }

            if (RedGem.IsHeld && RedGem.HeldBy == playerIndex)
            {
                RedGem.Reset();
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} dropped the Red Gem when entering spectator mode"), Microsoft.Xna.Framework.Color.Red);
                Console.WriteLine($"Player {player.name} dropped Red Gem when entering spectator mode");
            }


            // Set spectator status (REMOVE THIS EVENTUALLY)
            playerSpectatorStatus[playerIndex] = true;

            // Send teleport packet to client
            CTG2.WebPlayer(player.whoAmI, 120); //not sure if webbing is needed here

            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.ServerTeleport);
            packet.Write(playerIndex);
            packet.Write((int)spectatorSpawnPoint.X);
            packet.Write((int)spectatorSpawnPoint.Y);
            packet.Send(toClient: playerIndex);

            PlayerManager.GetPlayerManager(player.whoAmI).playerState = PlayerManager.PlayerState.Spectator;

            ModPacket statusPacket = mod.GetPacket();
            statusPacket.Write((byte)MessageType.ServerSpectatorUpdate);
            statusPacket.Write(playerIndex);
            statusPacket.Write(true); // is spectator
            statusPacket.Send(toClient: playerIndex);

            // DEBUG: Global broadcast
            Console.WriteLine($"Player {player.name} entered spectator mode");
        }
        else
        {
            Console.WriteLine($"GameManager: SetPlayerSpectator called with isSpectator=false for player {playerIndex}");
            if (player.team == 0)
            {
                Console.WriteLine($"Player {player.name} cannot exit spectator mode - no team assigned");
                return;
            }
            // Check if player has original team

            playerSpectatorStatus[playerIndex] = false;
            int playerTeam = player.team;

            var mod = ModContent.GetInstance<CTG2>();
            NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, playerIndex, playerTeam);


            Console.WriteLine($"GameManager: Directly calling startPlayerClassSelection for player {playerIndex} (non-game-start)");
            startPlayerClassSelection(playerIndex, false);
            // TELL SERVER TO REMOVE GHOST AND NO LONGER A SPECTATOR
            ModPacket statusPacket2 = mod.GetPacket();
            statusPacket2.Write((byte)MessageType.ServerSpectatorUpdate);
            statusPacket2.Write(playerIndex);
            statusPacket2.Write(false);
            statusPacket2.Send();
        }
    }


    public void startPlayerClassSelection(int playerIndex, bool gameStarted)
    {
        // Don't try to access PlayerManager directly on server
        // Instead, get the team from the vanilla Player.team
        Player player = Main.player[playerIndex];
        int team = player.team;

        PlayerManager.GetPlayerManager(player.whoAmI).pickedClass = false;
        var mod = ModContent.GetInstance<CTG2>();
        ModPacket classPacket = mod.GetPacket();
        classPacket.Write((byte)MessageType.UpdatePickedClass);
        classPacket.Write(playerIndex);
        classPacket.Write(false);
        classPacket.Send(toClient: playerIndex);

        Console.WriteLine($"GameManager: startPlayerClassSelection called for player {playerIndex}, team {team}, gameStarted: {gameStarted}");


        //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[DEBUG] {playerIndex} successfully entered class selection (non game)"), Color.Beige);
        // Send packet to set class selection time on client
        ModPacket timePacket = mod.GetPacket();
        timePacket.Write((byte)MessageType.SetClassSelectionTime);
        timePacket.Write(playerIndex);
        timePacket.Write(1800.0); // class selection time
        timePacket.Send(toClient: playerIndex);
        Console.WriteLine($"GameManager: Sent SetClassSelectionTime packet to player {playerIndex}");

        if (!intToTeam.ContainsKey(team))
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[ERROR] Invalid team {team} for player {playerIndex}"), Color.Red);
            return;
        }

        GameTeam gameTeam = intToTeam[team];
        PlayerManager.GetPlayerManager(player.whoAmI).playerState = PlayerManager.PlayerState.ClassSelection;
        // Send packet to update client-side player state to ClassSelection
        ModPacket statePacket = mod.GetPacket();
        statePacket.Write((byte)MessageType.UpdatePlayerState);
        statePacket.Write(playerIndex);
        statePacket.Write((byte)PlayerManager.PlayerState.ClassSelection);
        statePacket.Send(toClient: playerIndex);
        Console.WriteLine($"GameManager: Sent UpdatePlayerState packet to player {playerIndex} (ClassSelection)");
        CTG2.WebPlayer(player.whoAmI, 240);
        ModPacket packet = mod.GetPacket();
        packet.Write((byte)MessageType.ServerTeleport);
        packet.Write(playerIndex);
        packet.Write((int)gameTeam.ClassLocation.X);
        packet.Write((int)gameTeam.ClassLocation.Y);
        packet.Send(toClient: playerIndex);
        Console.WriteLine($"GameManager: Sent ServerTeleport packet to player {playerIndex} to {gameTeam.BaseLocation}");
    }

    
    public void endPlayerClassSelection(int playerIndex)
    {
        // Use vanilla Player.team instead of PlayerManager on server
        Player player = Main.player[playerIndex];
        var playerManager = player.GetModPlayer<PlayerManager>();
        int team = player.team;
        var mod = ModContent.GetInstance<CTG2>();

        if (!intToTeam.ContainsKey(team))
            return;

        if (!playerManager.pickedClass)
        {
            Random random = new Random();

            int num = random.Next(1, 18);

            ModPacket classPacket = mod.GetPacket();
            classPacket.Write((byte)MessageType.SetCurrentClass);
            classPacket.Write(player.whoAmI);
            classPacket.Write(num);
            classPacket.Send(toClient: player.whoAmI);

            ModPacket classPacket2 = mod.GetPacket();
            classPacket2.Write((byte)MessageType.UpdatePickedClass);
            classPacket2.Write(player.whoAmI);
            classPacket2.Write(true);
            classPacket2.Send(toClient: player.whoAmI);
            playerManager.pickedClass = true;
        }

        GameTeam gameTeam = intToTeam[team];

        PlayerManager.GetPlayerManager(player.whoAmI).playerState = PlayerManager.PlayerState.Active;
        ModPacket statePacket = mod.GetPacket();
        statePacket.Write((byte)MessageType.UpdatePlayerState);
        statePacket.Write(playerIndex);
        statePacket.Write((byte)PlayerManager.PlayerState.Active);
        statePacket.Send(toClient: playerIndex);

        //NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, playerIndex);

        // Force sync player stats to ensure HP displays correctly to other players
        //ForcePlayerStatSync(playerIndex);

        // Send teleport packet to client
        CTG2.WebPlayer(player.whoAmI, 240); //could be overkill
        ModPacket packet = mod.GetPacket();
        packet.Write((byte)MessageType.ServerTeleport);
        packet.Write(playerIndex);
        packet.Write((int)gameTeam.BaseLocation.X);
        packet.Write((int)gameTeam.BaseLocation.Y);
        packet.Send(toClient: playerIndex);

        ModPacket healpacket = Mod.GetPacket(); //do heal server side
        healpacket.Write((byte)MessageType.RequestFullHeal);
        healpacket.Write(player.whoAmI);
        healpacket.Send(toClient: player.whoAmI);
        player.Heal(600);
    }

    
    public override void PostUpdateWorld()
    {
        if (!Main.dedServ) return;
        if (pause)
        {
            if (pauseTimer % 6 == 0)
            {
                foreach (Player p in Main.player)
                {
                    if (p.active)
                    {
                        var mod = ModContent.GetInstance<CTG2>();

                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)MessageType.RequestAddBuff);
                        packet.Write(p.whoAmI);
                        packet.Write(BuffID.Webbed);
                        packet.Write(12);
                        packet.Send();
                    }
                }
            }

            pauseTimer++;
            return;
        }

        if (Main.GameUpdateCount % 30 == 0)
        {
            foreach (Player p in Main.player)
            {
                if (p.active) ForcePlayerStatSync(-1, p.whoAmI);
            }
        }

        if (pubsConfig)
        {
            // Handle new game timer
            if (isWaitingForNewGame)
            {
                if (newGameTimer > 0)
                {
                    if (newGameTimer == 15 * 60)
                    {
                        var mod = ModContent.GetInstance<CTG2>();

                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)MessageType.SyncGameInformation);
                        packet.Write(3);
                        packet.Write(newGameTimer);
                        packet.Write(false);
                        packet.Write(0);
                        packet.Write(0);
                        packet.Write(0);
                        packet.Write("Waiting for new game...");
                        packet.Write("Waiting for new game...");
                        packet.Write(mapName);
                        packet.Write(blueTeamSize);
                        packet.Write(redTeamSize);
                        packet.Write(matchStartTime);
                        packet.Write(blueAttempts);
                        packet.Write(redAttempts);
                        packet.Write(blueFurthest);
                        packet.Write(redFurthest);
                        packet.Send();
                    }

                    newGameTimer--;
                
                    // Send game info updates during waiting phase every 6 ticks
                    if (newGameTimer % 6 == 0)
                    {
                        var mod = ModContent.GetInstance<CTG2>();
                        ModPacket packet = mod.GetPacket();
                        packet.Write((byte)MessageType.ServerGameUpdate);
                        packet.Write(newGameTimer); // Send remaining time instead of match time
                        packet.Send();
                    }
                    
                    // Countdown announcements
                    if (newGameTimer == 10 * 60) // 10 seconds left
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] New game starting in 10 seconds..."), Microsoft.Xna.Framework.Color.Orange);
                    }
                    else if (newGameTimer == 5 * 60) // 5 seconds left
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] New game starting in 5 seconds..."), Microsoft.Xna.Framework.Color.Red);
                    }
                    else if (newGameTimer == 3 * 60) // 3 seconds left
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] 3..."), Microsoft.Xna.Framework.Color.Red);
                    }
                    else if (newGameTimer == 2 * 60) // 2 seconds left
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] 2..."), Microsoft.Xna.Framework.Color.Red);
                    }
                    else if (newGameTimer == 1 * 60) // 1 second left
                    {
                        ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] 1..."), Microsoft.Xna.Framework.Color.Red);
                    }
                    
                    return; // Don't run game logic while waiting for new game
                }
                else
                {
                    // Time to start new game
                    isWaitingForNewGame = false;
                    newGameTimer = 0;
                    
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] Starting new game now!"), Microsoft.Xna.Framework.Color.LimeGreen);
                    Console.WriteLine("GameManager: Starting new game after 15-second delay");
                    
                    StartGame();
                    return;
                }
            }
        }

        if (!IsGameActive)
        {
            if (resetEndGame)
            {
                // Send game info updates when game is not active every 6 ticks
                var mod = ModContent.GetInstance<CTG2>();

                ModPacket packet = mod.GetPacket();
                packet.Write((byte)MessageType.SyncGameInformation);
                packet.Write(3);
                packet.Write(MatchTime);
                packet.Write(false);
                packet.Write(0);
                packet.Write(0);
                packet.Write(0);
                packet.Write("Waiting for new game...");
                packet.Write("Waiting for new game...");
                packet.Write(mapName);
                packet.Write(blueTeamSize);
                packet.Write(redTeamSize);
                packet.Write(matchStartTime);
                packet.Write(blueAttempts);
                packet.Write(redAttempts);
                packet.Write(blueFurthest);
                packet.Write(redFurthest);
                packet.Send();

                resetEndGame = false;
            }

            return;
        }

        UpdateGame();

        if (Main.netMode == NetmodeID.Server)
        {
            int newMusicChoice = MusicSelectionLogic();
            if (newMusicChoice != _currentMusicChoice)
            {
            _currentMusicChoice = newMusicChoice;
            var packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncBiomeMusic);
            packet.Write(_currentMusicChoice);
            packet.Send(); // Sends to all clients
            }
        }

        base.PostUpdateWorld();
    }

    public int MusicSelectionLogic()
    {
        
        return MusicLoader.GetMusicSlot(Mod, "Assets/Music/clashroyaleOT");
    }

    public void ClearPlayerInventory(int playerIdx)
    {
        Player player = Main.player[playerIdx];
        // Clear main inventory
        for (int i = 0; i < player.inventory.Length; i++)
        {
            player.inventory[i].TurnToAir();
        }

        // Clear armor and accessories
        for (int i = 0; i < player.armor.Length; i++)
        {
            player.armor[i].TurnToAir();
        }

        // Clear dye slots
        for (int i = 0; i < player.dye.Length; i++)
        {
            player.dye[i].TurnToAir();
        }

        // Clear miscellaneous equipment
        for (int i = 0; i < player.miscEquips.Length; i++)
        {
            player.miscEquips[i].TurnToAir();
        }

        // Clear misc dyes
        for (int i = 0; i < player.miscDyes.Length; i++)
        {
            player.miscDyes[i].TurnToAir();
        }

        // Sync inventory changes to all clients
        //NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);

        Console.WriteLine($"GameManager: Cleared inventory for player {player.whoAmI} ({player.name})");
    }

    public void HandlePlayerTeamChange(int playerIndex, int newTeam)
    {
        if (!Main.player[playerIndex].active) return;

        Player player = Main.player[playerIndex];
        int oldTeam = player.team;

        Console.WriteLine($"GameManager: HandlePlayerTeamChange called for player {playerIndex} from team {oldTeam} to team {newTeam}");

        // Set the new team
        player.team = newTeam;

        // Update PlayerManager team
        var playerManager = player.GetModPlayer<PlayerManager>();
        playerManager.SetTeam(newTeam);

        // Send team update to all clients
        NetMessage.SendData(MessageID.PlayerTeam, -1, -1, null, playerIndex, newTeam);

        var mod = ModContent.GetInstance<CTG2>();

        // Send packet to update PlayerManager.team on client side
        ModPacket updatePacket = mod.GetPacket();
        updatePacket.Write((byte)MessageType.UpdatePlayerTeam);
        updatePacket.Write(playerIndex);
        updatePacket.Write(newTeam);
        updatePacket.Send(toClient: playerIndex);

        // Handle gem dropping if player was carrying one
        if (BlueGem.IsHeld && BlueGem.HeldBy == playerIndex)
        {
            BlueGem.Reset();
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} dropped the Blue Gem when changing teams"), Microsoft.Xna.Framework.Color.Blue);
            Console.WriteLine($"Player {player.name} dropped Blue Gem when changing teams");
        }

        if (RedGem.IsHeld && RedGem.HeldBy == playerIndex)
        {
            RedGem.Reset();
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} dropped the Red Gem when changing teams"), Microsoft.Xna.Framework.Color.Red);
            Console.WriteLine($"Player {player.name} dropped Red Gem when changing teams");
        }

        // Update team rosters
        BlueTeam.UpdateTeam();
        RedTeam.UpdateTeam();

        // Handle different scenarios based on new team and game state
        if (newTeam == 0)
        {
            // Player set to no team - make them spectator
            SetPlayerSpectator(playerIndex, true);
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} has been moved to spectator (no team)"), Microsoft.Xna.Framework.Color.Olive);
            Console.WriteLine($"Player {player.name} moved to spectator due to team change to 0");
        }
        else if (IsGameActive)
        {
            // Game is active - handle based on game phase



            // Remove from spectator if they were spectating
            if (playerSpectatorStatus.GetValueOrDefault(playerIndex, false))
            {
                playerSpectatorStatus[playerIndex] = false;
                ModPacket spectatorPacket = mod.GetPacket();
                spectatorPacket.Write((byte)MessageType.ServerSpectatorUpdate);
                spectatorPacket.Write(playerIndex);
                spectatorPacket.Write(false);
                spectatorPacket.Send(toClient: playerIndex);
            }

            // Start class selection for the player
            startPlayerClassSelection(playerIndex, MatchTime < matchStartTime); // true if during initial game start phase

            // Force sync stats after team change
            //ForcePlayerStatSync(playerIndex);

            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} has been moved to team {newTeam} and entered class selection"), Microsoft.Xna.Framework.Color.Green);
            Console.WriteLine($"Player {player.name} moved to team {newTeam} and started class selection");
        }
        else
        {
            // No game active - just update team assignment
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} has been assigned to team {newTeam}"), Microsoft.Xna.Framework.Color.Yellow);
            Console.WriteLine($"Player {player.name} assigned to team {newTeam} (no active game)");
        }
    }

    private void EnsureAllPlayersHavePvP()
    {
        foreach (Player player in Main.player)
        {
            if (player.active && player.team != 0 && !player.hostile)
            {
                player.hostile = true;
                NetMessage.SendData(MessageID.TogglePVP, -1, -1, null, player.whoAmI);
                Console.WriteLine($"GameManager: Forced PvP on for player {player.name}");
            }
        }
    }

    public void PubsConfig()
    {
        
        var manager = ModContent.GetInstance<GameManager>();
        List<Player> activePlayers = new();
        foreach (Player player in Main.player)
        {
            if (player != null && player.active && !player.dead)
            {
                activePlayers.Add(player);
            }
        }

        Random rng = new Random();
        int n = activePlayers.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (activePlayers[n], activePlayers[k]) = (activePlayers[k], activePlayers[n]);
        }

        int half = activePlayers.Count / 2;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            int teamID = (i < half) ? 1 : 3; 
            int whoAmI = activePlayers[i].whoAmI;

            manager.HandlePlayerTeamChange(whoAmI, teamID);
        }
    }
    
    public void ScrimsConfig()
    {

    }

    public void RngConfig()
    {

    }

    public static void ForcePlayerStatSync(int toWho, int fromWho)
    {
        if (fromWho < 0 || fromWho >= Main.player.Length) return;

        Player player = Main.player[fromWho];
        if (!player.active) return;

        var classSystem = player.GetModPlayer<ClassSystem>();
        classSystem.SyncPlayerStats(toWho, fromWho);

        //Console.WriteLine($"GameManager: Force synced stats for player {player.name}");
    }

    public static void ClearFloatingItems()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        for (int i = 0; i < Main.maxItems; i++)
        {
            if (Main.item[i].active)
            {
                Main.item[i].TurnToAir();
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i);
            }
        }
    }

    public void SyncGameInfo(int playerIndex)
    {
        var mod = ModContent.GetInstance<CTG2>();

        ModPacket packet = mod.GetPacket();
        packet.Write((byte)MessageType.SyncGameInformation);
        packet.Write(matchStage);
        packet.Write(MatchTime);
        packet.Write(isOvertime);
        packet.Write(overtimeTimer);
        packet.Write(intPercentageBlue);
        packet.Write(intPercentageRed);
        packet.Write(blueGemCarrier);
        packet.Write(redGemCarrier);
        packet.Write(mapName);
        packet.Write(blueTeamSize);
        packet.Write(redTeamSize);
        packet.Write(matchStartTime);
        packet.Write(blueAttempts);
        packet.Write(redAttempts);
        packet.Write(blueFurthest);
        packet.Write(redFurthest);
        packet.Send(toClient: playerIndex);
    }
}
