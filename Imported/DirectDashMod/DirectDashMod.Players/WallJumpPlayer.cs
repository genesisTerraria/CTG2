using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using CTG2;

namespace DirectDashMod.Players;

public class WallJumpPlayer : ModPlayer
{
	public const int TILESIZE = 16;

	public const int GRAB_RANGE = 2;

	public const int GRAB_HEIGHT = 24;

	public const float BASE_CLIMB_SPEED = 2f;

	public const int BASE_STAMINA = 300;

	public const int CLIMB_ANIM_RATE = 15;

	public static readonly Rectangle HOLD_FRAME_BODY = new Rectangle(0, 168, 40, 56);

	public static readonly Rectangle HOLD_FRAME_LEGS = new Rectangle(0, 280, 40, 56);

	public static readonly Rectangle HOLD_FRAME_BODY_NONE = new Rectangle(0, 0, 40, 56);

	public static readonly Vector2 BASE_CANCEL_BOOST = new Vector2(1.15f, 0.9f);

	public bool acc_CanWallJump;

	public Vector2 acc_CancelBoost = Vector2.One;

	public bool acc_ChainBounce;

	public int acc_Stamina = 300;

	public bool grabbing;

	public bool old_Grabbing;

	public int grabX;

	public int grabDir;

	public int climbDir;

	public int old_ClimbDir;

	public int grabFrameCounter;

	public int oldPlayerDirection;

	public int stamina;

	public int staminaFlashTime;

	public static ModKeybind grabKey = null;

	public byte heldJumpFrames;

	public bool groundJump;

	public int forceDirFrames;

	public const int FORCE_DIR_FRAME_LEN = 12;

	public bool HasStamina => this.stamina > 0;

	public bool StaminaFlash => this.staminaFlashTime / 2 % 4 == 3;

	public int ShouldWallJump(int range = 8, int rangeVert = 0, float offset = 0f)
	{
		if (base.Player.sliding)
		{
			return 0;
		}
		int i0 = (int)((base.Player.position.X + 2f) / 16f);
		int i1 = (int)((base.Player.position.X + (float)base.Player.width - 2f) / 16f);
		int x0 = (int)((base.Player.position.X - (float)range) / 16f);
		int x1 = (int)((base.Player.position.X + (float)base.Player.width + (float)range) / 16f);
		int num = (int)(base.Player.position.Y / 16f);
		int y1 = (int)((base.Player.position.Y + (float)base.Player.height - 1f) / 16f);
		int n0 = (int)((base.Player.position.Y + (float)base.Player.height - 33f) / 16f);
		int n1 = (int)((base.Player.position.Y + 32f) / 16f);
		bool leftWall = false;
		bool rightWall = false;
		for (int j = num; j <= y1; j++)
		{
			Tile t = Main.tile[i0, j];
			if (!t.HasUnactuatedTile || !Main.tileSolid[t.TileType] || Main.tileSolidTop[t.TileType])
			{
				t = Main.tile[x0, j];
				if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
				{
					if (t.BlockType == BlockType.Solid || t.BlockType == BlockType.SlopeDownRight || t.BlockType == BlockType.SlopeUpRight)
					{
						leftWall = true;
					}
					else if ((t.BlockType == BlockType.SlopeDownLeft || t.BlockType == BlockType.HalfBlock) && j <= n0)
					{
						leftWall = true;
					}
					else if (t.BlockType == BlockType.SlopeUpLeft && j >= n1)
					{
						leftWall = true;
					}
				}
			}
			t = Main.tile[i1, j];
			if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
			{
				continue;
			}
			t = Main.tile[x1, j];
			if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
			{
				if (t.BlockType == BlockType.Solid || t.BlockType == BlockType.SlopeDownLeft || t.BlockType == BlockType.SlopeUpLeft)
				{
					rightWall = true;
				}
				else if ((t.BlockType == BlockType.SlopeDownRight || t.BlockType == BlockType.HalfBlock) && j <= n0)
				{
					rightWall = true;
				}
				else if (t.BlockType == BlockType.SlopeUpRight && j >= n1)
				{
					rightWall = true;
				}
			}
		}
		bool pref = base.Player.position.X / 16f % 1f < 0.5f;
		if (leftWall || rightWall)
		{
			if ((leftWall && !pref) || (rightWall && pref))
			{
				if (!pref)
				{
					return 1;
				}
				return -1;
			}
			if (!rightWall)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public void MakeForceJumpParticles(int wallNorm)
	{
		float dustPos = (int)((base.Player.Center.X + (float)(base.Player.width / 2 * -wallNorm) + (float)(15 * (1 - wallNorm) / 2)) / 16f) * 16;
		for (int I = 0; I < 8; I++)
		{
			Vector2 position = new Vector2(dustPos, base.Player.position.Y + (float)base.Player.height * Main.rand.NextFloat());
			Vector2? velocity = new Vector2(base.Player.velocity.X * (-0.05f + 0.1f * Main.rand.NextFloat()), (0.5f - Main.rand.NextFloat()) * 4f);
			float scale = 0.6f + 0.8f * Main.rand.NextFloat();
			Dust.NewDustPerfect(position, 16, velocity, 0, default(Color), scale);
		}
	}

	public void ForceJump(int wallNorm = 0, bool bounceJump = false, bool climbJump = false, bool send = true)
	{
		this.heldJumpFrames = 5;
		base.Player.GetModPlayer<DashPlayer3>().StopDash();
		if (!bounceJump)
		{
			base.Player.jump = Player.jumpHeight;
		}
		else
		{
			base.Player.velocity.Y *= this.acc_CancelBoost.Y;
		}
		base.Player.velocity.Y -= Player.jumpSpeed + base.Player.jumpSpeedBoost;
		base.Player.justJumped = true;
		base.Player.rocketRelease = false;
		this.grabDir = 0;
		bool directed = false;
		if (wallNorm != 0)
		{
			base.Player.velocity.X = (Player.jumpSpeed + base.Player.jumpSpeedBoost) * (float)wallNorm;
			if (base.Player.controlLeft || base.Player.controlRight)
			{
				if (wallNorm > 0)
				{
					this.forceDirFrames = 12;
				}
				else
				{
					this.forceDirFrames = -12;
				}
				directed = true;
			}
			else
			{
				base.Player.velocity.X /= 2f;
			}
			this.MakeForceJumpParticles(wallNorm);
		}
		DashPlayer3.ResetFallDamage(base.Player);
		if (send && Main.myPlayer == base.Player.whoAmI && Main.netMode == 1)
		{
			this.SendForceJump(-1, Main.myPlayer, wallNorm, bounceJump, climbJump, directed);
		}
	}

	public void SendForceJump(int toWho, int fromWho, int wallNorm, bool bounceJump, bool climbJump, bool directed)
	{
		ModPacket packet = base.Mod.GetPacket();
		packet.Write((byte)MessageType.FORCE_JUMP);
		packet.Write((byte)base.Player.whoAmI);
		packet.WriteVector2(base.Player.velocity);
		packet.Write((byte)((wallNorm + 1) | (bounceJump ? 4 : 0) | (climbJump ? 8 : 0) | (directed ? 16 : 0)));
		packet.Send(toWho, fromWho);
	}

	public void RecieveForceJump(Player ply, BinaryReader reader, int whoAmI = 0)
	{
		ply.velocity = reader.ReadVector2();
		byte num = reader.ReadByte();
		int wallNorm = (num & 3) - 1;
		bool bounceJump = (num & 4) != 0;
		bool climbJump = (num & 8) != 0;
		bool directed = (num & 0x10) != 0;
		base.Player.GetModPlayer<DashPlayer3>().StopDash();
		if (!bounceJump)
		{
			base.Player.jump = Player.jumpHeight;
		}
		base.Player.justJumped = true;
		base.Player.rocketRelease = false;
		this.grabDir = 0;
		if (wallNorm != 0)
		{
			if (directed)
			{
				if (wallNorm > 0)
				{
					this.forceDirFrames = 12;
				}
				else
				{
					this.forceDirFrames = -12;
				}
			}
			this.MakeForceJumpParticles(wallNorm);
		}
		if (climbJump)
		{
			this.stamina -= 60;
		}
		DashPlayer3.ResetFallDamage(base.Player);
		if (Main.netMode == 2)
		{
			this.SendForceJump(-1, whoAmI, wallNorm, bounceJump, climbJump, directed);
		}
	}

	public void ClimbJump()
	{
		this.ForceJump(0, bounceJump: false, climbJump: true);
		this.stamina -= 60;
	}

	public override void Load()
	{
		WallJumpPlayer.grabKey = KeybindLoader.RegisterKeybind(base.Mod, "Grab", "F");
	}

	public override void Unload()
	{
		WallJumpPlayer.grabKey = null;
	}

	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		DashPlayer3 dashPlayer = base.Player.GetModPlayer<DashPlayer3>();
		this.grabbing = WallJumpPlayer.grabKey.Current && this.acc_CanWallJump && !base.Player.mount.Active;
		if (this.heldJumpFrames == 0 && !dashPlayer.Dashing && base.Player.velocity.Y == 0f && triggersSet.Jump)
		{
			this.groundJump = true;
		}
		if (this.acc_CanWallJump && !base.Player.mount.Active && triggersSet.Jump && this.heldJumpFrames < 5)
		{
			if (dashPlayer.Dashing && base.Player.velocity.Y == 0f)
			{
				this.ForceJump();
				base.Player.velocity.X *= this.acc_CancelBoost.X;
				if (this.acc_ChainBounce && base.Player.TryGetModPlayer<DashPlayer3>(out var dashPly) && dashPly.dashCooldown == 0)
				{
					dashPly.dashCount = 0;
				}
			}
			else if (this.grabbing && this.grabDir != 0)
			{
				if (this.oldPlayerDirection == -this.grabDir || !this.HasStamina)
				{
					this.ForceJump(-this.grabDir);
				}
				else
				{
					this.ClimbJump();
				}
			}
			else if ((dashPlayer.Dashing || this.heldJumpFrames == 0) && !this.groundJump && base.Player.velocity.Y != 0f)
			{
				int wallJump = this.ShouldWallJump();
				if (wallJump != 0)
				{
					if (!dashPlayer.Dashing && this.grabbing && base.Player.direction == -wallJump && this.HasStamina)
					{
						this.ClimbJump();
					}
					else
					{
						this.ForceJump(wallJump, dashPlayer.Dashing);
					}
				}
			}
		}
		if (triggersSet.Up)
		{
			this.climbDir = -1;
		}
		else if (triggersSet.Down)
		{
			this.climbDir = 1;
		}
		else
		{
			this.climbDir = 0;
		}
		if (triggersSet.Jump)
		{
			if (this.heldJumpFrames < 5)
			{
				this.heldJumpFrames++;
			}
		}
		else
		{
			this.heldJumpFrames = 0;
			this.groundJump = false;
		}
		if (Main.netMode == 1 && Main.myPlayer == base.Player.whoAmI && (this.old_Grabbing != this.grabbing || (this.grabbing && this.old_ClimbDir != this.climbDir)))
		{
			this.SendGrabKeys(-1, Main.myPlayer);
		}
		this.old_Grabbing = this.grabbing;
		this.old_ClimbDir = this.climbDir;
		if (this.forceDirFrames != 0)
		{
			if (this.forceDirFrames > 0)
			{
				this.forceDirFrames--;
			}
			else
			{
				this.forceDirFrames++;
			}
			base.Player.controlRight = this.forceDirFrames > 0;
			base.Player.controlLeft = !base.Player.controlRight;
		}
	}

	public void SendGrabKeys(int toWho, int fromWho)
	{
		ModPacket packet = base.Mod.GetPacket();
		packet.Write((byte)MessageType.GRAB_KEYS);
		packet.Write((byte)base.Player.whoAmI);
		byte bools = (byte)((this.climbDir + 1) | (this.grabbing ? 4 : 0));
		packet.Write(bools);
		packet.Send(toWho, fromWho);
	}

	public void RecieveGrabKeys(Player ply, BinaryReader reader)
	{
		byte num = reader.ReadByte();
		int clmDir = (num & 3) - 1;
		bool grabd = (num & 4) != 0;
		this.grabbing = grabd;
		this.climbDir = clmDir;
		this.old_Grabbing = this.grabbing;
		this.old_ClimbDir = this.climbDir;
	}

	public override bool CanStartExtraJump(ExtraJump jump)
	{
		return this.grabDir == 0;
	}

	public override void PreUpdateMovement()
	{
		if (this.grabDir == 0)
		{
			return;
		}
		base.Player.velocity = Vector2.Zero;
		if (this.HasStamina)
		{
			if (this.climbDir != 0)
			{
				if (this.climbDir < 0)
				{
					float climbSpeed = -2f * base.Player.moveSpeed;
					if (this.ShouldWallJump(2, 24, climbSpeed) == -this.grabDir)
					{
						base.Player.velocity.Y = climbSpeed;
					}
				}
				else
				{
					base.Player.velocity.Y = base.Player.maxFallSpeed / 2f * base.Player.moveSpeed;
					if (this.ShouldWallJump(2, 24) == 0)
					{
						this.grabDir = 0;
					}
				}
				this.stamina--;
			}
			this.stamina--;
		}
		else
		{
			if (this.climbDir > 0)
			{
				base.Player.velocity.Y = base.Player.maxFallSpeed / 2f * base.Player.moveSpeed;
			}
			else
			{
				base.Player.velocity.Y = base.Player.maxFallSpeed / 4f;
			}
			if (this.ShouldWallJump(2, 24) == 0)
			{
				this.grabDir = 0;
			}
		}
	}

	public override void PostUpdate()
	{
		if (!this.grabbing)
		{
			this.grabDir = 0;
		}
		if (this.grabbing)
		{
			if (this.grabDir == 0 && base.Player.velocity.Y >= 0f && this.ShouldWallJump(2, 24) == -base.Player.direction)
			{
				this.grabDir = base.Player.direction;
				this.grabX = (int)((base.Player.Center.X + (float)(base.Player.width / 2 * base.Player.direction) + (float)(15 * (1 + base.Player.direction) / 2)) / 16f) * 16;
				base.Player.position.X = this.grabX - base.Player.width * (1 + base.Player.direction) / 2;
			}
			if (this.grabDir != 0)
			{
				if (this.HasStamina && this.climbDir < 0 && base.Player.velocity.Y < 0f)
				{
					this.grabFrameCounter++;
					if (Main.rand.NextBool(4))
					{
						Vector2 position = new Vector2(this.grabX + 2, base.Player.position.Y + (float)base.Player.height * (0.2f + 0.6f * Main.rand.NextFloat()));
						Vector2? velocity = new Vector2(-0.05f + 0.1f * Main.rand.NextFloat(), (0.5f - Main.rand.NextFloat()) * 1f);
						float scale = 0.4f + 0.6f * Main.rand.NextFloat();
						Dust.NewDustPerfect(position, 16, velocity, 0, default(Color), scale);
					}
				}
				else
				{
					this.grabFrameCounter = 0;
					if ((this.climbDir > 0 || (!this.HasStamina && Main.rand.NextBool())) && base.Player.velocity.Y > 0f)
					{
						Vector2 position2 = new Vector2(this.grabX + 2, base.Player.position.Y + (float)base.Player.height * (0.25f + 0.5f * Main.rand.NextFloat()));
						Vector2? velocity2 = new Vector2(-0.05f + 0.1f * Main.rand.NextFloat(), (0.5f - Main.rand.NextFloat()) * 1f);
						float scale = 0.4f + 0.6f * Main.rand.NextFloat();
						Dust.NewDustPerfect(position2, 16, velocity2, 0, default(Color), scale);
					}
				}
			}
		}
		if (this.grabDir != 0)
		{
			if (base.Player.bodyFrame == WallJumpPlayer.HOLD_FRAME_BODY_NONE || base.Player.bodyFrame == WallJumpPlayer.HOLD_FRAME_LEGS)
			{
				base.Player.bodyFrame = ((this.grabFrameCounter / 15 % 2 == 0) ? WallJumpPlayer.HOLD_FRAME_BODY : WallJumpPlayer.HOLD_FRAME_LEGS);
			}
			base.Player.legFrame = WallJumpPlayer.HOLD_FRAME_LEGS;
			this.oldPlayerDirection = base.Player.direction;
			base.Player.direction = this.grabDir;
		}
		if (this.grabDir != 0)
		{
			DashPlayer3.ResetFallDamage(base.Player);
		}
		if (base.Player.velocity.Y == 0f && this.grabDir == 0)
		{
			this.stamina = this.acc_Stamina;
		}
		if (!this.HasStamina)
		{
			this.staminaFlashTime++;
		}
		else
		{
			this.staminaFlashTime = 0;
		}
	}

	public override void PostUpdateEquips()
	{
		if (base.Player.GetModPlayer<DashPlayer3>().Swimming)
		{
			this.acc_CanWallJump = false;
		}
		if (this.acc_CanWallJump)
		{
			if (base.Player.spikedBoots == 1)
			{
				this.acc_Stamina *= 3;
			}
			else if (base.Player.spikedBoots == 2)
			{
				this.acc_Stamina = 999999999;
			}
			base.Player.spikedBoots = 0;
		}
		if (this.grabDir != 0)
		{
			base.Player.canRocket = false;
		}
	}

	public override void UpdateEquips()
	{
		if (this.grabDir != 0)
		{
			base.Player.portableStoolInfo.Reset();
		}
	}

	public override void ResetEffects()
	{
		this.acc_CanWallJump = false;
		this.acc_CancelBoost = Vector2.One;
		this.acc_ChainBounce = false;
		this.acc_Stamina = 300;
	}

	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		base.ModifyDrawInfo(ref drawInfo);
	}
}
