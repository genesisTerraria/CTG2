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
        }
    }
}