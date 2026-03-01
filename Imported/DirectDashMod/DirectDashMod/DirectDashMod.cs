/*
using System.IO;
using DirectDashMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CTG2;

public class CTG2 : Mod
{
	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		byte num = reader.ReadByte();
		byte plyNum = reader.ReadByte();
		Player ply = Main.player[plyNum];
		switch ((PacketType)num)
		{
		case PacketType.DASH:
		{
			DashPlayer3 dashPly = ply.GetModPlayer<DashPlayer3>();
			dashPly.RecieveDash(ply, reader);
			if (Main.netMode == 2)
			{
				dashPly.SendDash(-1, whoAmI);
			}
			break;
		}
		case PacketType.FORCE_JUMP:
			ply.GetModPlayer<WallJumpPlayer>().RecieveForceJump(ply, reader, whoAmI);
			break;
		case PacketType.GRAB_KEYS:
		{
			WallJumpPlayer jumpPly = ply.GetModPlayer<WallJumpPlayer>();
			jumpPly.RecieveGrabKeys(ply, reader);
			if (Main.netMode == 2)
			{
				jumpPly.SendGrabKeys(-1, whoAmI);
			}
			break;
		}
		}
	}
} */
