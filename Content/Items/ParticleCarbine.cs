using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Items
{
    public class ParticleCarbine : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LaserMachinegun;

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Handgun);
            Item.UseSound = SoundID.Item91;
            Item.damage = 28;
            Item.useAmmo = AmmoID.None;
            Item.shootSpeed = 8.5f;
            Item.shoot = ProjectileID.LaserMachinegunLaser;
        }
    }
}