using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using CTG2.Content.Items;
using Terraria.Audio;
using CTG2.Content.ClientSide;
using CTG2.Content.Buffs;
using Microsoft.Xna.Framework;
using CTG2;
using CTG2.Content.Projectiles;
using Microsoft.Extensions.Options;
using CTG2.Content;


public class ProjectileOverrides : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private bool playedSoundBoomerangs = false;

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        if (projectile.type == ProjectileID.NebulaArcanum || projectile.type == 706 || projectile.type == 711 || projectile.type == 666)
        {
            projectile.damage = 0;

            return false;
        }

        if (projectile.type == ProjectileID.LovePotion)
        {
            foreach (Player player in Main.player)
            {
                if (player.active && Vector2.Distance(projectile.Center, player.Center) <= 4 * 16 && player.team == Main.player[projectile.owner].team)
                {
                    var mod = ModContent.GetInstance<CTG2.CTG2>();
                    ModPacket buffPacket = mod.GetPacket();
                    buffPacket.Write((byte)MessageType.RequestAddBuff);
                    buffPacket.Write(player.whoAmI);
                    buffPacket.Write(ModContent.BuffType<Restoration>());
                    buffPacket.Write(4 * 60);
                    buffPacket.Send();
                }
            }
        }

        if (projectile.type == ProjectileID.FoulPotion)
        {
            foreach (Player player in Main.player)
            {
                if (player.active && Vector2.Distance(projectile.Center, player.Center) <= 4 * 16 && player.team == Main.player[projectile.owner].team)
                {
                    var mod = ModContent.GetInstance<CTG2.CTG2>();
                    ModPacket buffPacket = mod.GetPacket();
                    buffPacket.Write((byte)MessageType.RequestAddBuff);
                    buffPacket.Write(player.whoAmI);
                    buffPacket.Write(ModContent.BuffType<Resiliance>());
                    buffPacket.Write(7 * 60);
                    buffPacket.Send();
                }
            }
        }

        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (projectile.type == ProjectileID.EmeraldBolt || projectile.type == ModContent.ProjectileType<SpaceSplitterProjectile>()
         || projectile.type == ModContent.ProjectileType<SittingDuckBobber>() || projectile.type == ProjectileID.ThunderSpearShot
         || projectile.type == ProjectileID.Electrosphere || projectile.type == ProjectileID.ElectrosphereMissile)
        {
            target.noKnockback = true;

            var mod = ModContent.GetInstance<CTG2.CTG2>();
            ModPacket resultPacket = mod.GetPacket();
            resultPacket.Write((byte)MessageType.SetNoKnockback);
            resultPacket.Write(target.whoAmI);
            resultPacket.Send();
        }
    }


    public override void AI(Projectile projectile)
    {
        Player player = Main.LocalPlayer;
        // if (projectile.type == 280)
        // {
        //     float scale = 0.5f;

        //     Vector2 center = projectile.Center;

        //     projectile.scale = scale;

        //     projectile.width = (int)(projectile.width * scale);
        //     projectile.height = (int)(projectile.height * scale);

        //     projectile.Center = center;
        // }
        // if (projectile.type == 706)
        // {
        //     float scale = 0.75f;

        //     Vector2 center = projectile.Center;

        //     projectile.scale = scale;

        //     projectile.width = (int)(projectile.width * scale);
        //     projectile.height = (int)(projectile.height * scale);

        //     projectile.Center = center;
        // }
        if (projectile.type == ProjectileID.Electrosphere)
        {
            if (projectile.timeLeft > 240)
            {
                projectile.timeLeft = 240;
            }
        }
        if (projectile.type == ProjectileID.Hellwing)
        {
            if (projectile.timeLeft > 300)
            {
                projectile.timeLeft = 300;
            }
        }
        if (!playedSoundBoomerangs && (projectile.type == ProjectileID.ThornChakram || projectile.type == ProjectileID.Flamarang))
        {
            SoundEngine.PlaySound(SoundID.Item1, projectile.Center);
            playedSoundBoomerangs = true;
        }
        if (projectile.type == 167 || projectile.type == 169)
            projectile.timeLeft = 0;
        if (projectile.type == ProjectileID.ToxicCloud)
        {
            // Force a new timeLeft value (e.g., 120 ticks = 2 seconds)
            if (projectile.timeLeft > 180)
            {
                projectile.timeLeft = 180;
            }
        }
        if (projectile.type == ProjectileID.SporeCloud)
        {
            // Force a new timeLeft value (e.g., 120 ticks = 2 seconds)
            if (projectile.timeLeft > 180)
            {
                projectile.timeLeft = 180;
            }

            projectile.penetrate = 1;
        }
        if (projectile.type == ProjectileID.ApprenticeStaffT3Shot)
        {
            // Force a new timeLeft value (e.g., 120 ticks = 2 seconds)
            projectile.penetrate = 5;
        }
        if (projectile.type == ProjectileID.NebulaArcanum)
        {
            if (projectile.timeLeft > 5 * 60)
            {
                projectile.timeLeft = 5 * 60;
            }
        }
        if (projectile.type == ProjectileID.LaserMachinegunLaser)
        {
            if (projectile.timeLeft > 65)
            {
                projectile.timeLeft = 65;
            }
        }
        if (projectile.type == 41)
        {
            projectile.extraUpdates = 1;
        }
        if (projectile.type == 1006) // shimmer arrow
        {
            // Only apply if charge < 30
            if (projectile.localAI[0] < 30f)
            {
                projectile.extraUpdates = 1;
            }
        }
        if (projectile.type == ProjectileID.ThornChakram)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile barrier = Main.projectile[i];

                if (barrier.type == ModContent.ProjectileType<HexagonalBarrierProjectile>())
                {
                    HexagonalBarrierProjectile barrierProj = barrier.ModProjectile as HexagonalBarrierProjectile;
                    if (projectile.Hitbox.Intersects(barrier.Hitbox) && barrierProj.alive && barrierProj.teamCheck)
                    {
                        barrierProj.alive = false;
                        if (projectile.ai[1] > 40)
                        {
                            projectile.ai[0] = 1;
                            projectile.netUpdate = true;
                        }
                        else
                        {
                            projectile.velocity.X *= -1f;
                            projectile.velocity.Y *= -1f;
                        }

                        // Optional: play a sound or spawn dust for feedback
                        SoundEngine.PlaySound(SoundID.Dig, projectile.Center);
                        break;
                    }
                }
            }
        }
        if (projectile.type == ProjectileID.Flamarang || projectile.type == ProjectileID.Bananarang) 
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile barrier = Main.projectile[i];

                if (barrier.type == ModContent.ProjectileType<HexagonalBarrierProjectile>())
                {
                    HexagonalBarrierProjectile barrierProj = barrier.ModProjectile as HexagonalBarrierProjectile;
                    if (projectile.Hitbox.Intersects(barrier.Hitbox) && barrierProj.alive && barrierProj.teamCheck)
                    {
                        barrierProj.alive = false;
                        projectile.ai[0] = 1;
                        projectile.netUpdate = true;
                        projectile.velocity.X *= -0.8f;
                        projectile.velocity.Y *= -0.8f;

                        // Optional: play a sound or spawn dust for feedback
                        SoundEngine.PlaySound(SoundID.Dig, projectile.Center);
                        break;
                    }
                }
            }
        }
        if (projectile.type == ProjectileID.NebulaBlaze2)
        {
            Player owner = Main.player[projectile.owner];
            foreach (Player play in Main.player)
            {
                if (play.active && !play.dead && !play.ghost && play.team != 0 && projectile.Hitbox.Intersects(play.Hitbox))
                {
                    projectile.Kill();

                    if (owner.team == play.team)
                    {
                        var mod = ModContent.GetInstance<CTG2.CTG2>();
                        ModPacket packetBuff = mod.GetPacket();
                        packetBuff.Write((byte)MessageType.RequestAddBuff);
                        packetBuff.Write(play.whoAmI);
                        packetBuff.Write(BuffID.NebulaUpDmg1);
                        packetBuff.Write(3 * 60);
                        packetBuff.Send();
                    }

                    break;
                }
            }
        }
        if (projectile.type == 700) //kill ghast projectiles
        {
            projectile.Kill();
        }
        if (projectile.type == ProjectileID.NebulaArcanumExplosionShot) //Might have to ovveride the subshot shards as well
        {
            projectile.damage = 0; //make explosion 0 damage
            projectile.scale = 0;
            projectile.Kill();
        }
        if (projectile.type == ProjectileID.NebulaArcanumSubshot)
        {
            projectile.damage = 0;
            projectile.scale = 0;
            projectile.Kill();
        }
        if (projectile.type == ProjectileID.NebulaArcanumExplosionShotShard)
        {
            projectile.damage = 0;
            projectile.scale = 0;
            projectile.Kill();
        } //If nebula code doesnt work we have to kill the projectiles onspawn
        
    }


    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (projectile.type == 700) // Ghast
        {
            projectile.scale = 0f;
            projectile.damage = 0;
            projectile.alpha = 255;
            projectile.tileCollide = false;
            projectile.timeLeft = 1; // dies almost instantly
        }
        if (projectile.type == ProjectileID.NebulaArcanumExplosionShot)
        {
            projectile.damage = 0; //second check in case first fails for nebula epxlosion
            projectile.knockBack = 0f;
            projectile.scale = 0.1f;
        }
        if (projectile.type == 513)
        {
            projectile.damage = 31;
        }
        if (projectile.type == ProjectileID.IceSickle || projectile.type == ProjectileID.ChlorophyteOrb
         || projectile.type == ProjectileID.DemonScythe)
        {
            projectile.penetrate = 1;
        }
        
    //     if (projectile.type != ProjectileID.ToxicCloud &&
    //         projectile.type != ProjectileID.ToxicCloud2 &&
    //         projectile.type != ProjectileID.ToxicCloud3)
    //         return;

    //     // Only kill clouds that came from the ToxicFlask projectile
    //    if (source is EntitySource_Parent parentSource &&
    //     parentSource.Entity is Projectile parent &&
    //     parent.type == ProjectileID.ToxicFlask)
    //     {
    //         projectile.active = false;
    //         projectile.timeLeft = 0;
    //         NetMessage.SendData(MessageID.KillProjectile, -1, -1, null, projectile.whoAmI);
    //     }
    }   
}


public class ModifyHurtModPlayer : ModPlayer
{
    public override bool CanHitPvp(Item item, Player target)
    {
        if (target.ghost)
            return false;

        return base.CanHitPvp(item, target);
    }


    public override bool CanHitPvpWithProj(Projectile proj, Player target)
    {
        if (target.ghost)
            return false;

        return base.CanHitPvpWithProj(proj, target);
    }

    [System.Obsolete]
    public override void OnHurt(Player.HurtInfo info)
    {
        var modPlayer = Player.GetModPlayer<PlayerManager>();
        int attackerIndex = info.DamageSource.SourcePlayerIndex;
        int projIndex = info.DamageSource.SourceProjectileLocalIndex;

        if (projIndex >= 0 && projIndex < Main.maxProjectiles)
        {
            Projectile proj = Main.projectile[projIndex];

            if (Main.player[attackerIndex].HeldItem.type != ModContent.ItemType<UpgradedShardstonePickaxe>()
             && Main.player[attackerIndex].HeldItem.type != ModContent.ItemType<ShardstonePickaxe>())
            {
                foreach (Projectile proje in Main.projectile)
                {
                    if (proje.active && proje.owner == Player.whoAmI && proje.type == ModContent.ProjectileType<FoliageTendrilsProjectile>())
                    {
                        var tendril = proje.ModProjectile as FoliageTendrilsProjectile;
                        if (tendril != null && tendril.latched)
                        {
                            proje.Kill();
                        }
                    }
                }
            }
        }

        bool isPickaxe = info.DamageSource.SourceItem != null && (
            info.DamageSource.SourceItem.type == ModContent.ItemType<ShardstonePickaxe>() ||
            info.DamageSource.SourceItem.type == ModContent.ItemType<UpgradedShardstonePickaxe>()
        );

        // if (Player.HasBuff(ModContent.BuffType<Kenophobia>()) && Player.HasBuff(ModContent.BuffType<Stygiophobia>()) && !isPickaxe && Player.statLife > 0)
        // {
        //     int initialHealth = Player.statLife;

        //     Player.statLife -= 3;

        //     CombatText.NewText(Player.getRect(), Color.Red, 3);

        //     if (initialHealth <= 3)
        //     {
        //         PlayerDeathReason reason = PlayerDeathReason.ByCustomReason("Archer passive ability extra damage");
        //         reason.SourceItem = info.DamageSource.SourceItem;
        //         reason.SourcePlayerIndex = info.DamageSource.SourcePlayerIndex;
        //         reason.SourceProjectileType = info.DamageSource.SourceProjectileType;

        //         Player.KillMe(reason, 3, 0);
        //     }
        // }

        // Ninja section
        if (info.DamageSource.SourceProjectileType == ModContent.ProjectileType<ThrowingStarsProjectile>() && Player.whoAmI == Main.myPlayer)
        {
            Player attacker = Main.player[attackerIndex];
            attacker.AddBuff(ModContent.BuffType<Hypervision>(), 60);

            ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
            packet.Write((byte)CTG2.MessageType.RequestAddBuff);
            packet.Write(attacker.whoAmI);
            packet.Write(ModContent.BuffType<Hypervision>());
            packet.Write(60);
            packet.Send();

            ModPacket packetMana = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
            packetMana.Write((byte)CTG2.MessageType.RequestMana);
            packetMana.Write(attacker.whoAmI);
            packetMana.Write(7);
            packetMana.Send();
        }

        // Alchemist section
        if (info.DamageSource.SourceProjectileType == ModContent.ProjectileType<SpaceSplitterProjectile>())
        {
            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                Projectile hurtProjectile = Main.projectile[projIndex];

                Player owner = Main.player[info.DamageSource.SourcePlayerIndex];
                Vector2 dest = Player.position;

                var mod = ModContent.GetInstance<CTG2.CTG2>();
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)MessageType.RequestTeleport);
                packet.Write(owner.whoAmI);
                packet.Write((int)dest.X);
                packet.Write((int)dest.Y);
                packet.Send();

                ModPacket packet1 = mod.GetPacket();
                packet1.Write((byte)MessageType.RequestAddBuff);
                packet1.Write(Player.whoAmI);
                packet1.Write(320);
                packet1.Write(120);
                packet1.Send();

                packet1 = mod.GetPacket();
                packet1.Write((byte)MessageType.RequestAddBuff);
                packet1.Write(Player.whoAmI);
                packet1.Write(ModContent.BuffType<TimeDilation>());
                packet1.Write(120);
                packet1.Send();

                SoundEngine.PlaySound(SoundID.Item8, owner.Center);

                hurtProjectile.Kill();
            }
        }

        if (info.DamageSource.SourceItem != null && info.DamageSource.SourceItem.type == ItemID.TragicUmbrella)
        {
            Player.AddBuff(ModContent.BuffType<Endangered>(), 15 * 60);
        }

        if (Player.HasBuff(ModContent.BuffType<Endangered>()) && !isPickaxe && Player.statLife > 0 && info.Damage >= 8)
        {
            int initialHealth = Player.statLife;

            Player.statLife -= 4;

            CombatText.NewText(Player.getRect(), Color.Red, 4);

            if (initialHealth <= 4)
            {
                PlayerDeathReason reason = PlayerDeathReason.ByCustomReason("Ninja umbrella extra damage");
                reason.SourceItem = info.DamageSource.SourceItem;
                reason.SourcePlayerIndex = info.DamageSource.SourcePlayerIndex;
                reason.SourceProjectileType = info.DamageSource.SourceProjectileType;

                Player.KillMe(reason, 4, 0);
            }
        }

        // Paladin section
        if (modPlayer.currentClass.Name == "Paladin")
        {
            if (Player.HeldItem.type == 4760 && Main.mouseRight) // Paladin buffs when hit
            {
                Player.AddBuff(BuffID.Honey, 400);
                Player.AddBuff(BuffID.Regeneration, 400); //check if honey buff even works later
                Player.AddBuff(ModContent.BuffType<Retaliation>(), 30);
            }
        }
        if (info.DamageSource.SourceProjectileType == 229)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Tiki Priest")
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active || player.dead)
                        continue;
                    // ai[0] stores tiki's team
                    if (player.team != attacker.team)
                        continue;

                    if (Vector2.Distance(attacker.Center, player.Center) <= 14 * 16) // 14 block radius
                    {
                        ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                        packet.Write((byte)CTG2.MessageType.RequestHeal);
                        packet.Write(player.whoAmI);
                        packet.Write(7);
                        packet.Send();
                    }
                }
            }
            Player.ClearBuff(BuffID.Poisoned);
        }
        if (info.DamageSource.SourceProjectileType == ProjectileID.EmeraldBolt)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Tree")
            {
                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestHeal);
                packet.Write(attacker.whoAmI);
                packet.Write(6);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == ProjectileID.ApprenticeStaffT3Shot)
        {
            Player.AddBuff(ModContent.BuffType<Transmutated>(), 60);
            Player.AddBuff(BuffID.Cursed, 60);
        }
        if (info.DamageSource.SourceProjectileType == 732)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Psychic")
            {
                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestMana);
                packet.Write(attacker.whoAmI);
                packet.Write(10);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == ProjectileID.LaserMachinegunLaser
         || info.DamageSource.SourceProjectileType == ProjectileID.ElectrosphereMissile)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Astronaut")
            {
                int amount = attacker.HasBuff(BuffID.MagicPower) ? 4 : 2;
                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestMana);
                packet.Write(attacker.whoAmI);
                packet.Write(amount);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == ProjectileID.JavelinFriendly)
        {
            Player.AddBuff(BuffID.Dazed, 60);
        }
        if (info.DamageSource.SourceProjectileType == 153)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Mutant")
            {
                attacker.AddBuff(2, 120);
                attacker.AddBuff(48, 240);
                attacker.AddBuff(58, 120);

                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(2);
                packet.Write(120);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(48);
                packet.Write(240);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(58);
                packet.Write(120);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == ModContent.ProjectileType<PolearmProjectile>())
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Gladiator")
            {
                attacker.AddBuff(BuffID.CatBast, 60);
                attacker.AddBuff(BuffID.Endurance, 60);

                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(215);
                packet.Write(120);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(114);
                packet.Write(120);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == ProjectileID.DD2FlameBurstTowerT2Shot)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Phoenix")
            {
                attacker.AddBuff(BuffID.StarInBottle, 120);

                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(158);
                packet.Write(120);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == 273)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Leech")
            {
                attacker.AddBuff(58, 90);

                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(58);
                packet.Write(90);
                packet.Send();
            }
        }
        if (info.DamageSource.SourceProjectileType == 304)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Leech")
            {
                attacker.AddBuff(2, 90);
                attacker.AddBuff(5, 90);
                attacker.AddBuff(7, 90);
                attacker.AddBuff(114, 90);

                ModPacket packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(2);
                packet.Write(90);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(5);
                packet.Write(90);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(7);
                packet.Write(90);
                packet.Send();

                packet = ModContent.GetInstance<CTG2.CTG2>().GetPacket();
                packet.Write((byte)CTG2.MessageType.RequestAddBuff);
                packet.Write(attacker.whoAmI);
                packet.Write(114);
                packet.Write(90);
                packet.Send();
            }
        }

        Player.ClearBuff(BuffID.Ichor);
        Player.ClearBuff(BuffID.Poisoned);
    }
}
