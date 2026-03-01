using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CTG2.Content.Commands
{
    public class DisableRollCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "roll";

        public override string Description => null;
        public override void Action(CommandCaller caller, string input, string[] args)
        {
        }
    }


    public class DisableAllDeath : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "alldeath";

        public override string Description => null;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
        }
    }


    public class DisableMeCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "me";

        public override string Description => null;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
        }
    }
    

    
    public class DisableDeathCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "death";

        public override string Description => null;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
        }
    }
}
