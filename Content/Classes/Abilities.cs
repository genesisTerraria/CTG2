using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures; 
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;
using ClassesNamespace;
using CTG2.Content.ClientSide;
using CTG2.Content.ServerSide;
using CTG2.Content.Buffs;
using CTG2.Content.Classes;
using CTG2.Content.Items;
using Terraria.Audio;
using Microsoft.Xna.Framework.Audio;


namespace CTG2.Content
{
    public class Abilities : ModPlayer
    {
        public int cooldown = 0;

        public int class1EndTimer = -1;

        public bool playedSound = false;
        public int class3SpawnTimer = -1;
        public bool class3PendingSpawn = false;

        public int class4BuffTimer = -1;
        public bool class4PendingBuffs = false;

        public int class5EndTimer = -1;

        public int class7HitCounter = 0;
        public int class7EndTimer = -1;

        public int class8HP = 0;
        public bool psychicActive = false;

        public int class11EndTimer = -1;

        public int class12SwapTimer = -1;
        public int class12ClosestDist = 99999;
        public Player class12ClosestPlayer = null;

        public int class13EndTimer = -1;

        public int class15AbilityTimer = -1;

        public CtgClass class16RushData;
        public CtgClass class16RegenData;

        public int class17EndTimer = -1;

        public bool initializedMutant;
        public int mutantState = 1;

        SoundStyle abilityReady = new SoundStyle("CTG2/Content/Classes/AbilityReady");
        SoundStyle whiteMageHeal = new SoundStyle("CTG2/Content/Classes/WhiteMageHeal");
        SoundStyle clownSwap = new SoundStyle("CTG2/Content/Classes/ClownSwap");


        private int GetItemIDByName(string itemName)
        {
            for (int i = 5546; i < ItemLoader.ItemCount; i++)
            {
                ModItem modItem = ItemLoader.GetItem(i);
                if (modItem != null && modItem.Name.Equals(itemName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void SetInventory(CtgClass classData, bool rmoot)
        {
            Player.statLifeMax2 = classData.HealthPoints;
            Player.statManaMax2 = classData.ManaPoints;

            List<ItemData> classItems = classData.InventoryItems;

            bool placedWeapon = false;
            bool placedMushrooms = false;

            for (int b = 0; b < Player.inventory.Length; b++)
            {
                if (Player.inventory[b].type == ItemID.TheRottedFork && rmoot)
                {
                    Item newItem = new Item();
                    newItem.SetDefaults(GetItemIDByName("Amalgamated Hand"));
                    newItem.stack = 1;
                    Player.inventory[b] = newItem;
                    placedWeapon = true;
                }
                else if (Player.inventory[b].type == ModContent.ItemType<AmalgamatedHand>() && !rmoot) //use GetItemIDByName for Amalgamated Hand
                {
                    Item newItem = new Item();
                    newItem.SetDefaults(ItemID.TheRottedFork);
                    newItem.stack = 1;
                    Player.inventory[b] = newItem;
                    placedWeapon = true;
                }
                else if (b > 29 && b < 50 && Player.inventory[b].type == ItemID.None && !placedMushrooms && !rmoot)
                {
                    Item newItem = new Item();
                    newItem.SetDefaults(ItemID.Mushroom);
                    newItem.stack = 99;
                    Player.inventory[b] = newItem;
                    placedMushrooms = true;
                }
                else if (Player.inventory[b].type == ItemID.CrimsonHelmet || Player.inventory[b].type == ItemID.CrimsonScalemail || Player.inventory[b].type == ItemID.CrimsonGreaves ||
                         Player.inventory[b].type == ItemID.CharmofMyths || Player.inventory[b].type == ItemID.WormScarf || Player.inventory[b].type == ItemID.FireGauntlet || Player.inventory[b].type == ItemID.FrozenTurtleShell ||
                         Player.inventory[b].type == ItemID.BlizzardinaBottle || Player.inventory[b].type == ItemID.EoCShield || Player.inventory[b].type == ItemID.Magiluminescence || Player.inventory[b].type == ItemID.DestroyerEmblem ||
                         Player.inventory[b].type == ItemID.DevilHorns || Player.inventory[b].type == ItemID.FlowerBoyShirt || Player.inventory[b].type == ItemID.FlowerBoyPants || Player.inventory[b].type == ItemID.LizardTail ||
                         Player.inventory[b].type == ItemID.ApprenticeScarf || Player.inventory[b].type == ItemID.Yoraiz0rDarkness || Player.inventory[b].type == 5558 || Player.inventory[b].type == 4772 || 
                         Player.inventory[b].type == 1839 || Player.inventory[b].type == 1748 || Player.inventory[b].type == ItemID.Mushroom || Player.inventory[b].type == 4663 || Player.inventory[b].type == 1007 || Player.inventory[b].type == 1019)
                {
                    Item newItem = new Item();
                    newItem.TurnToAir();
                    Player.inventory[b] = newItem;
                }
            }

            for (int c = 0; c < Player.armor.Length; c++)
            {
                var itemData = classItems[Player.inventory.Length + c];
                Item newItem = new Item();
                newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                Player.armor[c] = newItem;
                // if (Player.armor[c].type == ItemID.EoCShield && Player.GetModPlayer<ClassSystem>().simpleDashSelected) {
                //     Item simpledash = new Item(5558,1,0);
                //     Player.armor[c] = simpledash;
                // }
            }

            for (int cc = 0; cc < Player.dye.Length; cc++)
            {
                var itemData = classItems[Player.inventory.Length + Player.armor.Length + cc];
                Item newItem = new Item();
                newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                Player.dye[cc] = newItem;
            }

            for (int d = 0; d < Player.miscEquips.Length; d++)
            {
                var itemData = classItems[Player.inventory.Length + Player.dye.Length +Player.armor.Length + d];
                Item newItem = new Item();
                newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                Player.miscEquips[d] = newItem;
            }

            for (int e = 0; e < Player.miscDyes.Length; e++)
            {
                var itemData = classItems[Player.inventory.Length +Player.armor.Length + Player.dye.Length + Player.miscEquips.Length + e];
                Item newItem = new Item();
                newItem.SetDefaults(itemData.Type);
                newItem.stack = itemData.Stack;
                newItem.Prefix(itemData.Prefix);

                Player.miscDyes[e] = newItem;
            }

            Player.trashItem = new Item();
            Main.mouseItem = new Item();
        }

        private int TryAddItemToInventory(Player target, int itemType, int amount)
        {
            if (amount <= 0)
                return 0;

            int remaining = amount;
            int maxStack = ContentSamples.ItemsByType[itemType].maxStack;

            bool isCoin = itemType == ItemID.CopperCoin
                         || itemType == ItemID.SilverCoin
                         || itemType == ItemID.GoldCoin
                         || itemType == ItemID.PlatinumCoin;
            bool isAmmo = ContentSamples.ItemsByType[itemType].ammo > 0;

            // Top off existing stacks of the same item (anywhere)
            for (int i = 0; i < target.inventory.Length && remaining > 0; i++)
            {
                Item it = target.inventory[i];
                if (it != null && it.type == itemType && it.stack < maxStack)
                {
                    int space = maxStack - it.stack;
                    int take = Math.Min(space, remaining);
                    it.stack += take;
                    remaining -= take;
                }
            }

            const int reservedCoins = 4;
            const int reservedAmmo = 4;
            const int preferredCoinStartIndex = 50; // first coin slot index
            int invLen = target.inventory.Length;
            int coinStart;
            int ammoStart;

            if (invLen > preferredCoinStartIndex + reservedCoins + reservedAmmo - 1)
            {
                coinStart = preferredCoinStartIndex;
                ammoStart = coinStart + reservedCoins;
            }
            else
            {
                int totalReserved = reservedCoins + reservedAmmo;
                coinStart = Math.Max(0, invLen - totalReserved);
                ammoStart = coinStart + reservedCoins;
            }

            coinStart = Math.Min(Math.Max(0, coinStart), invLen);
            ammoStart = Math.Min(Math.Max(coinStart, ammoStart), invLen);

            // Fill non-reserved slots first (never touch reserved coin/ammo slots for non-coin/non-ammo items)
            for (int i = 0; i < coinStart && remaining > 0; i++)
            {
                Item it = target.inventory[i];
                if (it == null || it.type == ItemID.None)
                {
                    int give = Math.Min(remaining, maxStack);
                    Item newItem = new Item();
                    newItem.SetDefaults(itemType);
                    newItem.stack = give;
                    target.inventory[i] = newItem;
                    remaining -= give;
                }
            }

            // If item is a coin, try coin reserved slots (only coins)
            if (remaining > 0 && isCoin)
            {
                for (int i = coinStart; i < Math.Min(coinStart + reservedCoins, invLen) && remaining > 0; i++)
                {
                    Item it = target.inventory[i];
                    if (it != null && it.type == itemType && it.stack < maxStack)
                    {
                        int space = maxStack - it.stack;
                        int take = Math.Min(space, remaining);
                        it.stack += take;
                        remaining -= take;
                    }
                    else if (it == null || it.type == ItemID.None)
                    {
                        int give = Math.Min(remaining, maxStack);
                        Item newItem = new Item();
                        newItem.SetDefaults(itemType);
                        newItem.stack = give;
                        target.inventory[i] = newItem;
                        remaining -= give;
                    }
                }
            }

            // If item is ammo, try ammo reserved slots (only ammo)
            if (remaining > 0 && isAmmo)
            {
                for (int i = ammoStart; i < Math.Min(ammoStart + reservedAmmo, invLen) && remaining > 0; i++)
                {
                    Item it = target.inventory[i];
                    if (it != null && it.type == itemType && it.stack < maxStack)
                    {
                        int space = maxStack - it.stack;
                        int take = Math.Min(space, remaining);
                        it.stack += take;
                        remaining -= take;
                    }
                    else if (it == null || it.type == ItemID.None)
                    {
                        int give = Math.Min(remaining, maxStack);
                        Item newItem = new Item();
                        newItem.SetDefaults(itemType);
                        newItem.stack = give;
                        target.inventory[i] = newItem;
                        remaining -= give;
                    }
                }
            }

            // Server sync if anything changed
            if (remaining != amount && Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, target.whoAmI);
            }

            return remaining;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            int projectileType = info.DamageSource.SourceProjectileType;
            int attackerIndex = info.DamageSource.SourcePlayerIndex;

            if (attackerIndex >= 0 && attackerIndex < Main.maxPlayers)
            {
                Player attacker = Main.player[attackerIndex];
                int damage = info.Damage;
                var attackerManager = attacker.GetModPlayer<PlayerManager>();
                int selectedClass = attackerManager.currentClass.AbilityID;

                if (selectedClass == 18 && attacker.team != Player.team)
                {
                    int itemType = Main.rand.Next(1, ItemLoader.ItemCount);
                    int amount = ContentSamples.ItemsByType[itemType].maxStack;

                    int remaining = TryAddItemToInventory(attacker, itemType, amount);

                    if (remaining > 0)
                    {
                        // fallback to quickspawn for any remainder (keeps previous behavior when inventory is full)
                        attacker.QuickSpawnItem(null, itemType, remaining);
                    }
                }

                switch (projectileType)
                {
                    case ProjectileID.HellfireArrow: // Archer ability
                        if (attacker.HasBuff(320) && attacker.team != Player.team)
                        {
                            Player.AddBuff(30, 60);
                            Player.AddBuff(32, 60);
                            Player.AddBuff(44, 60);
                        }
                        break;

                    case 15:
                    case 19: // Flame Bunny ability
                        if (attacker.HasBuff(320) && attacker.HasBuff(137) && attacker.team != Player.team)
                        {
                            Player.AddBuff(39, 20);
                            Player.AddBuff(70, 20);
                        }
                        break;

                    case 273:
                    case 304: // Leech ability
                        if (attacker.HasBuff(320) && attacker.team != Player.team)
                        {
                            int healAmount = damage / 10;
                            attacker.statLife += healAmount;
                            attacker.HealEffect(healAmount);
                        }
                        break;
                }
            }
        }


        private void SetCooldown(int seconds)
        {
            cooldown = seconds * 60;
        }


        private void ArcherOnUse()
        {
            Player.AddBuff(320, 6 * 60);
            class1EndTimer = 360;

            playedSound = false;

            SoundEngine.PlaySound(SoundID.DD2_PhantomPhoenixShot.WithVolumeScale(Main.soundVolume * 6f), Player.Center);
        }


        private void NinjaOnUse()
        {
            Player.AddBuff(BuffID.Invisibility, 60 * 60);

            playedSound = false;

            SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void BeastOnUse()
        {
            playedSound = false;

            Player.AddBuff(BuffID.Electrified, 30);

            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAddBuff);
            packet.Write(Player.whoAmI);
            packet.Write(BuffID.Electrified);
            packet.Write(30);
            packet.Send();

            class3SpawnTimer = 30;
            class3PendingSpawn = true;

            SoundEngine.PlaySound(SoundID.DD2_BetsySummon.WithVolumeScale(Main.soundVolume * 4f), Player.Center);
        }


        private void BeastPostStatus()
        {
            if (class3PendingSpawn)
            {
                if (class3SpawnTimer <= 0)
                {
                    var mod = ModContent.GetInstance<CTG2>();
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)MessageType.RequestSpawnNpc);
                    packet.Write((int)Player.Center.X);
                    packet.Write((int)Player.Center.Y);
                    packet.Write(-2);
                    packet.Write(Player.team);
                    packet.Write(0f);
                    packet.Send();

                    SoundEngine.PlaySound(SoundID.DD2_BetsyScream.WithVolumeScale(Main.soundVolume * 4f), Player.Center);

                    class3PendingSpawn = false;
                }
            }
        }


        private void GladiatorOnUse()
        {
            Player.AddBuff(206, 300);
            Player.AddBuff(195, 300);
            Player.AddBuff(75, 300);
            Player.AddBuff(320, 300);

            class4BuffTimer = 300;
            class4PendingBuffs = true;

            playedSound = false;

            SoundEngine.PlaySound(SoundID.DD2_KoboldIgnite.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void GladiatorPostStatus()
        {
            if (class4PendingBuffs) // runs 5 second interval
            {
                if (class4BuffTimer <= 0)
                {

                    Player.AddBuff(137, 90);
                    Player.AddBuff(32, 90);
                    Player.AddBuff(195, 90);
                    Player.AddBuff(5, 90);
                    Player.AddBuff(215, 90);

                    class4PendingBuffs = false;
                }
            }
        }


        private void PaladinOnUse()
        {
            var mod = ModContent.GetInstance<CTG2>();

            Player.AddBuff(32, 270); // Chilled
            Player.AddBuff(ModContent.BuffType<Immortality>(), 270); // Immortality

            playedSound = false;
            class5EndTimer = 270;

            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact.WithVolumeScale(Main.soundVolume * 2f), Player.Center);

            for (int i = 3; i < Player.armor.Length - 3; i++)
            {
                if (Player.armor[i].type == ItemID.None)
                {
                    Player.armor[i].SetDefaults(ItemID.CobaltShield);
                    break;
                }
            }
        }


        private void PaladinClear()
        {
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == ItemID.CobaltShield)
                {
                    Player.inventory[i] = new Item();
                }
            }

            for (int i = 0; i < Player.armor.Length; i++)
            {
                if (Player.armor[i].type == ItemID.CobaltShield)
                {
                    Player.armor[i] = new Item();
                }
            }

            Player.trashItem = new Item();
            Main.mouseItem = new Item();
        }


        private void TankOnUse()
        {
            playedSound = false;
        }
        
        
        private void BlackMageOnUse()
        {
            Player.AddBuff(176, 15);
            Player.AddBuff(206, 420);
            Player.AddBuff(137, 420);
            Player.AddBuff(320, 420);

            Player.statMana = Player.statManaMax2;

            class7EndTimer = 420;
            // if (Player.GetModPlayer<Abilities>().class7HitCounter >= 10)
            // {
            //     Player.AddBuff(176, 15);
            //     Player.AddBuff(206, 420);
            //     Player.AddBuff(137, 420);
            //     Player.AddBuff(320, 420);

            //     class7EndTimer = 420;

            //     Player.GetModPlayer<Abilities>().class7HitCounter = 0;

            //     Main.NewText("Ability activated!");
            // }
            // else
            // {
            //     Main.NewText($"{Player.GetModPlayer<Abilities>().class7HitCounter}/10 hits");
            // }

            playedSound = false;

            SoundEngine.PlaySound(SoundID.Item104.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void PsychicOnUse()
        {
            Player.AddBuff(196, 54000);
            Player.AddBuff(178, 54000);

            psychicActive = true;
            class8HP = Player.statLife;

            playedSound = false;
            SoundEngine.PlaySound(SoundID.Item113.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }
        public override bool CanUseItem(Item item)
        {
            if (psychicActive)
            {
                // Only allow if enough HP
                Player.AddBuff(21, 60);

                if (Player.statLife < class8HP) class8HP = Player.statLife;
                else if (class8HP < Player.statLife) Player.statLife = class8HP;
            }
            return base.CanUseItem(item);
        }
        public override void OnConsumeMana(Item item, int manaConsumed)
        {
            if (psychicActive && manaConsumed > 0)
            {
                int hpCost = manaConsumed;
                Player.statLife -= hpCost;
                class8HP = Player.statLife;

                if (Player.statLife <= 0)
                {
                    Player.KillMe(PlayerDeathReason.ByCustomReason(Terraria.Localization.NetworkText.FromLiteral($"{Player.name} finished ")), 9999, 0);
                    psychicActive = false;
                    class8HP = 0;
                }

                NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, Player.whoAmI); // Sync HP
            }
        }

        private void PsychicPostStatus()
        {

        }

        private void WhiteMageOnUse()
        {
            Player.AddBuff(2, 300);

            var mod = ModContent.GetInstance<CTG2>();

            foreach (Player other in Main.player)
            {
                if (!other.active || other.dead || other.whoAmI == Player.whoAmI)
                    continue;

                if (Vector2.Distance(Player.Center, other.Center) <= 25 * 16 && Player.team == other.team) // 25 block radius
                {
                    ModPacket packet1 = mod.GetPacket();
                    packet1.Write((byte)MessageType.RequestAddBuff);
                    packet1.Write(other.whoAmI);
                    packet1.Write(103);
                    packet1.Write(480);
                    packet1.Send();

                    ModPacket packet2 = mod.GetPacket();
                    packet2.Write((byte)MessageType.RequestAddBuff);
                    packet2.Write(other.whoAmI);
                    packet2.Write(206);
                    packet2.Write(480);
                    packet2.Send();

                    ModPacket packet3 = mod.GetPacket();
                    packet3.Write((byte)MessageType.RequestAddBuff);
                    packet3.Write(other.whoAmI);
                    packet3.Write(2);
                    packet3.Write(480);
                    packet3.Send();

                    ModPacket audioPacket = mod.GetPacket();
                    audioPacket.Write((byte)MessageType.RequestAudioToClient);
                    audioPacket.Write("CTG2/Content/Classes/WhiteMageHeal");
                    audioPacket.Write(other.whoAmI);
                    audioPacket.Send();
                }
            }

            ModPacket audioPacketSelf = mod.GetPacket();
            audioPacketSelf.Write((byte)MessageType.RequestAudioToClient);
            audioPacketSelf.Write("CTG2/Content/Classes/WhiteMageHeal");
            audioPacketSelf.Write(Player.whoAmI);
            audioPacketSelf.Send();

            playedSound = false;
        }


        private void MinerOnUse() //not finished
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Point playerTile = Player.Center.ToTileCoordinates();

                for (int offsetX = -13; offsetX <= 13; offsetX++)
                {
                    for (int offsetY = -5; offsetY <= 1; offsetY++)
                    {
                        int x = playerTile.X + offsetX;
                        int y = playerTile.Y + offsetY;

                        if (WorldGen.InWorld(x, y))
                        {
                            Tile tile = Main.tile[x, y];
                            if (tile.HasTile && tile.TileType == TileID.Dirt)
                            {
                                WorldGen.KillTile(x, y, false, false, true);
                                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y);
                                Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 16, 16, ItemID.DirtBlock);
                            }
                        }
                    }
                }
            }

            playedSound = false;
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact.WithVolumeScale(Main.soundVolume * 6f), Player.Center);
        }


        private void FishOnUse()
        {
            Player.AddBuff(1, 120);
            Player.AddBuff(104, 120);
            Player.AddBuff(109, 120);

            playedSound = false;
            class11EndTimer = 120;
            SoundEngine.PlaySound(SoundID.Item66, Player.Center);
        }


        private void ClownOnUse() //not finished
        {
            Player.AddBuff(BuffID.Electrified, 60);
            Player.AddBuff(BuffID.WitheredWeapon, 60);

            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAddBuff);
            packet.Write(Player.whoAmI);
            packet.Write(BuffID.Electrified);
            packet.Write(60);
            packet.Send();

            packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAddBuff);
            packet.Write(Player.whoAmI);
            packet.Write(BuffID.WitheredWeapon);
            packet.Write(60);

            foreach (Player other in Main.player)
            {
                if (!other.active || other.dead || other.whoAmI == Player.whoAmI)
                    continue;

                if (Vector2.Distance(Player.Center, other.Center) <= 22 * 16) // 22 block radius
                {
                    ModPacket audioPacket = mod.GetPacket();
                    audioPacket.Write((byte)MessageType.RequestAudioToClient);
                    audioPacket.Write("CTG2/Content/Classes/ClownSwap");
                    audioPacket.Write(other.whoAmI);
                    audioPacket.Send();
                }
            }

            ModPacket audioPacketSelf = mod.GetPacket();
            audioPacketSelf.Write((byte)MessageType.RequestAudioToClient);
            audioPacketSelf.Write("CTG2/Content/Classes/ClownSwap");
            audioPacketSelf.Write(Player.whoAmI);
            audioPacketSelf.Send();

            Player.GetModPlayer<ClassSystem>().clownSwapCaller = Player.whoAmI; //Gives this reference to clownpoststatus

            class12SwapTimer = 60;

            playedSound = false;
        }


        private void ClownPostStatus()
        {
            if (Player.GetModPlayer<ClassSystem>().clownSwapCaller != Player.whoAmI) //Run this only for the person who called it 
                return; 
                
            if (class12SwapTimer != -1){ class12SwapTimer--; }

            if (class12SwapTimer == 0)
            {
                foreach (Player other in Main.player)
                {
                    if (!other.active || other.dead || other.whoAmI == Player.whoAmI || other.ghost || other.team == 0)
                        continue;

                    if (Vector2.Distance(Player.Center, other.Center) <= 20 * 16 && Vector2.Distance(Player.Center, other.Center) < class12ClosestDist && Player.team != other.team) // 22 block radius
                    {
                        class12ClosestDist = (int)Vector2.Distance(Player.Center, other.Center);
                        class12ClosestPlayer = other;
                    }
                }

                if (class12ClosestPlayer != null)
                {
                    Vector2 tempPosition = Player.position;
                    Vector2 tempPosition2 = class12ClosestPlayer.position;

                    var mod = ModContent.GetInstance<CTG2>();

                    //class12ClosestPlayer.Teleport(tempPosition);
                    ModPacket packet1 = mod.GetPacket();
                    packet1.Write((byte)MessageType.RequestTeleport);
                    packet1.Write(Player.whoAmI);
                    packet1.Write((int)tempPosition2.X);
                    packet1.Write((int)tempPosition2.Y);
                    packet1.Send();

                    ModPacket packet2 = mod.GetPacket();
                    packet2.Write((byte)MessageType.RequestTeleport);
                    packet2.Write(class12ClosestPlayer.whoAmI);
                    packet2.Write((int)tempPosition.X);
                    packet2.Write((int)tempPosition.Y);
                    packet2.Send();

                    ModPacket packet3 = mod.GetPacket();
                    packet3.Write((byte)MessageType.RequestAddBuff);
                    packet3.Write(class12ClosestPlayer.whoAmI);
                    packet3.Write(BuffID.WaterWalking);
                    packet3.Write(180);
                    packet3.Send();

                    ModPacket packet4 = mod.GetPacket();
                    packet4.Write((byte)MessageType.RequestAddBuff);
                    packet4.Write(class12ClosestPlayer.whoAmI);
                    packet4.Write(BuffID.ObsidianSkin);
                    packet4.Write(180);
                    packet4.Send();

                    SoundEngine.PlaySound(SoundID.Item6, class12ClosestPlayer.Center);
                    SoundEngine.PlaySound(SoundID.Item6, Player.Center);

                    Main.NewText("Successfully swapped!");
                    Player.GetModPlayer<ClassSystem>().clownSwapCaller = -1; //reset the caller after the logic is done
                }
                else
                {
                    Main.NewText("Swap was unsuccessful!");
                }

                class12ClosestDist = 99999;
                class12ClosestPlayer = null;
            }

            playedSound = false;
        }



        private void PhoenixOnUse()
        {
            Player.AddBuff(176, 2 * 60);

            playedSound = false;
            class13EndTimer = 120;

            SoundEngine.PlaySound(SoundID.Item100.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void TikiPriestOnUse()
        {
            var mod1 = ModContent.GetInstance<CTG2>();
            ModPacket packet1 = mod1.GetPacket();
            packet1.Write((byte)MessageType.RequestSpawnNpc);
            packet1.Write((int)Player.Center.X);
            packet1.Write((int)Player.Center.Y);
            packet1.Write(ModContent.NPCType<TikiTotem>());
            packet1.Write(Player.team);
            packet1.Write(0f);
            packet1.Send();

            SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn.WithVolumeScale(Main.soundVolume * 2f), Player.Center);

            playedSound = false;
        }


        private void TreeOnUse() //not done
        {
            for (int i = 0; i < 7; i++)
            {
                Projectile.NewProjectile(
                    Player.GetSource_Misc("Class15Ability"),
                    Player.Center,
                    Vector2.Zero,
                    511,
                    0,
                    0f,
                    Player.whoAmI
                );
            }

            playedSound = false;

            SoundEngine.PlaySound(SoundID.NPCHit5.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void TreePostStatus()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (proj.active && proj.type == 511 && proj.owner == Player.whoAmI)
                {
                    foreach (Player other in Main.player)
                    {
                        if (!other.active || other.dead || other.whoAmI == Player.whoAmI || other.ghost || other.team == 0)
                            continue;

                        if (Player.whoAmI == other.whoAmI || Player.team == other.team) continue;
                        if (proj.Hitbox.Intersects(other.Hitbox))
                        {
                            other.AddBuff(160, 30);
                            other.AddBuff(197, 30);

                            NetMessage.SendData(MessageID.AddPlayerBuff, other.whoAmI, -1, null, other.whoAmI, 160, 30);
                            NetMessage.SendData(MessageID.AddPlayerBuff, other.whoAmI, -1, null, other.whoAmI, 197, 30);
                        }
                    }
                }
            }

            playedSound = false;
        }


        private void MutantInitialize()
        {
            using (var stream = Mod.GetFileStream($"Content/Classes/rushmutant.json"))
            using (var fileReader = new StreamReader(stream))
            {
                var jsonData = fileReader.ReadToEnd();
                try
                {
                    class16RushData = JsonSerializer.Deserialize<CtgClass>(jsonData);
                }
                catch
                {
                    Main.NewText("Failed to load or parse inventory file.", Microsoft.Xna.Framework.Color.Red);
                    return;
                }
            }
            using (var stream = Mod.GetFileStream($"Content/Classes/regenmutant.json"))
            using (var fileReader = new StreamReader(stream))
            {
                var jsonData = fileReader.ReadToEnd();
                try
                {
                    class16RegenData = JsonSerializer.Deserialize<CtgClass>(jsonData);
                }
                catch
                {
                    Main.NewText("Failed to load or parse inventory file.", Microsoft.Xna.Framework.Color.Red);
                    return;
                }
            }

        }


        private void MutantOnUse()
        {
            Player.AddBuff(149, 45);
            Player.AddBuff(21, 20 * 60);

            switch (mutantState)
            {
                case 1:
                    SetInventory(class16RegenData, false);
                    mutantState = 2;

                    break;

                case 2:
                    SetInventory(class16RushData, true);
                    mutantState = 1;

                    break;
            }

            playedSound = false;
            SoundEngine.PlaySound(SoundID.NPCHit20.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }


        private void LeechOnUse()
        {
            Player.AddBuff(320, 5 * 60);

            playedSound = false;
            class17EndTimer = 300;
            SoundEngine.PlaySound(SoundID.Item60.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
        }

        private void RngManOnUse()
        {
            int itemType = Main.rand.Next(1, ItemLoader.ItemCount);
            int amount = ContentSamples.ItemsByType[itemType].maxStack;

            int remaining = TryAddItemToInventory(Player, itemType, amount);

            if (remaining > 0)
            {
                // fallback to quickspawn for any remainder (keeps previous behavior when inventory is full)
                Player.QuickSpawnItem(null, itemType, remaining);
            }
        }


        public override void PostItemCheck() // Upon activation
        {
            if (Main.netMode == NetmodeID.Server) return;

            if (!initializedMutant)
            {
                MutantInitialize();
                initializedMutant = true;
            }

            var playerManager = Player.GetModPlayer<PlayerManager>();
            int selectedClass = playerManager.currentClass.AbilityID;

            switch (selectedClass)
            {
                case 1:
                    if (Player.dead || Player.ghost || (!playedSound && class1EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash.WithVolumeScale(Main.soundVolume * 4f), Player.Center);
                        playedSound = true;
                    }

                    break;

                case 4:
                    if (Player.dead || Player.ghost || (!playedSound && class4BuffTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BookStaffCast, Player.Center);
                        playedSound = true;
                    }

                    break;
                
                case 5:
                    if (Player.dead || Player.ghost || (!playedSound && class5EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton.WithVolumeScale(Main.soundVolume * 2f), Player.Center);
                        playedSound = true;

                        PaladinClear();
                    }

                    break;

                case 7:
                    if (Player.dead || Player.ghost || (!playedSound && class7EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton, Player.Center);
                        playedSound = true;
                    }

                    break;

                case 11:
                    if (Player.dead || Player.ghost || (!playedSound && class11EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.Item77, Player.Center);
                        playedSound = true;
                    }

                    break;

                case 13:
                    if (Player.dead || Player.ghost || (!playedSound && class13EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.Item88, Player.Center);
                        playedSound = true;
                    }

                    break;

                case 17:
                    if (Player.dead || Player.ghost || (!playedSound && class17EndTimer == 0))
                    {
                        SoundEngine.PlaySound(SoundID.Item90, Player.Center);
                        playedSound = true;
                    }

                    break;
            }

            if (cooldown == 1)
                SoundEngine.PlaySound(abilityReady.WithVolumeScale(Main.soundVolume * 2f), Player.Center);

            if (((Player.HeldItem.type == ItemID.WhoopieCushion && Player.controlUseItem && Player.itemTime == 0) || CTG2.Ability1Keybind.JustPressed) && cooldown == 0 && playerManager.playerState == PlayerManager.PlayerState.Active && !Player.HasBuff(BuffID.Webbed)) // Only activate if not on cooldown
            {
                switch (selectedClass)
                {
                    case 1:
                        SetCooldown(36);
                        ArcherOnUse();

                        break;

                    case 2:
                        SetCooldown(10);
                        NinjaOnUse();

                        break;

                    case 3:
                        SetCooldown(35);
                        BeastOnUse();

                        break;

                    case 4:
                        SetCooldown(35);
                        GladiatorOnUse();

                        break;

                    case 5:
                        SetCooldown(20);
                        PaladinOnUse();

                        break;

                    case 6:
                        SetCooldown(40);
                        TankOnUse();

                        break;

                    case 7:
                        SetCooldown(42);
                        BlackMageOnUse();

                        break;

                    case 8:
                        SetCooldown(40);
                        PsychicOnUse();

                        break;

                    case 9:
                        SetCooldown(30);
                        WhiteMageOnUse();

                        break;

                    case 10:
                        SetCooldown(15);
                        MinerOnUse();

                        break;

                    case 11:
                        SetCooldown(35);
                        FishOnUse();

                        break;

                    case 12: //not finished
                        SetCooldown(30);
                        ClownOnUse();

                        break;

                    case 13: //not finished
                        SetCooldown(15);
                        PhoenixOnUse();

                        break;

                    case 14: //not finished
                        SetCooldown(20);
                        TikiPriestOnUse();

                        break;

                    case 15: //not finished 
                        SetCooldown(27);
                        TreeOnUse();

                        break;

                    case 16:
                        SetCooldown(1);
                        MutantOnUse();

                        break;

                    case 17: //not finished
                        SetCooldown(40);
                        LeechOnUse();

                        break;

                    case 18:
                        SetCooldown(5);
                        RngManOnUse();

                        break;
                }
            }
        }

        public override void PreUpdate()
        {
            if (GameInfo.matchStage == 3)
                return; 
        }
       
        public override void PostUpdate()
        {
            BeastPostStatus();
            GladiatorPostStatus();
            PsychicPostStatus();
            ClownPostStatus();
            TreePostStatus();

            if (!GameInfo.paused)
            {
                if (cooldown > 0)
                    cooldown--;

                if (class1EndTimer >= 0)
                    class1EndTimer--;

                if (class3SpawnTimer >= 0)
                    class3SpawnTimer--;

                if (class4BuffTimer >= 0)
                    class4BuffTimer--;

                if (class5EndTimer >= 0)
                    class5EndTimer--;

                if (class7EndTimer >= 0)
                    class7EndTimer--;

                if (class11EndTimer >= 0)
                    class11EndTimer--;

                if (class13EndTimer >= 0)
                    class13EndTimer--;
                
                if (class17EndTimer >= 0)
                    class17EndTimer--;
            }
        }
        
        public override void UpdateLifeRegen()
        {
            if (Player.HasBuff(BuffID.Electrified))
            {
                if (Player.lifeRegen < 0)
                {
                    Player.lifeRegen = 0;
                }
            }
        }
    }


/*
        public class AbilitiesGlobal : ModSystem
        {
            public override void PostUpdatePlayers()
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];

                    if (player.active && player.dead)
                    {
                        if (Abilities.cooldown > 0)
                            Abilities.cooldown--;
                    }
                }
            }
        } Shouldnt be updated globally i believe*/
}
