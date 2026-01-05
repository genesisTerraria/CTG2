using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using CTG2.Content.Items;
using Terraria.Audio;
using CTG2.Content.ClientSide;
using CTG2.Content.Buffs;


public class ProjectileOverrides : GlobalProjectile
{
    public override bool InstancePerEntity => true;


    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        if (projectile.type == ProjectileID.NebulaArcanum)
        {
            projectile.damage = 0;

            return false;
        }

        return true;
    }


    public override void AI(Projectile projectile)
    {
        if (projectile.type == 167 || projectile.type == 169)
            projectile.timeLeft = 0;
        if (projectile.type == 511)
        {
            // Force a new timeLeft value (e.g., 120 ticks = 2 seconds)
            if (projectile.timeLeft > 120)
            {
                projectile.timeLeft = 120;
            }
        }
        if (projectile.type == 41)
        {
            projectile.extraUpdates = 1; // determines how quickly the projectile falls and velocity magnitude
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


    public override void OnHurt(Player.HurtInfo info)
    {
        var modPlayer = Player.GetModPlayer<PlayerManager>();
        int attackerIndex = info.DamageSource.SourcePlayerIndex;
        int projIndex = info.DamageSource.SourceProjectileLocalIndex;
        if (projIndex >= 0 && projIndex < Main.maxProjectiles)
        {
            Projectile proj = Main.projectile[projIndex];
            if (proj.active && (proj.type == 263 || proj.type == 513))
            {
                proj.Kill();
            }
        }


        if (modPlayer.currentClass.Name == "Paladin")
        {
            Player.AddBuff(BuffID.RapidHealing, 300);

            if (Player.HeldItem.type == 4760 && Main.mouseRight) // Paladin buffs when hit
            {
                Player.AddBuff(BuffID.Honey, 300); //check if honey buff even works later
                Player.AddBuff(2, 180); // regeneration
                Player.AddBuff(ModContent.BuffType<Retaliation>(), 24);
            }
        }
        else if (info.DamageSource.SourceProjectileType == ModContent.ProjectileType<AmalgamatedHandProjectile1>() || info.DamageSource.SourceProjectileType == ModContent.ProjectileType<AmalgamatedHandProjectile2>())
        {
            Player.ClearBuff(BuffID.OnFire);
        }
        else if (info.DamageSource.SourceProjectileType == 267)
        {
            Player attacker = Main.player[attackerIndex];
            var attackerPlayer = attacker.GetModPlayer<PlayerManager>();
            if (attackerPlayer.currentClass.Name == "Tiki Priest")
                attacker.Heal(4);
            Player.ClearBuff(BuffID.Poisoned);
        }
        else if (info.DamageSource.SourceProjectileType == ProjectileID.ThornChakram)
        {
            Player.ClearBuff(BuffID.Poisoned);
        }
        else if (info.DamageSource.SourceProjectileType == 480) // jman cursed inferno
        {
            Player.ClearBuff(BuffID.CursedInferno);
        }
        else if (info.DamageSource.SourceProjectileType == 19) //flamebunny flamrang
        {
            Player.ClearBuff(24);
        }
        else if (info.DamageSource.SourceProjectileType == 15) //flamebunny fof
        {
            Player.ClearBuff(24);
        }
        else if (info.DamageSource.SourceProjectileType == 280) //goldenshowerproj
        {
            Player.ClearBuff(BuffID.Ichor);
        }
        else if (info.DamageSource.SourceProjectileType == 267) //Poison dart
        {
            Player.ClearBuff(BuffID.Poisoned);
        }
    }
}
