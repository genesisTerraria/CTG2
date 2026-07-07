using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using ReLogic.Utilities;
using System;
using CTG2.Content.ClientSide;
using System.Globalization;
using CTG2.Commands;
using CTG2.Content.Projectiles;

namespace CTG2.Content.Items.ModifiedWeps
{
    public class CooldownPlayer : ModPlayer
    {
        // Storing these here ensures they are unique to the player
        // and persist across item switches.
        public long fisherLastUsedCounter;
        public long daggerfishLastUsedCounter;
        public long sharknadoLastUsedCounter;
        public long chumBallLastUsedCounter;
        public long anchorLastUsedCounter;
        public long zapinatorLastUsedCounter;
        public long hellwingLastUsedCounter;
        public long umbrellaLastUsedCounter;
        public long splitterLastUsedCounter;
        public long blowgunLastUsedCounter;
        public long orbLastUsedCounter;
    }

    public class OverloadedWeps : GlobalItem
    {
        private uint rForkDelay = 37;
        private uint rForkLastUsedCounter = 0;
        private uint nagDelay = 40;
        private uint nagLastUsedCounter = 0;

        private uint zapinatorDelay = 51;
        private uint grenadeDelay = 60;
        private uint grenadeLastUsedCounter = 0;

        private uint bananarangDelay = 52;
        private uint bananarangLastUsedCounter = 0;

        private uint maceDelay = 60;
        private uint maceLastUsedCounter = 0;

        private uint flamarangDelay = 30;
        private uint flamarangLastUsedCounter = 0;

        private uint goldenShowerDelay = 70;
        private uint goldenShowerLastUsedCounter = 0;

        private uint ghastlyglaiveDelay = 70;
        private uint ghastlyglaiveLastUsedCounter = 0;

        private uint chainKnifeDelay = 55;
        private uint chainKnifeLastUsedCounter = 0;

        private uint cursedFlamesDelay = 67;
        private uint cursedFlamesLastUsedCounter = 0;

        private uint flamelashDelay = 55;
        private uint flamelashLastUsedCounter = 0;
        private uint particleDelay = 55;
        private uint particleLastUsedCounter = 0;

        private uint blowgunDelay = 50;

        private uint orbDelay = 50;

        private uint thunderZapperDelay = 60;
        private uint thunderZapperLastUsedCounter = 0;

        private uint spectreDelay = 34;
        private uint spectreLastUsedCounter = 0;

        private uint fisherDelay = 55;

        private uint anchorDelay = 120;
        private uint sharknadoDelay = 47;
        private uint chumBallDelay = 47;
        private uint umbrellaDelay = 10 * 60;
        private uint geodeDelay = 60;
        private uint geodeLastUsedCounter = 0;

        private uint splitterDelay = 5 * 60;

        bool playedAnchorSound = true;
        bool playedUmbrellaSound = true;
        bool playedSplitterSound = true;
        bool playedOrbSound = true;

        public override bool InstancePerEntity => true;


        public override void SetDefaults(Item item)
        {
            switch (item.type)
            {
                case ItemID.WhoopieCushion:
                    item.useTime = 1;
                    item.useAnimation = 1;
                    break;
                case ItemID.GoldenShower: // Fish
                    item.useTime = 20;
                    item.useAnimation = 20;
                    item.shootSpeed = 6.8f;
                    item.damage = 35;
                    item.mana = 0;
                    item.scale = 0;
                    item.crit = 0;
                    item.UseSound = SoundID.Item109;
                    break;
                case ItemID.BlueMoon:
                    item.shoot = 969;
                    item.damage = 25;
                    break;
                case ItemID.Blowgun: // Tiki Priest
                    item.useTime = 25;
                    item.useAnimation = 25;
                    item.shoot = 267;
                    item.shootSpeed = 15f;
                    item.useAmmo = AmmoID.None;
                    item.autoReuse = false;
                    item.damage = 34;
                    item.crit = 0;
                    break;
                case ItemID.StaffofEarth: // Tiki Priest: Staff of Earth
                    item.useTime = 24;
                    item.useAnimation = 24;
                    item.mana = 0;
                    item.shootSpeed = 10f;
                    item.autoReuse = false;
                    item.damage = 28;
                    item.crit = 0;
                    item.shoot = ProjectileID.ChlorophyteOrb;
                    item.scale = 0;
                    item.UseSound = SoundID.Item8;
                    break;
                case ItemID.Bananarang: // Tree
                    item.useTime = 20;
                    item.useAnimation = 20;
                    item.shootSpeed = 12f;
                    item.knockBack = 6f;
                    item.autoReuse = false;
                    item.damage = 32;
                    item.crit = 0;
                    break;
                case ItemID.ChainKnife: // Leech
                    item.useTime = 4;
                    item.useAnimation = 4;
                    item.shootSpeed = 28;
                    item.damage = 31;
                    item.crit = 0;
                    item.DamageType = DamageClass.Magic;
                    break;
                case ItemID.WandofSparking:
                    item.damage = 8;
                    item.useTime = 8;
                    item.useAnimation = 8;
                    item.shootSpeed = 9f;
                    item.crit = 0;
                    item.mana = 18;
                    item.shoot = ProjectileID.EmeraldBolt;
                    break;
                case ItemID.Geode:
                    item.damage = 30;
                    item.useTime = 8;
                    item.useAnimation = 8;
                    item.shootSpeed = 8f;
                    item.crit = 0;
                    item.consumable = false;
                    break;
                case ItemID.CursedFlames: // Leech
                    item.useTime = 37;
                    item.useAnimation = 37;
                    item.shootSpeed = 10;
                    item.damage = 24;
                    item.mana = 0;
                    item.scale = 0;
                    item.crit = 0;
                    item.shoot = ProjectileID.VampireKnife;
                    item.DamageType = DamageClass.Melee;
                    break;
                case ItemID.Mace: // Tank
                    item.useTime = 19;
                    item.useAnimation = 19;
                    item.damage = 17;
                    item.crit = 0;
                    break;
                case ItemID.NebulaArcanum: //psy
                    item.damage = 56;
                    item.shootSpeed = 7;
                    break;
                case ItemID.AmethystStaff:
                    item.damage = 37;
                    item.useTime = 22;
                    item.useAnimation = 22;
                    item.crit = 0;
                    item.shootSpeed = 11f;
                    item.mana = 10;
                    item.shoot = 33;
                    item.scale = 0;
                    item.UseSound = null;
                    break;
                case ItemID.StickyGrenade: //miner //maybe ovveride projectile to not do self damage or at least not self knockback
                    item.damage = 40;
                    item.useTime = 21;
                    item.useAnimation = 21;
                    item.crit = 0;
                    item.consumable = false;
                    break;

                case 1446: //spectre staff
                    item.shoot = 126;
                    item.shootSpeed = 13f;
                    item.scale = 0.86f;
                    item.damage = 28;
                    item.useTime = 17;
                    item.useAnimation = 17;
                    item.crit = 0;
                    break;

                case 726: //ice sickle (ovveride projectile to last longere later)
                    item.shoot = 263;
                    item.shootSpeed = 4f;
                    item.damage = 30;
                    item.useTime = 28;
                    item.useAnimation = 28;
                    item.scale = 0f;
                    item.crit = 0;
                    item.mana = 17;
                    break;

                case ItemID.IceRod:
                    item.damage = 14;
                    break;

                //psychic projectile still needs to be ovverided
                case 272: //bmage wep (demon scythe)
                    item.damage = 31;
                    item.useAnimation = 18;
                    item.useTime = 18;
                    item.crit = 0;
                    item.mana = 26;
                    break;
                case 802: //rotted fork
                    item.damage = 32;
                    item.useAnimation = 37;
                    item.useTime = 37;
                    item.crit = 0;
                    break;
                case 537: // cobalt naginata
                    item.damage = 36;
                    item.useAnimation = 22;
                    item.useTime = 22;
                    item.shootSpeed = 4.2f;
                    item.crit = 0;
                    break;
                case ItemID.Gungnir:
                    item.damage = 22;
                    item.useAnimation = 22;
                    item.useTime = 22;
                    item.crit = 0;
                    item.shootSpeed = 14f;
                    item.mana = 25;
                    item.knockBack = 10;
                    item.shoot = 507;
                    item.UseSound = SoundID.DD2_DarkMageAttack;
                    break;
                case 4347: //gray zapinator
                    item.damage = 35;
                    item.useAnimation = 14;
                    item.useTime = 14;
                    item.knockBack = 10;
                    item.shoot = 285;
                    item.shootSpeed = 10f;
                    item.mana = 0;
                    item.crit = 0;
                    break;

                case ItemID.Flamelash: // phoenix fireball
                    item.crit = 0;
                    item.damage = 31;
                    item.shootSpeed = 13f;
                    item.shoot = ProjectileID.DD2FlameBurstTowerT2Shot;
                    item.scale = 0;
                    item.mana = 0;
                    item.useTime = 24;
                    item.useAnimation = 24;
                    break;

                case ItemID.MonkStaffT1: // phoenix phantom
                    item.crit = 0;
                    item.damage = 36;
                    item.shootSpeed = 5f;
                    item.shoot = ProjectileID.DD2PhoenixBowShot;
                    item.mana = 11;
                    item.scale = 0;
                    item.useTime = 24;
                    item.useAnimation = 24;
                    break;

                case 3543: // phoenix aerial bane
                    item.crit = 0;
                    item.damage = 33;
                    item.shootSpeed = 9f;
                    item.shoot = ProjectileID.DD2BetsyArrow;
                    item.mana = 11;
                    item.scale = 0;
                    item.useTime = 24;
                    item.useAnimation = 24;
                    break;
                case 4760: //Pala shield
                    item.shoot = 0;
                    item.scale = 0;
                    item.damage = 0;
                    item.useTime =1;
                    item.useAnimation=1;
                    break;
                case 4707: // Tragic Umbrella
                    item.damage = 10;
                    break;
                case ItemID.WaterBolt: //paladin weapon
                    item.shoot = ProjectileID.MonkStaffT2;
                    item.scale = 0;
                    item.damage = 34;
                    item.useTime = 18;
                    item.useAnimation = 18;
                    item.shootSpeed = 58f;
                    item.crit = 0;
                    item.mana = 11;
                    break;
                case ItemID.MonkStaffT2: //ghastly glaive
                    item.damage = 27;
                    item.useTime = 14;
                    item.useAnimation = 14;
                    item.shootSpeed = 46f;
                    item.crit = 0;
                    break;
                case ItemID.ThunderStaff: // psychic charge wep
                    item.shoot = ProjectileID.ThunderSpearShot;
                    item.scale = 0;
                    item.damage = 15;
                    item.useTime = 14;
                    item.useAnimation = 14;
                    item.shootSpeed = 20;
                    item.crit = 0;
                    item.mana = 0;
                    break;
            }
        }


        public override bool Shoot(Item item, Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ItemID.NebulaArcanum)
            {
                Vector2 finalVelocity = velocity;

                Projectile.NewProjectile(source, position, velocity, ProjectileID.NebulaArcanum, damage, knockback, player.whoAmI);
                return false;
            }
            else if (item.type == ItemID.Snowball)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath19, player.Center);
            }

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }


        public override bool CanUseItem(Item item, Player player)
        {
            var mPlayer = player.GetModPlayer<CooldownPlayer>();

            if (item.type == ItemID.Mace)
            {
                if (Main.GameUpdateCount - maceLastUsedCounter >= maceDelay)
                {
                    maceLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<KingFisher>())
            {
                if (Main.GameUpdateCount - mPlayer.fisherLastUsedCounter >= fisherDelay)
                {
                    mPlayer.anchorLastUsedCounter = Math.Max(Main.GameUpdateCount - 3 * 60 + 30, mPlayer.anchorLastUsedCounter);
                    mPlayer.fisherLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.daggerfishLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.sharknadoLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.chumBallLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                return false;
            }
            else if (item.type == ModContent.ItemType<Sharkron>())
            {
                if (Main.GameUpdateCount - mPlayer.sharknadoLastUsedCounter >= sharknadoDelay)
                {
                    mPlayer.fisherLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.daggerfishLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.sharknadoLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.chumBallLastUsedCounter = Main.GameUpdateCount;
                    return true;
                }
                return false;
            }
            else if (item.type == ModContent.ItemType<SeaUrchin>())
            {
                if (Main.GameUpdateCount - mPlayer.chumBallLastUsedCounter >= chumBallDelay)
                {
                    mPlayer.fisherLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.daggerfishLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.sharknadoLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.chumBallLastUsedCounter = Main.GameUpdateCount;
                    return true;
                }
                return false;
            }
            else if (item.type == ModContent.ItemType<FishermansAnchor>())
            {
                if (Main.GameUpdateCount - mPlayer.anchorLastUsedCounter >= anchorDelay)
                {
                    mPlayer.fisherLastUsedCounter = Math.Max(Main.GameUpdateCount - 55 + 40, mPlayer.fisherLastUsedCounter);
                    mPlayer.anchorLastUsedCounter = Main.GameUpdateCount;

                    playedAnchorSound = false;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.Bananarang)
            {
                if (Main.GameUpdateCount - bananarangLastUsedCounter >= bananarangDelay)
                {
                    bananarangLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.Flamarang)
            {
                if (Main.GameUpdateCount - flamarangLastUsedCounter >= flamarangDelay)
                {
                    flamarangLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.TheRottedFork)
            {
                if (Main.GameUpdateCount - rForkLastUsedCounter >= rForkDelay)
                {
                    rForkLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == 4062)
            {
                if (Main.GameUpdateCount - thunderZapperLastUsedCounter >= thunderZapperDelay)
                {
                    thunderZapperLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == 4347)
            {
                if (Main.GameUpdateCount - mPlayer.zapinatorLastUsedCounter >= zapinatorDelay)
                {
                    mPlayer.zapinatorLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.hellwingLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<HellwingTome>())
            {
                if (Main.GameUpdateCount - mPlayer.hellwingLastUsedCounter >= zapinatorDelay)
                {
                    mPlayer.zapinatorLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.hellwingLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.MonkStaffT2) //ghastly glaive
            {
                if (Main.GameUpdateCount - ghastlyglaiveLastUsedCounter >= ghastlyglaiveDelay)
                {
                    ghastlyglaiveLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.CobaltNaginata || item.type == ModContent.ItemType<GladiatorialPolearm>())
            {
                if (Main.GameUpdateCount - nagLastUsedCounter >= nagDelay)
                {
                    nagLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.StickyGrenade)
            {
                if (Main.GameUpdateCount - grenadeLastUsedCounter >= grenadeDelay)
                {
                    grenadeLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.GoldenShower)
            {
                if (Main.GameUpdateCount - goldenShowerLastUsedCounter >= goldenShowerDelay)
                {
                    goldenShowerLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.ChainKnife)
            {
                if (Main.GameUpdateCount - chainKnifeLastUsedCounter >= chainKnifeDelay)
                {
                    chainKnifeLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<StaffOfWinter>())
            {
                if (Main.GameUpdateCount - spectreLastUsedCounter >= spectreDelay)
                {
                    spectreLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.Blowgun)
            {
                if (Main.GameUpdateCount - mPlayer.blowgunLastUsedCounter >= blowgunDelay)
                {
                    mPlayer.blowgunLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.orbLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == 1296)
            {
                if (Main.GameUpdateCount - mPlayer.orbLastUsedCounter >= orbDelay)
                {
                    mPlayer.blowgunLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.orbLastUsedCounter = Main.GameUpdateCount;

                    playedOrbSound = false;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.Flamelash)
            {
                if (Main.GameUpdateCount - flamelashLastUsedCounter >= flamelashDelay)
                {
                    flamelashLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.CursedFlames)
            {
                if (Main.GameUpdateCount - cursedFlamesLastUsedCounter >= cursedFlamesDelay)
                {
                    cursedFlamesLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.TragicUmbrella)
            {
                if (Main.GameUpdateCount - mPlayer.umbrellaLastUsedCounter >= umbrellaDelay && !player.HasBuff(BuffID.WitheredArmor))
                {
                    mPlayer.umbrellaLastUsedCounter = Main.GameUpdateCount;

                    playedUmbrellaSound = false;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<MagicGeode>())
            {
                if (Main.GameUpdateCount - geodeLastUsedCounter >= geodeDelay)
                {
                    geodeLastUsedCounter = Main.GameUpdateCount;
                    mPlayer.splitterLastUsedCounter = Math.Max(Main.GameUpdateCount - 5 * 60 + 25, mPlayer.splitterLastUsedCounter);

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<ParticleCarbine>())
            {
                if (Main.GameUpdateCount - particleLastUsedCounter >= particleDelay)
                {
                    particleLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ModContent.ItemType<SpaceSplitter>())
            {
                if (Main.GameUpdateCount - mPlayer.splitterLastUsedCounter >= splitterDelay)
                {
                    mPlayer.splitterLastUsedCounter = Main.GameUpdateCount;

                    playedSplitterSound = false;

                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }


        public override void UpdateEquip(Item item, Player player)
        {
            switch (item.type)
            {
                case ItemID.ManaRegenerationBand:
                    player.statManaMax2 -= 20;
                    break;
                case ItemID.JungleHat:
                    player.statManaMax2 -= 40;
                    break;
                case ItemID.JungleShirt:
                    player.statManaMax2 -= 20;
                    break;
                case ItemID.JunglePants:
                    player.statManaMax2 -= 20;
                    break;
                case ItemID.TopazRobe:
                    player.statManaMax2 -= 40;
                    break;
                case ItemID.RubyRobe:
                    player.statManaMax2 -= 60;
                    break;
                case ItemID.ChlorophyteHeadgear:
                    player.statManaMax2 -= 80;
                    player.statDefense -= 4;
                    break;
                case ItemID.RainCoat: // Rain Coat
                    player.statDefense -= 2;
                    break;
                case ItemID.ShadowScalemail: // Gladiator
                    player.statDefense -= 1;
                    break;
                case ItemID.ShadowGreaves: // Gladiator
                    player.statDefense -= 6;
                    break;
                case ItemID.MoltenBreastplate: // Black Mage
                    player.statDefense -= 3;
                    break;
                case ItemID.TitaniumHelmet: // White Mage
                    player.statDefense -= 2;
                    break;
                case ItemID.GypsyRobe: // White Mage
                    player.statDefense -= 2;
                    break;
                case ItemID.CrimsonGreaves: // Mutant
                    player.statDefense -= 6;
                    break;
                case ItemID.CrimsonScalemail: // Mutant
                    player.statDefense -= 5;
                    break;
                case ItemID.Blindfold:
                    player.statDefense -= 100;
                    player.GetDamage(DamageClass.Generic) -= 0.99f;
                    break;
            }
        }

        public override void HoldItem(Item item, Player player)
        {
            if (item.type == 4760 && Main.mouseRight)
            {
                player.statDefense += 4;
            }
        }

        public override void UpdateInventory(Item item, Player player)
        {
            var mPlayer = player.GetModPlayer<CooldownPlayer>();

            if (Main.GameUpdateCount - mPlayer.anchorLastUsedCounter >= anchorDelay && !playedAnchorSound)
            {
                SoundEngine.PlaySound(SoundID.MaxMana.WithVolumeScale(Main.soundVolume * 2f), player.Center);
                playedAnchorSound = true;
            }

            if (Main.GameUpdateCount - mPlayer.umbrellaLastUsedCounter >= umbrellaDelay && !playedUmbrellaSound)
            {
                SoundEngine.PlaySound(SoundID.Item105.WithVolumeScale(Main.soundVolume * 2f), player.Center);
                playedUmbrellaSound = true;
            }

            if (Main.GameUpdateCount - mPlayer.splitterLastUsedCounter >= splitterDelay && !playedSplitterSound)
            {
                SoundEngine.PlaySound(SoundID.MaxMana.WithVolumeScale(Main.soundVolume * 2f), player.Center);
                playedSplitterSound = true;
            }

            if (Main.GameUpdateCount - mPlayer.orbLastUsedCounter >= orbDelay && !playedOrbSound)
            {
                SoundEngine.PlaySound(SoundID.MaxMana.WithVolumeScale(Main.soundVolume * 2f), player.Center);
                playedOrbSound = true;
            }
        }
    }
}
