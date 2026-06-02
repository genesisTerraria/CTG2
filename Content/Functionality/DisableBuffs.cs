using Terraria;
using Terraria.ModLoader;
using Terraria.ID;


public class DisableBuffs : ModPlayer
{
    public override void PreUpdate()
    {
        Player.ClearBuff(BuffID.Ichor);
        Player.ClearBuff(BuffID.Poisoned);
        Player.ClearBuff(BuffID.Lovestruck);
        Player.ClearBuff(BuffID.BetsysCurse);
    }
}