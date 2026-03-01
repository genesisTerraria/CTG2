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


public class ChargedBowProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/Projectile_294";
	private float Rotation, c1, td, charge;
	private int damage, t;
	private bool load = false;
	private bool recentlyFired = false;
	private bool start;
	private SlotId sound;
	private bool released;
	private int syncTimer;


	SoundStyle bowSound = new SoundStyle("CTG2/Content/Items/BowSound");
	SoundStyle bowSound2 = new SoundStyle("CTG2/Content/Items/BowSound2");


	public override void SendExtraAI(BinaryWriter writer)
	{
		writer.Write(charge);
		writer.Write(Rotation);
		writer.Write(released);
	}

	public override void ReceiveExtraAI(BinaryReader reader)
	{
		charge = reader.ReadSingle();
		Rotation = reader.ReadSingle();
		released = reader.ReadBoolean();
	}



	public override void SetDefaults()
	{
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


	public override bool PreDraw(ref Color lightColor)
	{
		Player player = Main.player[Projectile.owner];

		// Load arrow texture once
		if (!load)
		{
			Main.instance.LoadProjectile((int)Projectile.ai[1]);
			load = true;
		}

		// Determine facing direction from rotation
		player.direction = Math.Cos(Rotation) >= 0 ? 1 : -1;

		SpriteEffects bowEffects =
			player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;

		// ----------------------
		// COLOR / CHARGE GLOW
		// ----------------------
		Color drawColor = lightColor;

		if (charge >= 40f)
		{
			td++;
			Color highlight = Color.White;
			Color lowlight = new(150, 150, 150);

			drawColor = td <= 200
				? Color.Lerp(drawColor, highlight, 0.2f)
				: Color.Lerp(drawColor, lowlight, 0.2f);

			if (td > 400)
				td = 0;
		}

		// ----------------------
		// DRAW BOW
		// ----------------------
		Texture2D bowTexture = TextureAssets.Item[(int)Projectile.ai[0]].Value;
		Rectangle bowFrame = bowTexture.Bounds;
		Vector2 bowOrigin = bowFrame.Size() / 2f;

		Vector2 bowPos =
			player.MountedCenter +
			Vector2.One.RotatedBy(Rotation - MathHelper.PiOver4) * 9f;

		Main.EntitySpriteDraw(
			bowTexture,
			bowPos - Main.screenPosition,
			bowFrame,
			lightColor,
			Rotation,
			bowOrigin,
			Projectile.scale,
			bowEffects
		);

		// ----------------------
		// DRAW ARROW (IF NOT RELEASED)
		// ----------------------
		if (!released)
		{
			Texture2D arrowTexture = TextureAssets.Projectile[(int)Projectile.ai[1]].Value;
			Rectangle arrowFrame = arrowTexture.Bounds;
			Vector2 arrowOrigin = arrowFrame.Size() / 2f;

			// Pullback amount derived purely from synced charge
			float pullbackAmount = MathHelper.Lerp(0f, 13f, charge / 40f);

			Vector2 arrowPos =
				player.MountedCenter +
				Vector2.One.RotatedBy(Rotation - MathHelper.PiOver4) * 16f -
				new Vector2(pullbackAmount, 0f).RotatedBy(Rotation);

			Main.EntitySpriteDraw(
				arrowTexture,
				arrowPos - Main.screenPosition,
				arrowFrame,
				drawColor,
				Rotation + MathHelper.PiOver2,
				arrowOrigin,
				Projectile.scale,
				SpriteEffects.None
			);
		}

		return false;
	}


	public override void AI()
	{
		Player player = Main.player[Projectile.owner];

		Projectile.timeLeft = 2;
		Projectile.position = player.MountedCenter;
		Projectile.knockBack = player.HeldItem.knockBack;

		// ===============================
		// OWNER ONLY: INPUT + SOUNDS
		// ===============================
		if (Projectile.owner == Main.myPlayer)
		{
			Vector2 aim = Main.MouseWorld - player.MountedCenter;
			Rotation = aim.ToRotation();

			// -------------------------------
			// CHARGING
			// -------------------------------
			if (player.channel && !released)
			{
				charge = Math.Min(charge + 1f, 40f);

				// Start charging sound once
				if (!start)
				{
					sound = SoundEngine.PlaySound(
						bowSound.WithVolumeScale(Main.soundVolume * 1.25f),
						player.Center
					);
					start = true;
				}

				// Fully charged transition
				if (charge >= 40f && c1 == 0f)
				{
					c1 = 1f;

					if (SoundEngine.TryGetActiveSound(sound, out var s))
						s.Stop();

					SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
				}
			}

			// -------------------------------
			// RELEASE
			// -------------------------------
			else if (!released)
			{
				released = true;
				Projectile.netUpdate = true;

				// Stop any charging sound
				if (SoundEngine.TryGetActiveSound(sound, out var s))
					s.Stop();
			}

			// Sync charging state periodically
			if (++syncTimer % 3 == 0)
				Projectile.netUpdate = true;
		}

		// ===============================
		// FIRE ARROW (OWNER ONLY)
		// ===============================
		if (released && Projectile.owner == Main.myPlayer && !recentlyFired)
		{
			Item item = player.HeldItem;

			Vector2 speed =
				new Vector2(item.shootSpeed, 0f)
				.RotatedBy(Rotation)
				* (0.5f + (charge / 40f) * 0.5f)
				* 1.8f;

			Vector2 spawnPos =
				player.MountedCenter +
				Vector2.One.RotatedBy(Rotation - MathHelper.PiOver4) * 2f;

			Projectile arrow = Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				spawnPos,
				speed,
				(int)Projectile.ai[1],
				(int)(item.damage * (0.5 + charge / 80f)),
				item.knockBack,
				Projectile.owner
			);

			arrow.extraUpdates = 1;
			arrow.netUpdate = true;

			SoundEngine.PlaySound(
				bowSound2.WithVolumeScale(Main.soundVolume * 2.5f),
				Projectile.position
			);

			recentlyFired = true;
		}

		// ===============================
		// CLEANUP / LIFETIME
		// ===============================
		if (released)
		{
			if (Projectile.owner == Main.myPlayer)
			{
				t++;
				if (t >= player.HeldItem.useTime)
				{
					Projectile.Kill();
				}
			}
		}
	}
}
