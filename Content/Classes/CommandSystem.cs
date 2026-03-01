using CTG2;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using CTG2.Content;
using ClassesNamespace;
using CTG2.Content.ClientSide;


public class ClassCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "class";
    public override string Description => "Select a player class";

    public override void Action(CommandCaller caller, string input, string[] args)
    {   
        if (GameInfo.matchStage != 1) //!CTG2.Content.Game.preparationPhase
        {
            caller.Reply("You can only select a class during class selection!", Color.Red);
            return;
        }

        if (args.Length < 1 || !int.TryParse(args[0], out int classType))
        {
            caller.Reply("Usage: /class [number]", Color.Red);
            return;
        }

        Player player = caller.Player;
        var modPlayer = player.GetModPlayer<ClassSystem>();

        GameClass classPick = (GameClass)classType;
        
        caller.Reply($"You selected {classPick.ToString()}.", Color.Green);
        modPlayer.playerClass = classPick;
    }
}
