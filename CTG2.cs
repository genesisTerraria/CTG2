using System;
using Terraria.ModLoader;
using System.IO;
using System.Text.Json;
using ClassesNamespace;
using Terraria;
using CTG2.Content;
using CTG2.Content.Classes;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;
using CTG2.Content.Commands;
using CTG2.Content.Items;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using MonoMod.Cil;
using Humanizer;
using System.Reflection;
using DirectDashMod.Players;


using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.CameraModifiers;




namespace CTG2
{
    public enum MessageType : byte
    {
        RequestStartGame = 0,  // client → server
        RequestEndGame = 1,  // client → server
        ServerGameStart = 2,  // server → client
        ServerGameEnd = 3,  // server → client
        RequestTogglePause = 4,  // client -> server
        ServerGameUpdate = 5,  // server → client
        ServerTeleport = 6, // server -> client
        ServerSetSpawn = 7, // server -> client
        RequestSpawnNpc = 8,
        RequestSpawnProjectile = 9,
        RequestAddBuff = 10,
        RequestNextMap = 11,
        ServerChangeMap = 12,
        RequestTeleport = 13,
        RequestMaxHealth = 14,
        SyncNpcIndex = 15,
        SetNpcTeam = 16,
        RequestTeamChange = 17,
        RequestEnterSpectator = 18, // client → server
        RequestExitSpectator = 19,  // client → server  
        ServerSpectatorUpdate = 20,  // server → client
        EnterClassSelection = 21,
        ExitClassSelection = 22,
        UpdatePlayerTeam = 23,  // server → client
        SetClassSelectionTime = 24,  // server → client
        UpdatePlayerState = 25,  // server → client
        RequestViewMap = 26,
        RequestTeamChat = 27,
        RequestDie = 28,
        RequestKill = 29,
        RequestWeb = 30,
        RequestSyncStats = 31,
        RequestFullHeal = 32,
        //RequestUnpause = 33,
        RequestMute = 34,
        Mute = 35,
        RequestUnmute = 36,
        Unmute = 37,
        LateJoin = 38,
        RequestGamemodeChange = 39,
        UpdateGamemode = 40,
        RequestClassSelection = 41,
        RequestAudio = 42,
        UpdatePickedClass = 43,
        RequestBanPlayer = 44,
        DASH = 45,
        FORCE_JUMP = 46,
        GRAB_KEYS = 47,
        RequestChat = 48,
        RequestMatchTime = 49,
        UpdateMusic = 50,
        ChangeMusic = 51,
        SetCurrentClass = 52,
        RequestAudioToClient = 53,
        RequestAudioToClientPacket = 54,
        SyncBiomeMusic = 55,
        UpdateMatchStage = 56,
        UpdateBlueGemX = 57,
        UpdateRedGemX = 58,
        UpdateBlueGemCarrier = 59,
        UpdateRedGemCarrier = 60,
        UpdateOvertime = 61,
        UpdateOvertimeTimer = 62,
        UpdateMapName = 63,
        UpdateBlueTeamSize = 64,
        UpdateRedTeamSize = 65,
        UpdateMatchStartTime = 66,
        UpdateBlueAttempts = 67,
        UpdateRedAttempts = 68,
        UpdateBlueFurthest = 69,
        UpdateRedFurthest = 70,
        RequestSyncGameInfo = 71,
        SyncGameInformation = 72,
        SyncPlayerArmor = 73,
        RequestChatColored = 74,
        UpdateIsHeld = 75,
        UpdateHeldBy = 76,
        UpdateIsCaptured = 77,
        RequestAudioClientSide = 78,
        AudioClientSide = 79,
        UpdatePlayerKDR = 80,
        RequestDictAddAndSync = 81,
        SyncDict = 82,
        RequestMakeMeAdmin = 83,
        SendNewChatFlair = 84,
        RegisterUuid = 85,
        SyncUuidForPlayer = 86,
        ClearInventory = 87,
        SyncAbilityAttributes = 88,
        SyncClassSystemAttributes = 89,
        ArcherDash = 90,
        SyncAddBuff = 91,
        RequestHeal = 92,
        SyncHeal = 93
    }

    public class CTG2 : Mod
    {
        public static int requestedNpcIndex = 0;
        public static Random randomGenerator = new Random();
        public static ClientConfig config = new ClientConfig();
        public Dictionary<string, PlayerData> PlayerDataDictionary= new Dictionary<string, PlayerData>();
        public static readonly Dictionary<int, string> UuidByWhoAmI = new(); //for mapping

        public static ModKeybind ArcherDashKeybind;
        public static ModKeybind AdvancedBinocularsKeybind;
        public static ModKeybind Ability1Keybind;
        //public static int BiomeMusicId = 0; // client side 

        // static methods
        private static string GetTeamName(int teamId)
        {
            return teamId switch
            {
                1 => "RED",
                2 => "GREEN",
                3 => "BLUE",
                4 => "YELLOW",
                5 => "PINK",
                _ => $"TEAM {teamId}"
            };
        }


        private static Color GetTeamColor(int teamId)
        {
            return teamId switch
            {
                1 => Color.Red,
                2 => Color.Green,
                3 => Color.Blue,
                4 => Color.Yellow,
                5 => Color.Pink,
                _ => Color.White
            };
        }
        // overrides

        public override void Load()
        {

            //MusicLoader.AddMusic(this, "Assets/Music/clashroyaleOT");
            base.Load();
            // load client config in
            using (var stream = GetFileStream($"Content/Classes/clientconfig.json"))
            using (var fileReader = new StreamReader(stream))
            {
                var jsonData = fileReader.ReadToEnd();
                try
                {
                    config = JsonSerializer.Deserialize<ClientConfig>(jsonData);
                }
                catch
                {
                    //Main.NewText("Failed to load or parse client config file.", Microsoft.Xna.Framework.Color.Red);
                    return;
                }
            }

            ArcherDashKeybind = KeybindLoader.RegisterKeybind(this, "ArcherDash", "LeftShift");
            AdvancedBinocularsKeybind = KeybindLoader.RegisterKeybind(this, "AdvancedBinoculars", "MouseRight");
            Ability1Keybind = KeybindLoader.RegisterKeybind(this, "Ability 1", "R");
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            // GameManager
            var manager = ModContent.GetInstance<GameManager>();

            byte msgType = reader.ReadByte();
            switch (msgType)
            {
                // Client -> Server Packets (these cases will run on the Server)
                case (byte)MessageType.RequestStartGame:
                    manager.StartGame();
                    Console.WriteLine("Server Received Game Start Request!");
                    break;

                case (byte)MessageType.RequestEndGame:
                    manager.EndGame();
                    Console.WriteLine("Server Received Game End Request!");
                    break;
                case (byte)MessageType.RequestTogglePause:
                    ChatHelper.BroadcastChatMessage(!manager.pause ? NetworkText.FromLiteral($"Game paused.") : NetworkText.FromLiteral($"Game unpaused."), Color.Yellow);

                    if (!manager.pause)
                        manager.PauseGame();
                    else
                        manager.UnpauseGame();

                    break;
                case (byte)MessageType.RequestSpawnNpc:
                    var npcX = reader.ReadInt32();
                    var npcY = reader.ReadInt32();
                    var npcType = reader.ReadInt32();
                    // use ai0 to store spawning player's team.
                    float ai0 = (float)reader.ReadInt32();
                    float ai1 = reader.ReadSingle();
                    int npcIndex = NPC.NewNPC(Main.LocalPlayer.GetSource_Misc("SpawnNPC"), npcX, npcY, npcType, 0, ai0, ai1);

                    break;
                case (byte)MessageType.SyncNpcIndex:
                    int syncedNpcIndex = reader.ReadInt32();
                    CTG2.requestedNpcIndex = syncedNpcIndex;
                    break;
                case (byte)MessageType.SetNpcTeam:
                    int NpcIndex = reader.ReadInt32();
                    int npcTeam = reader.ReadInt32();
                    Main.npc[NpcIndex].GetGlobalNPC<AllNpcs>().team = npcTeam;
                    break;
                case (byte)MessageType.RequestSpawnProjectile:
                    var spawnPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    var velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    var projType = reader.ReadInt32();
                    var damage = reader.ReadInt32();
                    var knockback = reader.ReadSingle();

                    int projectileIndex = Projectile.NewProjectile(Main.LocalPlayer.GetSource_Misc("Class15Ability"), spawnPos, velocity, projType, damage, knockback);
                    break;
                    
                case (byte)MessageType.RequestAddBuff:
                {
                    // CLIENT -> SERVER ONLY
                    if (Main.netMode != NetmodeID.Server)
                        break;
                
                    int playerID = reader.ReadInt32();
                    int buffType = reader.ReadInt32();
                    int time = reader.ReadInt32();
                
                    Player player = Main.player[playerID];
                    if (!player.active)
                        break;
                
                    // Server applies the buff
                    player.AddBuff(buffType, time);
                
                    // Sync to all clients (including sender)
                    ModPacket packet = GetPacket();
                    packet.Write((byte)MessageType.SyncAddBuff);
                    packet.Write(playerID);
                    packet.Write(buffType);
                    packet.Write(time);
                    packet.Send();
                
                    break;
                }

                case (byte)MessageType.SyncAddBuff:
                {
                    // SERVER -> CLIENTS ONLY
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        break;
                
                    int playerID = reader.ReadInt32();
                    int buffType = reader.ReadInt32();
                    int time = reader.ReadInt32();
                
                    Player player = Main.player[playerID];
                    if (!player.active)
                        break;
                
                    player.AddBuff(buffType, time);
                    break;
                }
                
                case (byte)MessageType.SetCurrentClass:
                    int play = reader.ReadInt32();
                    int classNum = reader.ReadInt32();
                    string className = "";
                    Player ppp = Main.player[play];
                    var playerMnger = ppp.GetModPlayer<PlayerManager>();

                    var gmServer = ModContent.GetInstance<GameManager>();
                    if (classNum == 18 && !gmServer.rngConfig)
                    {
                        break;
                    }
                    if (classNum != 18 && gmServer.rngConfig)
                    {
                        break;
                    }

                    switch (classNum)
                    {
                        case 1:
                            className = "Archer";
                            break;
                        case 2:
                            className = "Ninja";
                            break;
                        case 3:
                            className = "Beast";
                            break;
                        case 4:
                            className = "Gladiator";
                            break;
                        case 5:
                            className = "Paladin";
                            break;
                        case 6:
                            className = "Tank";
                            break;
                        case 7:
                            className = "Black Mage";
                            break;
                        case 8:
                            className = "Psychic";
                            break;
                        case 9:
                            className = "White Mage";
                            break;
                        case 10:
                            className = "Miner";
                            break;
                        case 11:
                            className = "Fish";
                            break;
                        case 12:
                            className = "Clown";
                            break;
                        case 13:
                            className = "Flame Bunny";
                            break;
                        case 14:
                            className = "Tiki Priest";
                            break;
                        case 15:
                            className = "Tree";
                            break;
                        case 16:
                            className = "Mutant";
                            break;
                        case 17:
                            className = "Leech";
                            break;
                        case 18:
                            className = "Rng Man";
                            break;
                    }

                    foreach (var classes in CTG2.config.Classes)
                    {
                        if (classes.Name == className)
                        {
                            var cls = classes;

                            playerMnger.currentClass = cls;

                            var classPlayer = ppp.GetModPlayer<ClassSystem>();
                            classPlayer.setClass();

                            Console.WriteLine("Succesfully picked random class");
                            break;
                        }
                    }

                    if (!playerMnger.pickedClass)
                        Console.WriteLine("Failed to pick random class");

                    break;
                case (byte)MessageType.RequestNextMap:
                    string receivedMapName = reader.ReadString();


                    if (Enum.TryParse<MapTypes>(receivedMapName, true, out MapTypes mapType))
                    {

                        manager.queueMap(mapType);
                    }
                    else
                    {
                        // log?
                    }
                    break;
                case (byte)MessageType.RequestChat:
                    string message = reader.ReadString();
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Microsoft.Xna.Framework.Color.Olive);
                    break;
                case (byte)MessageType.RequestChatColored:
                    string msg = reader.ReadString();
                    uint clrVal = reader.ReadUInt32();
                    Color clr = new Color();
                    clr.PackedValue = clrVal;
                    ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(msg), clr);
                    break;
                case (byte)MessageType.UpdateIsHeld:
                    int gemType1 = reader.ReadInt32();
                    bool isHeld = reader.ReadBoolean();

                    // 1. Update the local state (this happens on Server or the receiving Clients)
                    var manager1 = ModContent.GetInstance<GameManager>();
                    if (gemType1 == 1)
                        manager1.RedGem.IsHeld = isHeld;
                    else
                        manager1.BlueGem.IsHeld = isHeld;

                    // 2. If the Server received this from a client, it must tell all other clients
                    if (Main.netMode == NetmodeID.Server) {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)MessageType.UpdateIsHeld);
                        packet.Write(gemType1);
                        packet.Write(isHeld);
                        // ignoreClient: whoAmI ensures we don't send the data back to the person who just sent it to us
                        packet.Send(-1, whoAmI); 
                    }
                    break;
                case (byte)MessageType.UpdateHeldBy:
                    int gemType2 = reader.ReadInt32();
                    int heldBy = reader.ReadInt32();

                    // 1. Update the local state (this happens on Server or the receiving Clients)
                    var manager2 = ModContent.GetInstance<GameManager>();
                    if (gemType2 == 1)
                        manager2.RedGem.HeldBy = heldBy;
                    else
                        manager2.BlueGem.HeldBy = heldBy;

                    // 2. If the Server received this from a client, it must tell all other clients
                    if (Main.netMode == NetmodeID.Server) {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)MessageType.UpdateHeldBy);
                        packet.Write(gemType2);
                        packet.Write(heldBy);
                        // ignoreClient: whoAmI ensures we don't send the data back to the person who just sent it to us
                        packet.Send(-1, whoAmI); 
                    }
                    break;
                case (byte)MessageType.UpdateIsCaptured:
                    int gemType3 = reader.ReadInt32();
                    bool isCaptured = reader.ReadBoolean();

                    // 1. Update the local state (this happens on Server or the receiving Clients)
                    var manager3 = ModContent.GetInstance<GameManager>();
                    if (gemType3 == 1)
                        manager3.RedGem.IsCaptured = isCaptured;
                    else
                        manager3.BlueGem.IsCaptured = isCaptured;

                    // 2. If the Server received this from a client, it must tell all other clients
                    if (Main.netMode == NetmodeID.Server) {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)MessageType.UpdateIsCaptured);
                        packet.Write(gemType3);
                        packet.Write(isCaptured);
                        // ignoreClient: whoAmI ensures we don't send the data back to the person who just sent it to us
                        packet.Send(-1, whoAmI); 
                    }
                    break;

                case (byte)MessageType.ClearInventory:
                    int playerIdxx = reader.ReadInt32();
                    var manager4 = ModContent.GetInstance<GameManager>();

                    manager4.ClearPlayerInventory(playerIdxx);

                     if (Main.netMode == NetmodeID.Server) {
                        ModPacket packet = GetPacket();
                        packet.Write((byte)MessageType.ClearInventory);
                        packet.Write(playerIdxx);
                        // ignoreClient: whoAmI ensures we don't send the data back to the person who just sent it to us
                        packet.Send(-1, whoAmI); 
                    }
                    break;

                // mainly used for clown ability
                case (byte)MessageType.RequestTeleport:
                    // teleport on server
                    var id2 = reader.ReadInt32();
                    int tpX2 = reader.ReadInt32();
                    int tpY2 = reader.ReadInt32();
                    var ply = Main.player[id2];
                    ply.Teleport(new Vector2(tpX2, tpY2));
                    Console.WriteLine("Server Received Teleport!");
                    // then tell all clients they were teleported
                    var mod = ModContent.GetInstance<CTG2>();
                    ModPacket packet2 = mod.GetPacket();
                    packet2.Write((byte)MessageType.ServerTeleport);
                    packet2.Write(ply.whoAmI);
                    packet2.Write(tpX2);
                    packet2.Write(tpY2);
                    packet2.Send();
                    break;

                case (byte)MessageType.RequestMaxHealth:
                    // teleport on server
                    int sender = reader.ReadInt32();
                    int newMax = reader.ReadInt32();
                    var playerToEdit = Main.player[sender];
                    playerToEdit.statLifeMax = newMax;
                    playerToEdit.statLifeMax2 = newMax;
                    break;
                case (byte)MessageType.UpdatePickedClass:
                    int plyrIndx = reader.ReadInt32();
                    bool pickedClass = reader.ReadBoolean();
                    var playerManage = Main.player[plyrIndx].GetModPlayer<PlayerManager>();
                    playerManage.pickedClass = pickedClass;
                    break;
                case (byte)MessageType.RequestTeamChange:
                    int target = reader.ReadInt32();
                    int requestedTeam = reader.ReadInt32();

                    if (requestedTeam == -1) //If call is for pubsconfig (gives random team)
                    {
                        int red = 0, blue = 0;
                        foreach (Player p in Main.player)
                        {
                            if (p.active && !p.dead && p.whoAmI != target)
                            {
                                if (p.team == 1) red++;
                                else if (p.team == 3) blue++;
                            }
                        }
                        requestedTeam = red < blue ? 1 : (blue < red ? 3 : (Main.rand.NextBool() ? 1 : 3));
                    }

                    ModContent.GetInstance<GameManager>().HandlePlayerTeamChange(target, requestedTeam);
                    break;

                case (byte)MessageType.RequestEnterSpectator:
                    int spectatorPlayerIndex = reader.ReadInt32();
                    manager.SetPlayerSpectator(spectatorPlayerIndex, true);
                    Console.WriteLine($"Server Received Enter Spectator Request from player {spectatorPlayerIndex}!");
                    break;

                case (byte)MessageType.RequestExitSpectator:
                    int exitSpectatorPlayerIndex = reader.ReadInt32();
                    manager.SetPlayerSpectator(exitSpectatorPlayerIndex, false);
                    Console.WriteLine($"Server Received Exit Spectator Request from player {exitSpectatorPlayerIndex}!");
                    break;

                case (byte)MessageType.ExitClassSelection:
                    int exitClassPlayerIndex = reader.ReadInt32();
                    manager.endPlayerClassSelection(exitClassPlayerIndex);
                    Console.WriteLine($"Server Received Exit Class Selection from player {exitClassPlayerIndex}!");
                    break;

                case (byte)MessageType.EnterClassSelection:
                    int enterClassIndex = reader.ReadInt32();
                    bool enterGameStart = reader.ReadBoolean();
                    manager.startPlayerClassSelection(enterClassIndex, enterGameStart);
                    Console.WriteLine($"Server Received Enter Class Selection from player {enterClassIndex}, gameStart: {enterGameStart}!");
                    break;

                // Server->Client Packets (these cases will run on the Client)
                case (byte)MessageType.ServerGameStart:
                    GameInfo.matchStage = 1;
                    Console.WriteLine("Client Received Game Start!");
                    break;
                case (byte)MessageType.ServerGameEnd:
                    GameInfo.matchStage = 0;
                    Console.WriteLine("Client Received Game End!");
                    break;
                case (byte)MessageType.ServerTeleport:
                    var id = reader.ReadInt32();
                    int tpX = reader.ReadInt32();
                    int tpY = reader.ReadInt32();
                    //Main.NewText($"CLIENT: Received ServerTeleport packet for player {id} to ({tpX}, {tpY}), myPlayer: {Main.myPlayer}", Color.Yellow);
                    if (id == Main.myPlayer)
                    {
                        Vector2 oldPos = Main.player[id].position;
                        Main.player[id].Teleport(new Vector2(tpX, tpY));
                        Vector2 newPos = Main.player[id].position;
                        //Main.NewText($"CLIENT: Teleported from {oldPos} to {newPos}!", Color.Green);
                    }
                    else
                    {
                        //Main.NewText($"CLIENT: Ignoring teleport packet - not for local player", Color.Orange);
                    }
                    break;
                case (byte)MessageType.ServerSetSpawn:
                    var localPlayer = Main.player[Main.myPlayer];
                    int spawnX = reader.ReadInt32();
                    int spawnY = reader.ReadInt32();
                    localPlayer.ChangeSpawn(spawnX, spawnY);
                    Console.WriteLine("Client Received Spawn Update!");
                    break;

                // Gems Status Updates
                case (byte)MessageType.ServerGameUpdate:
                    // Populate GameInfo fields
                    GameInfo.matchTime = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateMatchStage:
                    GameInfo.matchStage = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateBlueGemX:
                    GameInfo.blueGemX = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateRedGemX:
                    GameInfo.redGemX = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateBlueGemCarrier:
                    GameInfo.blueGemCarrier = reader.ReadString();
                    GameInfo.blueGemCarrierName = reader.ReadString();
                    break;
                case (byte)MessageType.UpdateRedGemCarrier:
                    GameInfo.redGemCarrier = reader.ReadString();
                    GameInfo.redGemCarrierName = reader.ReadString();
                    break;
                case (byte)MessageType.UpdateOvertime:
                    GameInfo.overtime = reader.ReadBoolean();
                    break;
                case (byte)MessageType.UpdateOvertimeTimer:
                    GameInfo.overtimeTimer = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateMapName:
                    GameInfo.mapName = reader.ReadString();
                    break;
                case (byte)MessageType.UpdateBlueTeamSize:
                    GameInfo.blueTeamSize = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateRedTeamSize:
                    GameInfo.redTeamSize = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateMatchStartTime:
                    GameInfo.matchStartTime = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateBlueAttempts:
                    GameInfo.blueAttempts = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateRedAttempts:
                    GameInfo.redAttempts = reader.ReadInt32();
                    break;
                case (byte)MessageType.UpdateBlueFurthest:
                    GameInfo.blueFurthest = reader.ReadSingle();
                    break;
                case (byte)MessageType.UpdateRedFurthest:
                    GameInfo.redFurthest = reader.ReadSingle();
                    break;
                case (byte)MessageType.RequestMatchTime:
                    GameInfo.matchTime = reader.ReadInt32();
                    break;
                case (byte)MessageType.RequestSyncGameInfo:
                    int playerCalling = reader.ReadInt32();
                    ModContent.GetInstance<GameManager>().SyncGameInfo(playerCalling);
                    break;
                case (byte)MessageType.SyncGameInformation:
                    GameInfo.matchStage = reader.ReadInt32();
                    GameInfo.matchTime = reader.ReadInt32();
                    GameInfo.overtime = reader.ReadBoolean();
                    GameInfo.overtimeTimer = reader.ReadInt32();
                    GameInfo.blueGemX = reader.ReadInt32();
                    GameInfo.redGemX = reader.ReadInt32();
                    GameInfo.blueGemCarrier = reader.ReadString();
                    GameInfo.redGemCarrier = reader.ReadString();
                    GameInfo.mapName = reader.ReadString();
                    GameInfo.blueTeamSize = reader.ReadInt32();
                    GameInfo.redTeamSize = reader.ReadInt32();
                    GameInfo.matchStartTime = reader.ReadInt32();
                    GameInfo.blueAttempts = reader.ReadInt32();
                    GameInfo.redAttempts = reader.ReadInt32();
                    GameInfo.blueFurthest = reader.ReadSingle();
                    GameInfo.redFurthest = reader.ReadSingle();
                    GameInfo.blueGemCarrierName = reader.ReadString();
                    GameInfo.redGemCarrierName = reader.ReadString();

                    break;
                case (byte)MessageType.SyncAbilityAttributes:
                    int playerindd = reader.ReadInt32();
                    int cooldown = reader.ReadInt32();
                    int class4BuffTimer = reader.ReadInt32();
                    bool class4PendingBuffs = reader.ReadBoolean();
                    int class6ReleaseTimer = reader.ReadInt32();
                    int class7HitCounter = reader.ReadInt32();
                    int class8HP = reader.ReadInt32();
                    bool psychicActive = reader.ReadBoolean();
                    int class12SwapTimer = reader.ReadInt32();
                    int class12ClosestDist = reader.ReadInt32();
                    int class15AbilityTimer = reader.ReadInt32();
                    int mutantState = reader.ReadInt32();

                    if (playerindd == Main.myPlayer)
                    {
                        var abils = Main.player[playerindd].GetModPlayer<Abilities>();

                        abils.cooldown = cooldown;
                        abils.class4BuffTimer = class4BuffTimer;
                        abils.class4PendingBuffs = class4PendingBuffs;
                        abils.class7HitCounter = class7HitCounter;
                        abils.class8HP = class8HP;
                        abils.psychicActive = psychicActive;
                        abils.class12SwapTimer = class12SwapTimer;
                        abils.class12ClosestDist = class12ClosestDist;
                        abils.class15AbilityTimer = class15AbilityTimer;
                        abils.mutantState = mutantState;
                    }
                    break;
                case (byte)MessageType.SyncClassSystemAttributes:
                    int playerIndd = reader.ReadInt32();
                    int blockCounter = reader.ReadInt32();
                    int bombCounter = reader.ReadInt32();

                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket classSystemPacket = ModContent.GetInstance<CTG2>().GetPacket();
                        classSystemPacket.Write((byte)MessageType.SyncClassSystemAttributes);
                        classSystemPacket.Write(playerIndd);
                        classSystemPacket.Write(blockCounter);
                        classSystemPacket.Write(bombCounter);
                        classSystemPacket.Send(toClient: playerIndd);
                    }

                    var sys = Main.player[playerIndd].GetModPlayer<ClassSystem>();

                    sys.blockCounter = blockCounter;
                    sys.bombCounter = bombCounter;

                    break;
            
                case (byte)MessageType.ServerSpectatorUpdate:
                    int playerIndex = reader.ReadInt32();
                    bool isSpectator = reader.ReadBoolean();
                    if (playerIndex == Main.myPlayer)
                    {
                        Main.player[playerIndex].ghost = isSpectator;
                        if (isSpectator)
                        {
                            Main.player[playerIndex].respawnTimer = 9999;
                            Main.player[playerIndex].team = 0;
                        }
                        else
                        {
                            Main.player[playerIndex].respawnTimer = 0;
                        }
                    }
                    Console.WriteLine($"Client Received Spectator Update: Player {playerIndex} is now {(isSpectator ? "spectator" : "active")}!");
                    break;

                case (byte)MessageType.UpdatePlayerTeam:
                    int playerIdx = reader.ReadInt32();
                    int newTeamID = reader.ReadInt32();
                    if (playerIdx == Main.myPlayer)
                    {
                        var playerManager = Main.player[playerIdx].GetModPlayer<PlayerManager>();
                        playerManager.team = newTeamID;
                        Console.WriteLine($"Client: Updated PlayerManager team to {newTeamID}");
                    }
                    break;

                case (byte)MessageType.SetClassSelectionTime:
                    int targetPlayerIdx = reader.ReadInt32();
                    double classTime = reader.ReadDouble();
                    if (targetPlayerIdx == Main.myPlayer)
                    {
                        PlayerManager.setPlayerClassSelectionTime(targetPlayerIdx, classTime);
                        Console.WriteLine($"Client: Set class selection time to {classTime}");
                    }
                    break;

                case (byte)MessageType.UpdatePlayerState:
                    int statePlayerIdx = reader.ReadInt32();
                    byte newState = reader.ReadByte();
                    //Main.NewText($"CLIENT: Received UpdatePlayerState packet for player {statePlayerIdx}, state: {(PlayerManager.PlayerState)newState}, myPlayer: {Main.myPlayer}", Color.Yellow);
                    if (statePlayerIdx == Main.myPlayer)
                    {
                        var playerManager = Main.player[statePlayerIdx].GetModPlayer<PlayerManager>();
                        var targetState = (PlayerManager.PlayerState)newState;

                        // If entering ClassSelection state, determine if this is game start based on match stage
                        if (targetState == PlayerManager.PlayerState.ClassSelection)
                        {
                            bool isGameStart = (GameInfo.matchStage == 1); // Stage 1 = Class Selection during game start
                            playerManager.HandleEnterClassSelection(isGameStart);
                            //Main.NewText($"CLIENT: Entering class selection, isGameStart: {isGameStart}, matchStage: {GameInfo.matchStage}", Color.Cyan);
                        }
                        else if (targetState == PlayerManager.PlayerState.Active)
                        {
                            playerManager.HandleExitClassSelection();
                        }
                        else
                        {
                            playerManager.changePlayerState(targetState);
                        }

                        //Main.NewText($"CLIENT: Updated player state to {targetState}", Color.Green);
                    }
                    else
                    {
                        //Main.NewText($"CLIENT: Ignoring state update packet - not for local player", Color.Orange);
                    }
                    break;

                case (byte)MessageType.UpdatePlayerKDR:
                    int statePlayerIdxx = reader.ReadInt32();
                    int newKills = reader.ReadInt32();
                    int newDeaths = reader.ReadInt32();
                    int newDamage = reader.ReadInt32();

                    if (statePlayerIdxx == Main.myPlayer)
                    {
                        var playerManager = Main.player[statePlayerIdxx].GetModPlayer<PlayerManager>();

                        playerManager.kills = newKills;
                        playerManager.deaths = newDeaths;
                        playerManager.damage = newDamage;
                    }
                    break;
                case (byte)MessageType.RequestViewMap:
                    int playerMapView = reader.ReadInt32();

                    // Get the map queue
                    var mapArray = manager.mapQueue.ToArray();

                    if (mapArray.Length == 0)
                    {
                        ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("Map queue is empty."), Color.Gray, playerMapView);
                    }
                    else
                    {
                        ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral($"=== Map Queue ({mapArray.Length} maps) ==="), Color.Yellow, playerMapView);

                        for (int i = 0; i < mapArray.Length; i++)
                        {
                            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral($"{i + 1}. {mapArray[i]}"), Color.White, playerMapView);
                        }
                    }

                    Console.WriteLine($"Server: Sent map queue ({mapArray.Length} maps) to player {playerMapView}");
                    break;
                case (byte)MessageType.RequestTeamChat:
                    int playerWhoTalked = reader.ReadInt32();
                    string teamMessage = reader.ReadString();

                    // Get the player who sent the message
                    var senderPlayer = Main.player[playerWhoTalked];
                    if (!senderPlayer.active)
                    {
                        Console.WriteLine($"Invalid player {playerWhoTalked} tried to send team chat");
                        break;
                    }

                    int senderTeam = senderPlayer.team;

                    // Check if player is on a team
                    if (senderTeam == 0)
                    {
                        ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral("You are not on a team!"), Color.Red, playerWhoTalked);
                        break;
                    }

                    // Get team name and color
                    string teamName = GetTeamName(senderTeam);
                    Color teamColor = GetTeamColor(senderTeam);


                    string formattedMessage = $"[{teamName}] {senderPlayer.name}: {teamMessage}";

                    int messagesSent = 0;
                    foreach (Player team_player in Main.player)
                    {
                        if (team_player.active && team_player.team == senderTeam)
                        {
                            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(formattedMessage), teamColor, team_player.whoAmI);
                            messagesSent++;
                        }
                    }

                    Console.WriteLine($"Server: Sent team chat from {senderPlayer.name} to {messagesSent} players on {teamName} team");
                    break;
                case (byte)MessageType.RequestClassSelection:
                    int playerSelecting = reader.ReadInt32();
                    string classSelected = reader.ReadString();

                    // Get the player who sent the message
                    var selectingPlayer = Main.player[playerSelecting];
                    if (!selectingPlayer.active)
                    {
                        Console.WriteLine($"Invalid player {playerSelecting} tried to pick a class");
                        break;
                    }

                    int team = selectingPlayer.team;

                    string formattedMsg = $"{selectingPlayer.name} picked {classSelected}";

                    foreach (Player team_player in Main.player)
                    {
                        if (team_player.active && team_player.team == team)
                            ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(formattedMsg), Color.Yellow, team_player.whoAmI);
                    }
                    break;
                case (byte)MessageType.RequestAudio:
                    string filepath = reader.ReadString();
                    SoundStyle sound = new SoundStyle(filepath);
                    SoundEngine.PlaySound(sound);
                    break;
                case (byte)MessageType.RequestAudioClientSide:
                    string filepathh = reader.ReadString();
                    var audoMod = ModContent.GetInstance<CTG2>();
                    ModPacket audoPacket = audoMod.GetPacket();
                    audoPacket.Write((byte)MessageType.AudioClientSide);
                    audoPacket.Write(filepathh);
                    audoPacket.Send();
                    break;
                case (byte)MessageType.AudioClientSide:
                    string filepathhh = reader.ReadString();
                    SoundStyle sounddd = new SoundStyle(filepathhh);
                    SoundEngine.PlaySound(sounddd);
                    break;
                case (byte)MessageType.RequestAudioToClient:
                    string filepa = reader.ReadString();
                    int playerIndx = reader.ReadInt32();

                    var audMod = ModContent.GetInstance<CTG2>();
                    ModPacket audioPacket = audMod.GetPacket();
                    audioPacket.Write((byte)MessageType.RequestAudioToClientPacket);
                    audioPacket.Write(filepa);
                    audioPacket.Send(toClient: playerIndx);
                    break;
                case (byte)MessageType.RequestAudioToClientPacket:
                    string filep = reader.ReadString();
                    SoundStyle soun = new SoundStyle(filep);
                    SoundEngine.PlaySound(soun);
                    break;
                case (byte)MessageType.RequestDie:
                    int playerWhoDies = reader.ReadInt32();

                    // Get the player who wants to die
                    var dyingPlayer = Main.player[playerWhoDies];
                    if (!dyingPlayer.active)
                    {
                        Console.WriteLine($"Invalid player {playerWhoDies} tried to die");
                        break;
                    }


                    dyingPlayer.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{dyingPlayer.name} committed suicide")), 9999, 0);

                    // Sync the death to all clients
                    NetworkText deathText = NetworkText.FromLiteral($"{dyingPlayer.name} committed suicide");
                    NetMessage.SendData(MessageID.PlayerDeathV2, -1, -1, deathText, playerWhoDies, 0f, 0f, 0f);
                    Console.WriteLine($"Server: Player {dyingPlayer.name} killed themselves");
                    break;

                case (byte)MessageType.RequestKill:
                    int adminPlayer = reader.ReadInt32();
                    string targetPlayerName = reader.ReadString();

                    // Find the target player by name
                    Player targetPlayer = null;
                    foreach (Player p in Main.player)
                    {
                        if (p.active && p.name.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase))
                        {
                            targetPlayer = p;
                            break;
                        }
                    }

                    if (targetPlayer == null)
                    {
                        ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral($"Player '{targetPlayerName}' not found or not online."), Color.Red, adminPlayer);
                        break;
                    }

                    var adminPlayerObj = Main.player[adminPlayer];


                    targetPlayer.KillMe(Terraria.DataStructures.PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{targetPlayer.name} was killed by admin {adminPlayerObj.name}")), 9999, 0);

                    // Sync the death to all clients
                    NetworkText killDeathText = NetworkText.FromLiteral($"{targetPlayer.name} was killed by admin {adminPlayerObj.name}");
                    NetMessage.SendData(MessageID.PlayerDeathV2, -1, -1, killDeathText, targetPlayer.whoAmI, 0f, 0f, 0f);

                    ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral($"Killed player {targetPlayer.name}"), Color.Green, adminPlayer);

                    Console.WriteLine($"Server: Admin {adminPlayerObj.name} killed player {targetPlayer.name}");
                    break;
                case (byte)MessageType.RequestWeb:
                    int webPlayerIndex = reader.ReadInt32();
                    int webTimeInTicks = reader.ReadInt32();

                    // Get the player to web
                    var playerToWeb = Main.player[webPlayerIndex];
                    if (!playerToWeb.active)
                    {
                        Console.WriteLine($"Invalid player {webPlayerIndex} tried to be webbed");
                        break;
                    }


                    playerToWeb.AddBuff(BuffID.Webbed, webTimeInTicks);
                    NetMessage.SendData(MessageID.AddPlayerBuff, -1, -1, null, webPlayerIndex, BuffID.Webbed, webTimeInTicks);

                    Console.WriteLine($"Server: Webbed player {playerToWeb.name} for {webTimeInTicks / 60} seconds");
                    break;

                case (byte)MessageType.RequestSyncStats:
                    {
                        var fromWho = reader.ReadByte();
                        var modPlayer = Main.player[fromWho].GetModPlayer<ClassSystem>();
                        modPlayer.ReceiveSync(reader);

                        if (Main.netMode == NetmodeID.Server)
                        {
                            modPlayer.SyncPlayer(-1, fromWho);
                        }
                        //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"sync called once"), Color.Orange);
                        break;
                    }
                case (byte)MessageType.SyncPlayerArmor:
                    byte playerIDd = reader.ReadByte();
                    Player targetd = Main.player[playerIDd];
                    for (int i = 0; i < targetd.armor.Length; i++)
                    {
                        short netID = reader.ReadInt16();
                        byte prefix = reader.ReadByte();
                        targetd.armor[i].SetDefaults(netID);
                        targetd.armor[i].Prefix(prefix);
                    }
                    for (int j = 0; j < targetd.dye.Length; j++)
                    {
                        short netID = reader.ReadInt16();
                        targetd.dye[j].SetDefaults(netID);
                    }
                    break;
                case (byte)MessageType.RequestFullHeal:
                    {
                        int plyrindex = reader.ReadInt32();
                        Main.player[plyrindex].Heal(200);
                    }
                    break;
                case (byte)MessageType.RequestHeal:
                {
                    if (Main.netMode != NetmodeID.Server)
                        break;
                
                    int plyrindex = reader.ReadInt32();
                    int amt = reader.ReadInt32();
                
                    Player player = Main.player[plyrindex];
                    if (!player.active)
                        break;
                
                    // Server heals
                    player.Heal(amt);
                
                    // Sync to all clients (including sender)
                    ModPacket packet = GetPacket();
                    packet.Write((byte)MessageType.SyncHeal);
                    packet.Write(plyrindex);
                    packet.Write(amt);
                    packet.Send();

                    break;
                }

                case (byte)MessageType.SyncHeal:
                {
                    // SERVER -> CLIENTS ONLY
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        break;
                
                    int playerID = reader.ReadInt32();
                    int amount = reader.ReadInt32();
                
                    Player player = Main.player[playerID];
                    if (!player.active)
                        break;
                
                    player.Heal(amount);
                    break;
                }

                // ...existing code...
                case (byte)MessageType.RequestMute:
                    {
                        int mutedplayer = reader.ReadInt32();
                        // Send mute packet to the target player
                        var muteMod = ModContent.GetInstance<CTG2>();
                        ModPacket packet = muteMod.GetPacket();
                        packet.Write((byte)MessageType.Mute);
                        packet.Write(mutedplayer);
                        packet.Send(toClient: mutedplayer);
                        break;
                    }
                case (byte)MessageType.Mute:
                    {
                        int mutedPlayer = reader.ReadInt32();
                        if (mutedPlayer == Main.myPlayer)
                        {
                            //Main.player[mutedPlayer].GetModPlayer<ChatPlayer>().IsMuted = true;
                            Main.NewText("You have been muted.", Microsoft.Xna.Framework.Color.Red);
                        }
                        break;
                    }
                case (byte)MessageType.RequestUnmute:
                    {
                        int unmuteplayer = reader.ReadInt32();
                        var unmutemod = ModContent.GetInstance<CTG2>();
                        ModPacket packet = unmutemod.GetPacket();
                        packet.Write((byte)MessageType.Unmute);
                        packet.Write(unmuteplayer);
                        packet.Send(toClient: unmuteplayer);
                        break;
                    }
                case (byte)MessageType.Unmute:
                    {
                        int unmutePlayer = reader.ReadInt32();
                        if (unmutePlayer == Main.myPlayer)
                        {
                            //Main.player[unmutePlayer].GetModPlayer<ChatPlayer>().IsMuted = false;
                            Main.NewText("You have been unmuted.", Microsoft.Xna.Framework.Color.Green);
                        }
                        break;
                    }
                case (byte)MessageType.LateJoin:
                    {
                        int joiningPlayer = reader.ReadInt32();
                        if (manager.pubsConfig)
                        {
                            int redCount = 0, blueCount = 0;
                            foreach (Player p in Main.player)
                            {
                                if (p.active && !p.dead)
                                {
                                    if (p.team == 1) redCount++;
                                    else if (p.team == 3) blueCount++;
                                }
                            }

                            int teamno;
                            if (redCount < blueCount)
                                teamno = 1;
                            else if (blueCount < redCount)
                                teamno = 3;
                            else
                                teamno = Main.rand.NextBool() ? 1 : 3;

                            var teammod = ModContent.GetInstance<CTG2>();
                            ModPacket packet = teammod.GetPacket();
                            packet.Write((byte)MessageType.RequestTeamChange);
                            packet.Write(joiningPlayer);
                            packet.Write(teamno);
                            packet.Send();

                            manager.startPlayerClassSelection(joiningPlayer, true);
                        }
                        break;
                    }
                case (byte)MessageType.RequestGamemodeChange:
                    {
                        // Read the mode once
                        string mode = reader.ReadString();

                        var managerForServer = ModContent.GetInstance<GameManager>();
                        if (Main.netMode == NetmodeID.Server)
                        {
                            // Server-side: update authoritative gamemode and broadcast to clients
                            if (mode == "pubs")
                            {
                                managerForServer.pubsConfig = true;
                                managerForServer.scrimsConfig = false;
                                managerForServer.rngConfig = false;
                            }
                            else if (mode == "scrims")
                            {
                                managerForServer.pubsConfig = false;
                                managerForServer.scrimsConfig = true;
                                managerForServer.rngConfig = false;
                            }
                            else if (mode == "rng")
                            {
                                managerForServer.pubsConfig = true;
                                managerForServer.scrimsConfig = false;
                                managerForServer.rngConfig = true;
                            }
                            else
                            {
                                managerForServer.pubsConfig = false;
                                managerForServer.scrimsConfig = false;
                                managerForServer.rngConfig = false;
                            }

                            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[GAME] Gamemode set to {mode}."), Color.Orange);

                            ModPacket syncPacket = GetPacket();
                            syncPacket.Write((byte)MessageType.UpdateGamemode);
                            syncPacket.Write(mode);
                            syncPacket.Send();
                        }
                        else
                        {
                            var gmLocal = ModContent.GetInstance<GameManager>();
                            gmLocal.pubsConfig = mode == "pubs";
                            gmLocal.scrimsConfig = mode == "scrims";
                            gmLocal.rngConfig = mode == "rng";
                        }

                        break;
                    }
                case (byte)MessageType.UpdateGamemode:
                    {
                        string mode = reader.ReadString();
                        var gmClient = ModContent.GetInstance<GameManager>();
                        gmClient.pubsConfig = false;
                        gmClient.scrimsConfig = false;
                        gmClient.rngConfig = false;

                        if (mode == "pubs")
                            gmClient.pubsConfig = true;
                        else if (mode == "scrims")
                            gmClient.scrimsConfig = true;
                        else if (mode == "rng")
                            gmClient.rngConfig = true;
                            gmClient.pubsConfig = true;

                        Main.NewText($"Gamemode synced: {mode}", Color.Orange);
                        break;
                    }
                case (byte)MessageType.RequestBanPlayer:
                    ProcessRequestBanPlayer(ref reader, whoAmI);
                    break;

                case (byte)MessageType.ArcherDash:
                        byte plyNum = reader.ReadByte();
                        Player plyy = Main.player[plyNum];
                        ArcherDashPlayer dashPly = plyy.GetModPlayer<ArcherDashPlayer>();

                        dashPly.RecieveDash(plyy, reader);

                        if (Main.netMode == NetmodeID.Server)
                            dashPly.SendDash(plyy.velocity, -1, whoAmI);

                        break;

                case (byte)MessageType.DASH:
                    {
                        byte plyNum1 = reader.ReadByte();
                        Player ply1 = Main.player[plyNum1];
                        DashPlayer3 dashPlyy = ply1.GetModPlayer<DashPlayer3>();
                        dashPlyy.RecieveDash(ply1, reader);
                        if (Main.netMode == 2)
                        {
                            dashPlyy.SendDash(-1, whoAmI);
                        }
                        break;
                    }

                case (byte)MessageType.FORCE_JUMP:
                    byte plyNum2 = reader.ReadByte();
                    Player ply2 = Main.player[plyNum2];
                    ply2.GetModPlayer<WallJumpPlayer>().RecieveForceJump(ply2, reader, whoAmI);
                    break;
                case (byte)MessageType.GRAB_KEYS:
                    {
                        byte plyNum3 = reader.ReadByte();
                        Player ply3 = Main.player[plyNum3];
                        WallJumpPlayer jumpPly = ply3.GetModPlayer<WallJumpPlayer>();
                        jumpPly.RecieveGrabKeys(ply3, reader);
                        if (Main.netMode == 2)
                        {
                            jumpPly.SendGrabKeys(-1, whoAmI);
                        }
                        break;
                    }

                case (byte)MessageType.UpdateMusic:
                    // if (Main.netMode == NetmodeID.MultiplayerClient)
                    // {
                    //     // Read the music path sent by the server.
                    //     string musicPath = reader.ReadString();
                    //     int newMusicId = -1;

                    //     if (!string.IsNullOrEmpty(musicPath) && ModContent.HasAsset(musicPath))
                    //     {
                    //         // Get the integer ID for the music path.
                    //         newMusicId = MusicLoader.GetMusicSlot(this, musicPath);
                    //     }
                    //     else
                    //     {
                    //         Logger.WarnFormat("errorM");
                    //     }

                    //     // Update the static variable in our biome.
                    //         GameMusicBiome.ChangeMusic(newMusicId);

                    //     // Manually trigger the music fade. This is important!
                    //     // Just changing the biome's property won't work if the biome is already active.
                    //     if (Main.curMusic != newMusicId)
                    //     {
                    //         Main.newMusic = newMusicId;
                    //     }
                    // }
                    break;
                case (byte)MessageType.SyncBiomeMusic:
                    int newMusicId = reader.ReadInt32();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        //Main.newMusic = newMusicId;
                    }

                    break;
                    
                case (byte)MessageType.RequestDictAddAndSync:
                    string uuid = reader.ReadString();
                    if (!PlayerDataDictionary.ContainsKey(uuid)) //add and sync
                    {
                        PlayerData newPlayer = new PlayerData(uuid);
                        PlayerDataDictionary.Add(uuid, newPlayer);
                        ModPacket packet3 = ModContent.GetInstance<CTG2>().GetPacket();
                        packet3.Write((byte)MessageType.SyncDict);
                        packet3.Write(PlayerDataDictionary.Count);
                        foreach (var kv in PlayerDataDictionary)
                        {
                            packet3.Write(kv.Key);          
                            kv.Value.NetSend(packet3);      
                        }
                        packet3.Send(); //send new synced dictionary to all
                    }
                    else
                    { //only sync dont add
                        ModPacket packet4 = ModContent.GetInstance<CTG2>().GetPacket();
                        packet4.Write((byte)MessageType.SyncDict);
                        packet4.Write(PlayerDataDictionary.Count);
                        foreach (var kv in PlayerDataDictionary)
                        {
                            packet4.Write(kv.Key);          
                            kv.Value.NetSend(packet4);      
                        }
                        packet4.Send(toClient: whoAmI); //since player already existed just send current dictionary 
                    }
                    break;

                case (byte)MessageType.SyncDict:
                    int n = reader.ReadInt32();
                    PlayerDataDictionary.Clear();
                    for (int i = 0; i < n; i++) {
                        string key = reader.ReadString();
                        var val = PlayerData.NetReceive(reader);
                        PlayerDataDictionary[key] = val;
                    }
                    //Main.NewText("Received Dictionary Sync successfuly");
                    break; 

                case (byte)MessageType.RequestMakeMeAdmin:
                {
                    string uuid2 = reader.ReadString();

                    if (!PlayerDataDictionary.TryGetValue(uuid2, out var pd)) {
                        pd = new PlayerData(uuid2);
                        PlayerDataDictionary[uuid2] = pd;
                    }
                    pd.setAdmin();

                    var p = ModContent.GetInstance<CTG2>().GetPacket();
                    p.Write((byte)MessageType.SyncDict);
                    p.Write(PlayerDataDictionary.Count);
                    foreach (var kv in PlayerDataDictionary) {
                        p.Write(kv.Key);
                        kv.Value.NetSend(p);
                    }
                    p.Send(); 
                    break;
                }
                case (byte)MessageType.SendNewChatFlair:
                {
                    string uuid3 = reader.ReadString();
                    int itemId = reader.ReadInt32();

                    if (!PlayerDataDictionary.TryGetValue(uuid3, out var pd)) {
                        pd = new PlayerData(uuid3);
                        PlayerDataDictionary[uuid3] = pd;
                    }
                    pd.chatFlairItemId = itemId;
                    var p = GetPacket();
                    p.Write((byte)MessageType.SyncDict);
                    p.Write(PlayerDataDictionary.Count);
                    foreach (var kv in PlayerDataDictionary) {
                        p.Write(kv.Key);
                        kv.Value.NetSend(p);
                    }
                    p.Send();
                    break;
                }
                case (byte)MessageType.RegisterUuid:
                {
                    string uuid4 = reader.ReadString();
                    int idx = whoAmI;

                    CTG2.UuidByWhoAmI[idx] = uuid4;
                    if (!PlayerDataDictionary.TryGetValue(uuid4, out var pd))
                        PlayerDataDictionary[uuid4] = new PlayerData(uuid4);

                    var mp = GetPacket();
                    mp.Write((byte)MessageType.SyncUuidForPlayer);
                    mp.Write(idx);
                    mp.Write(uuid4);
                    mp.Send();
                    break;
                }
                case (byte)MessageType.SyncUuidForPlayer:
                {
                    int idx = reader.ReadInt32();
                    string uuid5 = reader.ReadString();
                    UuidByWhoAmI[idx] = uuid5;
                    break;
                }

                default:
                    Logger.WarnFormat("CTG2: Unknown Message type: {0}", msgType);
                    break;

            }
        }
        private static void ProcessRequestBanPlayer(ref BinaryReader reader, int playerNumber)
        {
            string playertoban = reader.ReadString();
            for (int k = 0; k < 255; k++)
            {
                if (Main.player[k].active && Main.player[k].name.ToLower() == playertoban)
                {
                    Netplay.AddBan(k);
                    NetMessage.SendData(2, k, -1, NetworkText.FromKey("CLI.BanMessage", new object[0]), 0, 0f, 0f, 0f, 0, 0, 0);
                }
            }

        }


        public void setRequestedNpcIndex(int index)
        {
            CTG2.requestedNpcIndex = index;
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.SyncNpcIndex);
            packet.Write(index);
            packet.Send();
        }

        public static void SendEnterSpectatorRequest(int playerIndex) // for spectate command
        {
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestEnterSpectator);
            packet.Write(playerIndex);
            packet.Send();
        }

        public static void SendExitSpectatorRequest(int playerIndex)
        {
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestExitSpectator);
            packet.Write(playerIndex);
            packet.Send();
        }

        public static void WebPlayer(int playerIndex, int timeIntTicks)
        {
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestWeb);
            packet.Write(playerIndex);
            packet.Write(timeIntTicks);
            packet.Send();
        }
    }
}
