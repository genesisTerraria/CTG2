using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.GameContent.UI;
using Terraria.GameContent;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using System.Collections.Generic;
using Terraria.Chat;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using CTG2.Content;
using System.Linq;
using System.Security.Policy;
using CTG2.Content.ServerSide;
using CTG2.Content.ClientSide;
using CTG2.Content.Commands.Auth;
using CTG2.Content.GameHooks;

namespace CTG2.Content
{
    public class GameStartCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "start";
        public override string Usage => "/start <practice|real>";
        public override string Description => "Starts a practice or real game instance.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var modPlayer = caller.Player.GetModPlayer<AuthPlayer>();
            if (!modPlayer.IsAdmin)
            {
                caller.Reply("You must be an admin to use this command.", Color.Red);
                return;
            }

            if (args.Length != 1)
            {
                caller.Reply($"Usage: {Usage}", Color.Red);
                return;
            }

            bool realMatch;
            switch (args[0].ToLowerInvariant())
            {
                case "real":
                    realMatch = true;
                    break;
                case "practice":
                    realMatch = false;
                    break;
                default:
                    caller.Reply($"Unknown match type: {args[0]}. Usage: {Usage}", Color.Red);
                    return;
            }

            if (GameInfo.matchStage != 0 && GameInfo.matchStage != 3)
            {
                caller.Reply("You must end the current game to start a new game.", Color.Red);
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket myPacket = Mod.GetPacket();
                myPacket.Write((byte)MessageType.RequestStartGame);
                myPacket.Write(realMatch);
                myPacket.Send();
                return;
            }

            GameManager gameManager = ModContent.GetInstance<GameManager>();
            if (Hooks.BanPhaseActive)
                Hooks.ForceCompleteBanPhase(realMatch);
            else
                gameManager.StartGame(realMatch);
        }
    }
}
