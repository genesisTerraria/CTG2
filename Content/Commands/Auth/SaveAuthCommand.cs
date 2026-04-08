// using Terraria;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework;
// using CTG2.Content.Commands.Auth;

// public class SaveAuthCommand : ModCommand
// {
//     public override CommandType Type => CommandType.Chat;
//     public override string Command => "saveauth";
//     public override string Description => "Force save auth data.";
//     public override string Usage => "/saveauth";

//     public override void Action(CommandCaller caller, string input, string[] args)
//     {
//         AuthSystem.Save();
//         caller.Reply($"Saved {AuthSystem.Data.Users.Count} users to: {AuthSystem.FilePath}", Color.Green);
//     }
// }