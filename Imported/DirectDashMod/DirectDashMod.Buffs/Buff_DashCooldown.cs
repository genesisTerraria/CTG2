using DirectDashMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Imported.DirectDashMod.Buffs;

public class Buff_DashCooldown : ModBuff
{
	public override void SetStaticDefaults()
	{
		Main.debuff[base.Type] = true;
		Main.buffNoSave[base.Type] = true;
		Main.buffNoTimeDisplay[base.Type] = false;
		BuffID.Sets.NurseCannotRemoveDebuff[base.Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)
	{
		if (player.TryGetModPlayer<DashPlayer3>(out var dash))
		{
			dash.canRefreshDash = false;
		}
	}
}
