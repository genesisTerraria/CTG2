using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.Functionality
{
    public class MudHoneyCraft : ModSystem
    {
        public override void AddRecipes()
        {
            // Create a new recipe for 1 Mud Block
            Recipe recipe = Recipe.Create(ItemID.MudBlock, 1);
            recipe.AddIngredient(ItemID.DirtBlock, 1);
            recipe.AddCondition(Condition.NearHoney);
            recipe.Register();

            Recipe recipeMud = Recipe.Create(ItemID.DirtBlock, 1);
            recipeMud.AddIngredient(ItemID.MudBlock, 1);
            recipeMud.Register();

            Recipe recipeFish2 = Recipe.Create(ItemID.BombFish, 1);
            recipeFish2.AddIngredient(ItemID.AtlanticCod, 2);
            recipeFish2.Register();
        }
    }
}