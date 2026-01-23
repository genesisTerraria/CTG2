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

namespace CTG2.Content.Items.ModifiedWeps
{
    public class OverloadedWeps : GlobalItem
    {

        private uint rforkDelay = 40;
        private uint rforkLastUsedCounter = 0;

        private uint zapinatorDelay = 51; //lowkey i think this is off rn but i cant tell 
        private uint zapinatorLastUsedCounter = 0;
        private uint grenadeDelay = 50;
        private uint grenadeLastUsedCounter = 0;
        private uint bananarangDelay = 55;
        private uint bananarangLastUsedCounter = 0;

        private uint ThornChakramDelay = 30;
        private uint ThornChakramLastUsedCounter = 0;

        private uint flamarangDelay = 30;
        private uint flamarangLastUsedCounter = 0;

        private uint blowgunDelay = 40;
        private uint blowgunLastUsedCounter = 0;

        private uint goldenShowerDelay = 58;
        private uint goldenShowerLastUsedCounter = 0;

        private uint ghastlyglaiveDelay = 45;
        private uint ghastlyglaiveLastUsedCounter = 0;

        private uint chainKnifeDelay = 55;
        private uint chainKnifeLastUsedCounter = 0;

        private uint cursedFlamesDelay = 65;
        private uint cursedFlamesLastUsedCounter = 0;

        private uint sickleDelay = 147; //There should only be one ice sickle projectile on the field at once
        private uint sickleLastUsedCounter = 0;

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
                    item.useTime = 22;
                    item.useAnimation = 22;
                    item.shootSpeed = 6.37f;
                    item.damage = 35;
                    item.mana = 0;
                    item.scale = 0;
                    item.crit = 0;
                    item.UseSound = SoundID.Item109;
                    break;
                case ItemID.Blowgun: // Tiki Priest
                    item.useTime = 18;
                    item.useAnimation = 18;
                    item.shootSpeed = 13f;
                    item.autoReuse = false;
                    item.damage = 26;
                    item.crit = 0;
                    break;
                case ItemID.Bananarang: // Tree
                    item.useTime = 20;
                    item.useAnimation = 20;
                    item.shootSpeed = 11f;
                    item.knockBack = 6f;
                    item.autoReuse = false;
                    item.damage = 35;
                    item.crit = 0;
                    break;
                case ItemID.ChainKnife: // Leech
                    item.useTime = 4;
                    item.useAnimation = 4;
                    item.shootSpeed = 15;
                    item.damage = 31;
                    item.crit = 0;
                    break;
                case ItemID.CursedFlames: // Leech
                    item.useTime = 35;
                    item.useAnimation = 35;
                    item.shootSpeed = 15;
                    item.damage = 23;
                    item.mana = 0;
                    item.scale = 0;
                    item.crit = 0;
                    item.shoot = ProjectileID.VampireKnife;
                    break;

                case ItemID.ThornChakram:
                    item.damage = 37;
                    item.useTime = 14;
                    item.useAnimation = 14;
                    item.crit = 0;
                    item.shootSpeed = 13f;
                    break; //change proj later

                case 2586: //miner //maybe ovveride projectile to not do self damage or at least not self knockback
                    item.damage = 40;
                    item.useTime = 21;
                    item.useAnimation = 20;
                    item.crit = 0;
                    break;

                case 1446: //spectre staf
                    item.shoot = 126;
                    item.shootSpeed = 13f;
                    item.scale = 0.86f;
                    item.damage = 28;
                    item.useTime = 17;
                    item.useAnimation = 17;
                    item.crit = 0;
                    break;

                case 1306: //ice sickle (ovveride projectile to last longere later)
                    item.shoot = 263;
                    item.shootSpeed = 4f;
                    item.damage = 37;
                    item.useTime = 28;
                    item.useAnimation = 28;
                    item.scale = 0f;
                    item.crit = 0;
                    item.mana = 17;
                    break;

                case ItemID.IceRod:
                    item.damage = 13;
                    break;

                //psychic projectile still needs to be ovverided
                case 272: //bmage wep
                    item.damage = 38;
                    item.shoot = 496;
                    item.useAnimation = 18;
                    item.useTime = 18;
                    item.shootSpeed = 4f;
                    item.crit = 0;
                    break;

                case 802: //rotted fork
                    item.damage = 30;
                    item.useAnimation = 22;
                    item.useTime = 22;
                    item.shootSpeed = 3.6f;
                    item.crit = 0;
                    break;

                case 4347: //gray zapinator
                    item.damage = 35;
                    item.useAnimation = 18;
                    item.useTime = 18;
                    item.knockBack = 10;
                    item.shoot = 285;
                    item.shootSpeed = 10f;
                    item.mana = 0;
                    item.crit = 0;
                    break;

                //all done except jman

                case 742: //jman emerald staff
                    item.crit = 0;
                    item.shoot = 480;
                    item.damage = 34;
                    item.shootSpeed = 8f;
                    item.scale = 0;
                    item.useTime = 33;
                    item.useAnimation = 33;
                    break;

                //drone damage 31
                case 5451: //jman drone
                    item.crit = 0;
                    item.shoot = 513;
                    item.damage = 0;
                    item.shootSpeed = 2f;
                    item.useAnimation = 37;
                    item.useTime = 37;
                    item.knockBack = 4;
                    item.scale = 0;
                    break;

                case ItemID.Flamarang:
                    item.damage = 33;
                    item.crit = 0;
                    break;

                case ItemID.FlowerofFire:
                    item.damage = 33;
                    item.crit = 0;
                    break;

                case 3836: //ghastly glaive
                    item.damage = 35;
                    break;

                case 220:
                    item.damage = 17;
                    break;
                case 4760: //Pala shield
                    item.shoot = 0;
                    item.scale = 0;
                    item.damage = 0;
                    item.useTime =1;
                    item.useAnimation=1;
                    break;
                case 165:
                    item.shoot = 699;
                    item.scale = 0;
                    item.damage = 34;
                    item.useTime = 20;
                    item.useAnimation = 20;
                    item.shootSpeed = 48f;
                    item.crit = 0;
                    break;
            }
        }


        public override bool Shoot(Item item, Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.GetModPlayer<TestPlayer>().playerAttribute && player.GetModPlayer<TestPlayer>().TryGetAimedVelocity(player, position, velocity, out Vector2 aimedVelocity))
            {
                return false;
            }
            if (item.type == ItemID.NebulaArcanum)
            {
                Vector2 finalVelocity = velocity;

                Projectile.NewProjectile(source, position, velocity, ProjectileID.NebulaArcanum, damage, knockback, player.whoAmI);
                return false;
            }

            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }


        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemID.Bananarang)
            {
                if (Main.GameUpdateCount - bananarangLastUsedCounter >= bananarangDelay)
                {
                    bananarangLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == ItemID.ThornChakram)
            {
                if (Main.GameUpdateCount - ThornChakramLastUsedCounter >= ThornChakramDelay)
                {
                    ThornChakramLastUsedCounter = Main.GameUpdateCount;

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
            else if (item.type == 4347)
            {
                if (Main.GameUpdateCount - zapinatorLastUsedCounter >= zapinatorDelay)
                {
                    zapinatorLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }
            else if (item.type == 3836) //ghastly glaive
            {
                if (Main.GameUpdateCount - ghastlyglaiveLastUsedCounter >= ghastlyglaiveDelay)
                {
                    ghastlyglaiveLastUsedCounter = Main.GameUpdateCount;

                    return true;
                }
                else
                    return false;
            }

            else if (item.type == 802) //rottedfork
            {
                if (Main.GameUpdateCount - rforkLastUsedCounter >= rforkDelay)
                {
                    rforkLastUsedCounter = Main.GameUpdateCount;

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
            else if (item.type == ItemID.Blowgun)
            {
                if (Main.GameUpdateCount - blowgunLastUsedCounter >= blowgunDelay)
                {
                    blowgunLastUsedCounter = Main.GameUpdateCount;

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
                case ItemID.ChlorophyteHeadgear:
                    player.statManaMax2 -= 80;
                    break;
            }
        }
    }
}
