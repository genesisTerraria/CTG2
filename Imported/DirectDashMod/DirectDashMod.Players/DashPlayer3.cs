using System;
using System.Collections.Generic;
using System.IO;
using CTG2.Imported.DirectDashMod.Buffs;
using CTG2.Imported.DirectDashMod.Particles;
using CTG2.Imported.DirectDashMod.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Enums;
using CTG2;

namespace DirectDashMod.Players;

public class DashPlayer3 : ModPlayer
{
	public delegate bool TargTile(Tile tile);

	public static SoundStyle DASH_NOISE;

	public static Texture2D DRAW_BOUND_TEX;

	public const int COLOR_FLASH_MAX = 15;

	public const int TILESIZE = 16;

	public const float BASE_DASH_SPEED = 11.2f;

	public const float BASE_DASH_DIST = 224f;

	public const int DASH_FRAME_SPACE = 32;

	public static readonly int[] COL_SHIFT = new int[12]
	{
		210, 30, 270, 120, 0, 240, 60, 150, 90, 180,
		300, 330
	};

	public static readonly Vector2 DASH_DIST_SCALE = new Vector2(3f, 2f).SafeNormalize(Vector2.Zero);

	public static readonly Vector2 SMALL2 = new Vector2(0.01f);

	public Vector2 dashDir;

	public int dashDur;

	public int dashDurMax;

	public float lastMinSPeed = 1f;

	public int dashShadowDur;

	public int dashCount;

	public int dashCooldown;

	public Vector2 lastShadowPos;

	public List<DashFrameInfo> dashShadowFrames;

	public bool boostSpeed;

	public float colorFlashTime;

	public Vector2 expectedPos;

	public bool Swimming;

	public Vector2 lastSwimPos;

	public int airJumpFrames;

	public bool Bubbleing;

	public byte heldDashFrames;

	public static ModKeybind DashKey = null;

	public bool grappling;

	public int acc_dashCooldown;

	public int acc_dashCountMax;

	public float acc_dashDist;

	public float acc_dashSpeed;

	public bool canRefreshDash;

	public Color acc_Color = Color.Transparent;

	public bool acc_RunDash;

	public bool acc_SafeDash;

	public bool acc_ShieldDash = true;

	public bool acc_RedCoat;

	public float acc_dashCooldownMult = 1f;

	public bool acc_CanMount = true;

	public float DASH_SPEED => this.acc_dashSpeed;

	public float DASH_DIST => this.acc_dashDist;

	public int DASH_MAX => this.acc_dashCountMax;

	public bool Dashing => this.dashDur > 0;

	public int DashCooldown => (int)((float)this.acc_dashCooldown * this.acc_dashCooldownMult);

	public static void ResetFallDamage(Player ply)
	{
		int diff = (int)ply.position.Y / 16 - ply.fallStart2;
		ply.fallStart2 += diff;
		ply.fallStart += diff;
	}

	public void Dash(Vector2 dir)
	{
		this.dashDur = (this.dashDurMax = (this.dashShadowDur = (int)(this.DASH_DIST / this.DASH_SPEED)));
		this.dashDir = dir.SafeNormalize(Vector2.Zero);
		base.Player.velocity = this.dashDir * this.DASH_SPEED;
		base.Player.RemoveAllGrapplingHooks();
		this.lastShadowPos = base.Player.Center - this.dashDir * 32f;
		this.colorFlashTime = 15f;
		if (this.dashCount == 1 && this.DashCooldown > 0)
		{
			base.Player.AddBuff(ModContent.BuffType<Buff_DashCooldown>(), this.DashCooldown);
			this.canRefreshDash = false;
		}
		base.Player.GetModPlayer<WallJumpPlayer>().grabDir = 0;
		if (base.Player.mount.Active)
		{
			base.Player.mount.Dismount(base.Player);
		}
		if (base.Player.whoAmI == Main.myPlayer && Main.netMode == 1)
		{
			this.SendDash(-1, Main.myPlayer);
		}
	}

	public void RecieveDash(Player ply, BinaryReader reader)
	{
		int dashTime = reader.Read7BitEncodedInt();
		Vector2 dir = reader.ReadVector2();
		Vector2 pos = reader.ReadVector2();
		byte dashNumber = reader.ReadByte();
		this.dashDur = (this.dashDurMax = (this.dashShadowDur = dashTime));
		this.dashDir = dir;
		base.Player.velocity = DashPlayer3.StopJumpReset(this.dashDir * this.DASH_SPEED);
		base.Player.position = pos;
		base.Player.RemoveAllGrapplingHooks();
		this.dashCount = dashNumber;
		if (this.dashCount == 1 && this.DashCooldown > 0)
		{
			base.Player.AddBuff(ModContent.BuffType<Buff_DashCooldown>(), this.DashCooldown);
			this.canRefreshDash = false;
		}
		base.Player.GetModPlayer<WallJumpPlayer>().grabDir = 0;
		if (base.Player.mount.Active)
		{
			base.Player.mount.Dismount(base.Player);
		}
		this.lastShadowPos = base.Player.Center - this.dashDir * 32f;
		this.colorFlashTime = 15f;
	}

	public void SendDash(int toWho, int fromWho)
	{
		ModPacket packet = base.Mod.GetPacket();
		packet.Write((byte)MessageType.DASH);
		packet.Write((byte)base.Player.whoAmI);
		packet.Write7BitEncodedInt(this.dashDur);
		packet.WriteVector2(this.dashDir);
		packet.WriteVector2(base.Player.position);
		packet.Write((byte)this.dashCount);
		packet.Send(toWho, fromWho);
	}

	public bool StopDash()
	{
		if (this.Dashing)
		{
			base.Player.GetModPlayer<MoveOverridePlayer>().IsOverriding = false;
			this.boostSpeed = true;
			this.heldDashFrames = 5;
			this.dashDur = 0;
			return true;
		}
		return false;
	}

	public void StartSwim()
	{
		this.StopDash();
		this.Swimming = true;
		this.lastSwimPos = base.Player.position;
	}

	public void AddDashShadow(List<DrawData> data, Vector2 pos)
	{
		this.dashShadowFrames.Add(new DashFrameInfo(data, this, pos));
		this.lastShadowPos = pos;
	}

	public static Color RotateHue(Color col, int amount, float minMult, float maxMult)
	{
		byte[] cols = new byte[3] { col.R, col.G, col.B };
		byte min = cols[0];
		byte max = cols[0];
		int best = 0;
		for (int I = 1; I < 3; I++)
		{
			if (cols[I] < min)
			{
				min = cols[I];
			}
			if (cols[I] > max)
			{
				max = cols[I];
				best = I;
			}
		}
		if (min == max)
		{
			return col;
		}
		float hue = best * 2;
		int fore = (best + 1) % 3;
		int back = (best + 2) % 3;
		hue = ((cols[fore] <= cols[back]) ? (hue - (float)(cols[back] - min) / (float)(max - min)) : (hue + (float)(cols[fore] - min) / (float)(max - min)));
		hue += (float)amount / 60f;
		hue = (hue / 6f - (float)(int)(hue / 6f)) * 6f;
		if (hue < 0f)
		{
			hue += 6f;
		}
		int hueIndx = (int)hue;
		float rper = hue - (float)hueIndx;
		best = hueIndx / 2;
		min = (byte)((float)(int)min * minMult);
		max = (byte)((float)(int)max * maxMult);
		fore = (best + 1) % 3;
		back = (best + 2) % 3;
		if (hueIndx % 2 == 0)
		{
			cols[best] = max;
			cols[fore] = (byte)((float)(max - min) * rper + (float)(int)min);
			cols[back] = min;
		}
		else
		{
			cols[best] = (byte)((float)(max - min) * (1f - rper) + (float)(int)min);
			cols[fore] = max;
			cols[back] = min;
		}
		return new Color(cols[0], cols[1], cols[2], col.A);
	}

	public static Color GetColorCount(Color col, int jumpsUsed, int maxJumps)
	{
		if (jumpsUsed == 0)
		{
			return col;
		}
		int rotate = DashPlayer3.COL_SHIFT[(maxJumps + DashPlayer3.COL_SHIFT.Length - jumpsUsed) % DashPlayer3.COL_SHIFT.Length] - DashPlayer3.COL_SHIFT[maxJumps % DashPlayer3.COL_SHIFT.Length];
		if (rotate < 0)
		{
			rotate += 360;
		}
		float mult = 10f / (float)(jumpsUsed + 10);
		return DashPlayer3.RotateHue(col, rotate, mult, mult);
	}

	public Color GetDashedColor(Color col)
	{
		return DashPlayer3.GetColorCount(col, this.dashCount, this.DASH_MAX);
	}

	public bool BoostedSpeed()
	{
		if (!(Math.Abs(base.Player.velocity.X) > base.Player.accRunSpeed))
		{
			return Math.Abs(base.Player.velocity.Y) > base.Player.maxFallSpeed;
		}
		return true;
	}

	public float GetDashSpeed()
	{
		float per = (float)this.dashDur / (float)this.dashDurMax;
		float minSpeed = this.lastMinSPeed;
		return (float)((double)(this.DASH_SPEED - minSpeed) * Math.Sin(Math.PI / 2.0 * (double)per) * Math.PI / 2.0 + (double)minSpeed);
	}

	public static Vector2 StopJumpReset(Vector2 vec)
	{
		if (vec.Y == 0f)
		{
			return new Vector2(vec.X, 1E-07f);
		}
		return vec;
	}

	public override void SetStaticDefaults()
	{
	}

	public override void Load()
	{
		DashPlayer3.DashKey = KeybindLoader.RegisterKeybind(base.Mod, "Dash", "Mouse3");
		DashPlayer3.DRAW_BOUND_TEX = ModContent.Request<Texture2D>("CTG2/Imported/DirectDashMod/Images/WaterSpace").Value;
	}

	public override void Unload()
	{
		DashPlayer3.DashKey = null;
	}

	public override void Initialize()
	{
		this.dashShadowFrames = new List<DashFrameInfo>();
	}

	public override bool CanStartExtraJump(ExtraJump jump)
	{
		if (!this.Dashing && this.dashShadowDur == 0)
		{
			return this.airJumpFrames == 0;
		}
		return false;
	}

	public override void PostUpdateMiscEffects()
	{
		if (this.Dashing)
		{
			base.Player.canCarpet = false;
		}
	}

	public override void PreUpdate()
	{
		for (int I = this.dashShadowFrames.Count - 1; I >= 0; I--)
		{
			if (this.dashShadowFrames[I].life <= 1f)
			{
				this.dashShadowFrames.RemoveAt(I);
			}
			else
			{
				this.dashShadowFrames[I].life -= 1f;
			}
		}
		if (this.dashDur > 0 && --this.dashDur == 0)
		{
			base.Player.velocity = DashPlayer3.StopJumpReset(this.dashDir * this.GetDashSpeed());
			this.StopDash();
		}
		if (this.dashShadowDur > 0)
		{
			this.dashShadowDur--;
		}
		if (this.colorFlashTime > 0f)
		{
			this.colorFlashTime -= 1f;
		}
		if (this.dashCooldown > 0)
		{
			this.dashCooldown--;
		}
		if (this.airJumpFrames > 0)
		{
			this.airJumpFrames--;
		}
		if ((base.Player.velocity.Y == 0f && base.Player.GetModPlayer<WallJumpPlayer>().grabDir == 0) || this.grappling)
		{
			if (this.canRefreshDash && this.dashCooldown == 0 && this.dashCount > 0)
			{
				this.colorFlashTime = 7f;
				this.dashCount = 0;
				if (base.Player.whoAmI == Main.myPlayer)
				{
					SoundEngine.PlaySound(in SoundID.MaxMana, base.Player.Center);
				}
			}
			this.boostSpeed = false;
		}
		else if (!this.BoostedSpeed())
		{
			this.boostSpeed = false;
		}
		if ((this.Dashing || this.Swimming) && base.Player.mount.Active)
		{
			base.Player.mount.Dismount(base.Player);
		}
		foreach (DashFrameInfo dashShadowFrame in this.dashShadowFrames)
		{
			dashShadowFrame.Clean(this);
		}
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
	{
		if (drawInfo.shadow == 0f && this.boostSpeed && this.BoostedSpeed() && (base.Player.Center - this.lastShadowPos).LengthSquared() > 9216f)
		{
			this.lastShadowPos = base.Player.Center;
			Dust d = Dust.NewDustPerfect(base.Player.Center, ModContent.DustType<DashDust>(), base.Player.velocity, 64);
			drawInfo.DustCache.Add(d.dustIndex);
		}
	}

	public override void PreUpdateMovement()
	{
		this.grappling = base.Player.grappling[0] != -1;
		if (this.Bubbleing)
		{
			Vector2 vel = this.dashDir * 10f;
			base.Player.GetModPlayer<MoveOverridePlayer>().SetPlayerSpeed(vel, AllowRefresh: true);
			this.expectedPos = base.Player.position + vel;
		}
		else if (this.Swimming)
		{
			base.Player.velocity = Vector2.Zero;
		}
		else if (this.Dashing)
		{
			float speed = this.GetDashSpeed();
			Vector2 vel2 = DashPlayer3.StopJumpReset(this.dashDir * speed);
			base.Player.GetModPlayer<MoveOverridePlayer>().SetPlayerSpeed(vel2);
			this.expectedPos = base.Player.position + base.Player.velocity;
		}
	}

	public static float CheckAllTilesWithin(Vector2 _start, Vector2 _end, Vector2 size, TargTile cond, bool last = false, float minDist = 0f, float maxDist = 2f)
	{
		Vector2 start;
		Vector2 end;
		if (_start.X > _end.X)
		{
			start = _end;
			end = _start;
		}
		else
		{
			start = _start;
			end = _end;
		}
		int dirX = ((_end.X >= _start.X) ? 1 : (-1));
		int num = ((_end.Y >= _start.Y) ? 1 : (-1));
		int ndx = (dirX + 1) / 2;
		int ndy = (num + 1) / 2;
		int minY = Math.Max(0, (int)(Math.Min(start.Y, end.Y) / 16f));
		int maxY = Math.Min(Main.tile.Height - 1, (int)((Math.Max(start.Y, end.Y) + size.Y) / 16f));
		float dy = end.Y - start.Y;
		float tall = size.Y + Math.Abs(dy);
		float xP0 = (_start.X + size.X * (float)ndx) / 16f;
		float xP1 = (_end.X + size.X * (float)ndx) / 16f;
		float yP0 = (_start.Y + size.Y * (float)ndy) / 16f;
		float yP1 = (_end.Y + size.Y * (float)ndy) / 16f;
		int flip = ((!last) ? 1 : (-1));
		float best = 1f;
		int x0 = Math.Max(0, (int)(start.X / 16f));
		int x1 = Math.Min(Main.tile.Width - 1, (int)((end.X + size.X) / 16f));
		dy = ((x0 != x1) ? (dy / (float)(x1 - x0)) : 0f);
		tall += dy;
		float curY = start.Y + ((dy < 0f) ? dy : 0f);
		int x2 = x0;
		while (x2 <= x1)
		{
			float xper = ((xP1 - xP0 == 0f) ? (-10f) : (((float)(x2 + (1 - ndx)) - xP0) / (xP1 - xP0)));
			int num2 = Math.Max(minY, (int)(curY / 16f));
			int y1 = Math.Min(maxY, (int)((curY + tall) / 16f));
			for (int i = num2; i <= y1; i++)
			{
				float yper = ((yP1 - yP0 == 0f) ? (-10f) : (((float)(i + (1 - ndy)) - yP0) / (yP1 - yP0)));
				float realPer = Math.Max(xper, yper);
				if (realPer >= minDist && realPer <= maxDist)
				{
					Tile tile = Main.tile[x2, i];
					if (cond(tile))
					{
						best = Math.Min(best * (float)flip, realPer * (float)flip) * (float)flip;
					}
				}
			}
			x2++;
			curY += dy;
		}
		return best;
	}

	public static Point CountAllTilesWithin(Vector2 _start, Vector2 _end, Vector2 size, TargTile cond, float minPer = -9999f, float maxPer = 2f)
	{
		Vector2 start;
		Vector2 end;
		if (_start.X > _end.X)
		{
			start = _end;
			end = _start;
		}
		else
		{
			start = _start;
			end = _end;
		}
		int dirX = ((_end.X >= _start.X) ? 1 : (-1));
		int num = ((_end.Y >= _start.Y) ? 1 : (-1));
		int ndx = (dirX + 1) / 2;
		int ndy = (num + 1) / 2;
		int minY = (int)(Math.Min(start.Y, end.Y) / 16f);
		int maxY = (int)((Math.Max(start.Y, end.Y) + size.Y) / 16f);
		float dy = end.Y - start.Y;
		float tall = size.Y + Math.Abs(dy);
		float xP0 = (_start.X + size.X * (float)ndx) / 16f;
		float xP1 = (_end.X + size.X * (float)ndx) / 16f;
		float yP0 = (_start.Y + size.Y * (float)ndy) / 16f;
		float yP1 = (_end.Y + size.Y * (float)ndy) / 16f;
		int count = 0;
		int total = 0;
		int x0 = (int)(start.X / 16f);
		int x1 = (int)((end.X + size.X) / 16f);
		dy = ((x0 != x1) ? (dy / (float)(x1 - x0)) : 0f);
		tall += dy;
		float curY = start.Y + ((dy < 0f) ? dy : 0f);
		int x2 = x0;
		while (x2 <= x1)
		{
			float xper = ((xP1 - xP0 == 0f) ? (-10f) : (((float)(x2 + (1 - ndx)) - xP0) / (xP1 - xP0)));
			int num2 = Math.Max(minY, (int)(curY / 16f));
			int y1 = Math.Min(maxY, (int)((curY + tall) / 16f));
			for (int i = num2; i <= y1; i++)
			{
				float yper = ((yP1 - yP0 == 0f) ? (-10f) : (((float)(i + (1 - ndy)) - yP0) / (yP1 - yP0)));
				float realPer = Math.Max(xper, yper);
				if (realPer >= minPer && realPer <= maxPer)
				{
					Tile tile = Main.tile[x2, i];
					if (cond(tile))
					{
						count++;
					}
					total++;
				}
			}
			x2++;
			curY += dy;
		}
		return new Point(count, total);
	}

	public override void PostUpdate()
	{
		int SpaceTile = ModContent.TileType<SpaceWaterTile>();
		Vector2 pos = base.Player.position + DashPlayer3.SMALL2;
		Vector2 size2 = base.Player.Size - DashPlayer3.SMALL2 * 2f;
		if (this.Dashing)
		{
			if (base.Player.position != this.expectedPos)
			{
				float firstWater = DashPlayer3.CheckAllTilesWithin(pos, this.expectedPos + DashPlayer3.SMALL2, size2, (Tile tile) => tile.HasUnactuatedTile && tile.TileType == SpaceTile, last: false, 0.0001f);
				float firstSolid = DashPlayer3.CheckAllTilesWithin(pos, this.expectedPos + DashPlayer3.SMALL2, size2, (Tile tile) => tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && tile.TileType != SpaceTile, last: false, 0.0001f);
				if (firstWater < firstSolid)
				{
					MoveOverridePlayer.BoundPos(base.Player.position + (this.expectedPos - base.Player.position) * firstWater, base.Player.Size);
					this.StartSwim();
				}
			}
			if (this.acc_RedCoat)
			{
				for (int I = 0; I < Main.maxNPCs; I++)
				{
					NPC npc = Main.npc[I];
					if (npc.active && !npc.friendly && npc.Hitbox.Intersects(base.Player.Hitbox) && npc.immune[base.Player.whoAmI] == 0)
					{
						bool crit = Main.rand.NextFloat() < base.Player.GetTotalCritChance(DamageClass.Melee) / 100f;
						NPC.HitInfo info = npc.CalculateHitInfo(base.Player.statLifeMax / 12 + (int)base.Player.statDefense, base.Player.direction, crit, 6f, DamageClass.Melee);
						npc.StrikeNPC(info);
						npc.immune[base.Player.whoAmI] = 10;
					}
				}
			}
			if (this.acc_ShieldDash && this.dashDur > 3)
			{
				bool hit = false;
				for (int I2 = 0; I2 < Main.maxNPCs; I2++)
				{
					NPC npc2 = Main.npc[I2];
					if (npc2.active && !npc2.friendly && npc2.Hitbox.Intersects(base.Player.Hitbox))
					{
						bool crit2 = Main.rand.NextFloat() < base.Player.GetTotalCritChance(DamageClass.Melee) / 100f;
						NPC.HitInfo info2 = npc2.CalculateHitInfo(30 + (int)base.Player.statDefense, base.Player.direction, crit2, 9f, DamageClass.Melee);
						npc2.StrikeNPC(info2);
						hit = true;
					}
				}
				if (hit)
				{
					this.dashDur = 3;
					this.dashDir *= -1f;
				}
			}
		}
		else if (this.Swimming)
		{
			base.Player.position = this.lastSwimPos;
			Vector2 vel = this.dashDir * this.DASH_SPEED * 1.2f;
			Vector2 targPos = base.Player.position + vel;
			DashPlayer3.CheckAllTilesWithin(pos, targPos + DashPlayer3.SMALL2, size2, (Tile tile) => tile.HasUnactuatedTile && tile.TileType == SpaceTile, last: true);
			Point point = DashPlayer3.CountAllTilesWithin(pos, targPos + DashPlayer3.SMALL2, size2, (Tile tile) => tile.HasUnactuatedTile && tile.TileType == SpaceTile);
			base.Player.position = MoveOverridePlayer.BoundPos(base.Player.position + vel, base.Player.Size);
			if (point.X < 2)
			{
				this.Swimming = false;
				base.Player.velocity = DashPlayer3.StopJumpReset(this.dashDir * this.DASH_SPEED * 1.2f);
				if (this.dashDir.X != 0f)
				{
					this.airJumpFrames = 8;
				}
				if (base.Player.GetModPlayer<WallJumpPlayer>().acc_ChainBounce)
				{
					this.dashCount = 0;
				}
			}
			else
			{
				this.lastSwimPos = base.Player.position;
			}
		}
		else if (this.Bubbleing)
		{
			MoveOverridePlayer movePly = base.Player.GetModPlayer<MoveOverridePlayer>();
			if (movePly.expectedPosition.DistanceSQ(movePly.lastPosition) > 1f)
			{
				base.Player.mount.Dismount(base.Player);
			}
		}
		else
		{
			WallJumpPlayer jumpPly = base.Player.GetModPlayer<WallJumpPlayer>();
			if (Main.myPlayer == base.Player.whoAmI && this.airJumpFrames > 0 && jumpPly.heldJumpFrames > 0 && jumpPly.heldJumpFrames < 5)
			{
				jumpPly.ForceJump();
			}
		}
		this.lastMinSPeed = (this.acc_RunDash ? base.Player.accRunSpeed : base.Player.maxRunSpeed);
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (triggersSet.QuickMount && !this.acc_CanMount)
		{
			base.Player.controlMount = false;
			triggersSet.QuickMount = false;
		}
		bool dashingKey = DashPlayer3.DashKey.Current;
		if (this.dashCount < this.DASH_MAX && !this.Dashing && dashingKey && this.heldDashFrames < 5)
		{
			this.dashCount++;
			this.heldDashFrames = 5;
			this.dashCooldown = 5;
			int up = (triggersSet.Up ? 1 : 0);
			int down = (triggersSet.Down ? 1 : 0);
			int left = (triggersSet.Left ? 1 : 0);
			int right = (triggersSet.Right ? 1 : 0);
			Vector2 dir = default(Vector2);
			if (up - down != 0 || left - right != 0)
			{
				dir.X = right - left;
				dir.Y = down - up;
				dir.Normalize();
			}
			else
			{
				dir.X = base.Player.direction;
				dir.Y = 0f;
			}
			this.Dash(dir);
			DashPlayer3.ResetFallDamage(base.Player);
			SoundEngine.PlaySound(in SoundID.Item9, base.Player.Center);
		}
		if (dashingKey)
		{
			if (this.heldDashFrames < 5)
			{
				this.heldDashFrames++;
			}
		}
		else
		{
			this.heldDashFrames = 0;
		}
	}

	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		if (this.DASH_MAX > 0)
		{
			Color targColor = this.GetDashedColor(drawInfo.colorHair);
			if (this.colorFlashTime > 0f)
			{
				float per = this.colorFlashTime / 15f;
				drawInfo.colorHair.R = (byte)((float)(255 - targColor.R) * per + (float)(int)targColor.R);
				drawInfo.colorHair.G = (byte)((float)(255 - targColor.G) * per + (float)(int)targColor.G);
				drawInfo.colorHair.B = (byte)((float)(255 - targColor.B) * per + (float)(int)targColor.B);
			}
			else
			{
				drawInfo.colorHair = targColor;
			}
		}
	}

	public override void PostUpdateEquips()
	{
		if (this.Dashing || this.Swimming)
		{
			base.Player.canRocket = false;
			base.Player.jump = 1;
		}
		if (base.Player.dashType > 2)
		{
			this.acc_dashCountMax++;
		}
	}

	public override void ResetEffects()
	{
		this.acc_dashCountMax = 0;
		this.acc_dashDist = 0f;
		this.acc_dashSpeed = 0f;
		this.acc_dashCooldown = 0;
		this.acc_dashCooldownMult = 1f;
		this.acc_Color = Color.Transparent;
		this.canRefreshDash = true;
		this.acc_SafeDash = false;
		this.acc_RunDash = false;
		this.acc_ShieldDash = false;
		this.acc_RedCoat = false;
		this.acc_CanMount = true;
		this.Bubbleing = false;
	}

	public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
	{
		if ((!this.acc_SafeDash && !this.acc_ShieldDash) || !this.Dashing)
		{
			return !this.Swimming;
		}
		return false;
	}

	public override bool CanBeHitByProjectile(Projectile proj)
	{
		if (this.acc_SafeDash)
		{
			return !this.Dashing;
		}
		return true;
	}
}
