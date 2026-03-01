using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Microsoft.Xna.Framework; 


public class NoThrowing : ModPlayer
{
    public override void PreUpdate()
    {
        if (PlayerInput.Triggers.Current.Throw)
        {
            PlayerInput.Triggers.Current.Throw = false;
            Main.NewText("Throwing items is blocked!", Color.Red);
        }
    }
}