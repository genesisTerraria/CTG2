using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace CTG2.Content.Items
{
    public class KingFisher : ModItem
    {
        public override string Texture => "Terraria/Images/Item_2296";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.SittingDucksFishingRod);
            Item.shoot = ModContent.ProjectileType<SittingDuckBobber>();
            Item.shootSpeed = 16f;
        }

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn your custom bobber instead of the vanilla one
            int proj = Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<SittingDuckBobber>(),
                damage, knockback, player.whoAmI);

            return false; // Return false to cancel the vanilla spawn
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(45f, -34f); // adjust to match rod tip
        }
    }

    public class SittingDuckBobber : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_366";

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(366);

            Projectile.damage = 21;
            Projectile.friendly = true;
            Projectile.hostile = false;
        }

        // public override bool? CanHitNPC(NPC target)
        // {
        //     // Only damage during flight phase
        //     return Projectile.ai[0] == 0f ? true : false;
        // }

        public override void AI()
        {
            // Let vanilla bobber AI run first, THEN override what it reset
            base.AI();

            Projectile.damage = 25;
            Projectile.friendly = Projectile.ai[0] == 0f; // Only damage while in flight
        }
    }
}