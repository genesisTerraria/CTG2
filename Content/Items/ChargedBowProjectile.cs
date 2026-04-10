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
using System.IO;
using CTG2.Content.Items;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.DataStructures;

// --- RENDER LAYER ---
// This handles drawing so the outline system can wrap around the bow
public class ChargedBowDrawer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        // Only draw if the player is holding the projectile and it isn't "released" yet
        return drawInfo.drawPlayer.ownedProjectileCounts[ModContent.ProjectileType<ChargedBowProjectile>()] > 0;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        Projectile proj = null;

        // Find the projectile instance
        for (int i = 0; i < Main.maxProjectiles; i++) {
            if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<ChargedBowProjectile>()) {
                proj = Main.projectile[i];
                break;
            }
        }

        if (proj == null || proj.ModProjectile is not ChargedBowProjectile bow) return;

        // Access the fields from the projectile instance
        float rot = bow.GetRotation();
        float charge = Math.Min(bow.GetCharge(), 40f);
        bool released = bow.GetReleased();
        float td = bow.GetTD();

        SpriteEffects bowEffects = player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

        // ----------------------
        // COLOR / CHARGE GLOW
        // ----------------------
        Color drawColor = drawInfo.colorArmorBody; // Matches player lighting

        if (charge >= 40f) {
            Color highlight = Color.White;
            Color lowlight = new Color(150, 150, 150);
            drawColor = (td % 400) <= 200 
                ? Color.Lerp(drawColor, highlight, 0.2f) 
                : Color.Lerp(drawColor, lowlight, 0.2f);
        }

        // ----------------------
        // DRAW BOW
        // ----------------------
        Texture2D bowTexture = TextureAssets.Item[(int)proj.ai[0]].Value;
        Vector2 bowOrigin = bowTexture.Size() / 2f;
        Vector2 bowPos = player.MountedCenter + Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * 9f;

        drawInfo.DrawDataCache.Add(new DrawData(
            bowTexture,
            bowPos - Main.screenPosition,
            null,
            drawInfo.colorArmorBody,
            rot,
            bowOrigin,
            proj.scale,
            bowEffects,
            0
        ));

        // ----------------------
        // DRAW ARROW
        // ----------------------
        if (!released) {
            Texture2D arrowTexture = TextureAssets.Projectile[(int)proj.ai[1]].Value;
            float pullbackAmount = MathHelper.Lerp(0f, 13f, charge / 40f);
            Vector2 arrowPos = player.MountedCenter + 
                              Vector2.One.RotatedBy(rot - MathHelper.PiOver4) * 16f - 
                              new Vector2(pullbackAmount, 0f).RotatedBy(rot);

            drawInfo.DrawDataCache.Add(new DrawData(
                arrowTexture,
                arrowPos - Main.screenPosition,
                null,
                drawColor,
                rot + MathHelper.PiOver2,
                arrowTexture.Size() / 2f,
                proj.scale,
                SpriteEffects.None,
                0
            ));
        }
    }
}

// --- PROJECTILE CLASS ---
public class ChargedBowProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_294";
    private float Rotation, c1, td;
    public float charge;
    private int t;
    private bool recentlyFired = false;
    private bool start;
    private SlotId sound;
    private bool released;
    private int syncTimer;

    // Getters for the Drawer Layer
    public float GetRotation() => Rotation;
    public float GetCharge() => charge;
    public bool GetReleased() => released;
    public float GetTD() => td;

    SoundStyle bowSound = new SoundStyle("CTG2/Content/Items/BowSound");
    SoundStyle bowSound2 = new SoundStyle("CTG2/Content/Items/BowSound2");

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(Math.Min(charge, 40f));
        writer.Write(Rotation);
        writer.Write(released);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        charge = reader.ReadSingle();
        Rotation = reader.ReadSingle();
        released = reader.ReadBoolean();
    }

    public override void SetDefaults() {
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.aiStyle = 75;
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.ownerHitCheck = true;
        Projectile.penetrate = -1;
    }

    // PreDraw is now disabled because the PlayerDrawLayer handles it
    public override bool PreDraw(ref Color lightColor) => false;

    public override void AI() {
        Player player = Main.player[Projectile.owner];

        Projectile.timeLeft = 2;
        Projectile.position = player.MountedCenter;
        Projectile.knockBack = player.HeldItem.knockBack;

        if (charge >= 40f) td++; // Increment glow timer

        if (Projectile.owner == Main.myPlayer) {
            Vector2 aim = Main.MouseWorld - player.MountedCenter;
            Rotation = aim.ToRotation();
            player.direction = Math.Cos(Rotation) >= 0 ? 1 : -1;

            if (player.channel && !released) {
                charge = charge + 1f;
                if (charge >= 40f && c1 == 0f) {
                    c1 = 1f;
                    if (SoundEngine.TryGetActiveSound(sound, out var s)) s.Stop();
                    SoundEngine.PlaySound(SoundID.MaxMana.WithVolumeScale(Main.soundVolume * 2f), player.Center);
                }
            }
            else if (!released) {
                released = true;
                Projectile.netUpdate = true;
                if (SoundEngine.TryGetActiveSound(sound, out var s)) s.Stop();
            }

            if (++syncTimer % 3 == 0) Projectile.netUpdate = true;
        }

        if (player.channel && !released && !start) {
            sound = SoundEngine.PlaySound(bowSound.WithVolumeScale(Main.soundVolume * 1.25f), player.Center);
            start = true;
        }

        if (released && Projectile.owner == Main.myPlayer && !recentlyFired) {
            Item item = player.HeldItem;
            Vector2 speed = new Vector2(item.shootSpeed, 0f).RotatedBy(Rotation) * (0.5f + (Math.Min(charge, 40f) / 40f) * 0.5f) * 1.8f;
            Vector2 spawnPos = player.MountedCenter + Vector2.One.RotatedBy(Rotation - MathHelper.PiOver4) * 2f;

            Projectile arrow = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                spawnPos,
                speed,
                (int)Projectile.ai[1],
                (int)player.GetDamage(item.DamageType).ApplyTo(item.damage * (0.5f + Math.Min(charge, 40f) / 80f)),
                item.knockBack,
                Projectile.owner
            );
            arrow.extraUpdates = 1;
            arrow.netUpdate = true;
        }

        if (released && !recentlyFired) {
            SoundEngine.PlaySound(bowSound2.WithVolumeScale(Main.soundVolume * 2.5f), Projectile.position);
            recentlyFired = true;
        }

        if (released) {
            t++;
            if (t >= player.HeldItem.useTime) Projectile.Kill();
        }
    }
}


public class ChargedBowPlayer : ModPlayer
{
    public override void ResetEffects()
    {
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.owner == Player.whoAmI && p.ModProjectile is ChargedBowProjectile bow)
            {
                if (bow.charge >= 60f)
                {
                    Player.moveSpeed *= 0.8f;
                    Player.maxRunSpeed *= 0.8f;
                }
                break;
            }
        }
    }
}