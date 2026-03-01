using Microsoft.Xna.Framework;
using Terraria;
using System.Collections.Generic;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using CTG2.Content.ClientSide;
using Terraria.Audio;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Audio;
using Terraria.ModLoader;

namespace CTG2.Content.ServerSide;

public class Gem
{
    public bool IsHeld { get; set;}
    public int HeldBy {get; set;}
    
    public bool IsCaptured {get; set;}
    
    
    // X, Y Coords for the Top-Left Corner of Gem
    public Vector2 Position {get; private set;}
    public int Width {get;set;}
    public int Height {get;set;}
    private int team;
    public Rectangle GemHitbox {get; private set;}

    public Gem(Vector2 position, int team)
    {
        IsHeld = false;
        HeldBy = -1;
        IsCaptured = false;
        Position = position;
        Width = 6 * 16;
        Height = 9 * 16;
        GemHitbox = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        this.team = team;

    }

    public void Reset()
    {
        IsHeld = false;
        IsCaptured = false;
        HeldBy = -1;
    }
    // Runs all Gem Logic - Check if Gem can be picked up/captured by any players
    public void Update(Gem otherGem, List<Player> otherTeam)
    {
        if (IsHeld)
        {
            //TryCapture(otherGem, Main.player[HeldBy]);
            //TryDrop(Main.player[HeldBy]);
        }
        else
        {
            //TryGetGem(otherTeam);
        }
    }
    
    private void TryGetGem(List<Player> otherTeam)
    {
        foreach (var ply in otherTeam)
        {
            if (!ply.active || ply.dead)
                continue;

            if (ply.Hitbox.Intersects(GemHitbox))
            {
                // IsHeld = true;
                // HeldBy = ply.whoAmI;
                // NetworkText pickUpText = NetworkText.FromLiteral($"{ply.name} has picked up the gem!");
                // Broadcast to everyone
                var mod = ModContent.GetInstance<CTG2>();
                ModPacket packet = mod.GetPacket();
                packet.Write((byte)MessageType.RequestAudio);
                packet.Write("CTG2/Content/ServerSide/GemPickup");
                packet.Send();
                // ChatHelper.BroadcastChatMessage(pickUpText, Color.Aqua);
                break;
            }
        }
    }
    
    private void TryCapture(Gem otherGem, Player carrier)
    {
        PlayerManager playerManager = carrier.GetModPlayer<PlayerManager>();

        if (!carrier.active || carrier.dead) //|| (playerManager.playerState != PlayerManager.PlayerState.Active) || (carrier.team != this.team)
            return;

        if (carrier.Hitbox.Intersects(otherGem.GemHitbox))
        {
            IsCaptured = true;
            IsHeld     = false;
            // Optionally trigger capture event
            NetworkText captureText = NetworkText.FromLiteral($"{carrier.name} has captured the gem!");
            // Broadcast to everyone
            ChatHelper.BroadcastChatMessage(captureText, Color.Aqua);
        }
    }

    private void TryDrop(Player gemHolder)
    {
         PlayerManager playerManager = gemHolder.GetModPlayer<PlayerManager>();
        if (gemHolder.dead) // || (playerManager.playerState != PlayerManager.PlayerState.Active) || (gemHolder.team != this.team)
        {
            // IsHeld = false;
            // NetworkText dropText = NetworkText.FromLiteral($"{gemHolder.name} has dropped the gem!");
            // Broadcast to everyone
            var mod = ModContent.GetInstance<CTG2>();
            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAudio);
            packet.Write("CTG2/Content/ServerSide/GemDrop");
            packet.Send();
            // ChatHelper.BroadcastChatMessage(dropText, Color.Aqua);
        }
        // if (gemHolder.ghost == true)
        // {
        //     IsHeld = false;
        //     NetworkText dropText = NetworkText.FromLiteral($"{gemHolder.name} has dropped the gem!");
        //     // Broadcast to everyone
        //     ChatHelper.BroadcastChatMessage(dropText, Color.Aqua);
        // }
    }
}


// client-sided gem code
public class GemFunctionality : ModPlayer
{
    public override void PreUpdate()
    {
        var mod = ModContent.GetInstance<CTG2>();

        var redGem = ModContent.GetInstance<GameManager>().RedGem;
        var blueGem = ModContent.GetInstance<GameManager>().BlueGem;

        Player player = Main.LocalPlayer;
        PlayerManager playerManager = player.GetModPlayer<PlayerManager>();

        if (player.team == 3 && player.Hitbox.Intersects(redGem.GemHitbox) && !redGem.IsHeld && !player.dead && !player.ghost) //red gem pickup code
        {
            redGem.IsHeld = true;
            redGem.HeldBy = player.whoAmI;

            //need packets to update gem state from client->server side here

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(1);
            packetIsHeld.Write(true);
            packetIsHeld.Send();

            ModPacket packetHeldBy = mod.GetPacket();
            packetHeldBy.Write((byte)MessageType.UpdateHeldBy);
            packetHeldBy.Write(1);
            packetHeldBy.Write(player.whoAmI);
            packetHeldBy.Send();

            ModPacket packetText = mod.GetPacket();
            packetText.Write((byte)MessageType.RequestChatColored);
            packetText.Write($"{player.name} has picked up the red team's gem!");
            packetText.Write(Color.Aqua.PackedValue);
            packetText.Send(); //send chat pickup text

            ModPacket packetAudio = mod.GetPacket();
            packetAudio.Write((byte)MessageType.RequestAudioClientSide);
            packetAudio.Write("CTG2/Content/ServerSide/GemPickup");
            packetAudio.Send(); //send gem pickup audio
        }
        else if (player.team == 1 && player.Hitbox.Intersects(blueGem.GemHitbox) && !blueGem.IsHeld && !player.dead && !player.ghost) //blue gem pickup code
        {
            blueGem.IsHeld = true;
            blueGem.HeldBy = player.whoAmI;
            
            //need packets to update gem state from client->server side here

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(2);
            packetIsHeld.Write(true);
            packetIsHeld.Send();

            ModPacket packetHeldBy = mod.GetPacket();
            packetHeldBy.Write((byte)MessageType.UpdateHeldBy);
            packetHeldBy.Write(2);
            packetHeldBy.Write(player.whoAmI);
            packetHeldBy.Send();

            ModPacket packetText = mod.GetPacket();
            packetText.Write((byte)MessageType.RequestChatColored);
            packetText.Write($"{player.name} has picked up the blue team's gem!");
            packetText.Write(Color.Aqua.PackedValue);
            packetText.Send(); //send chat pickup text

            ModPacket packetAudio = mod.GetPacket();
            packetAudio.Write((byte)MessageType.RequestAudioClientSide);
            packetAudio.Write("CTG2/Content/ServerSide/GemPickup");
            packetAudio.Send(); //send gem pickup audio
        }

        if (redGem.IsHeld && redGem.HeldBy == player.whoAmI && ((player.dead && player.team == 3) || player.ghost))
        {
            redGem.IsHeld = false;

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(1);
            packetIsHeld.Write(false);
            packetIsHeld.Send();

            ModPacket packetText = mod.GetPacket();
            packetText.Write((byte)MessageType.RequestChatColored);
            packetText.Write($"{player.name} has dropped the red team's gem!");
            packetText.Write(Color.Aqua.PackedValue);
            packetText.Send();

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAudioClientSide);
            packet.Write("CTG2/Content/ServerSide/GemDrop");
            packet.Send();
        }
        else if (blueGem.IsHeld && blueGem.HeldBy == player.whoAmI && ((player.dead && player.team == 1) || player.ghost))
        {
            blueGem.IsHeld = false;

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(2);
            packetIsHeld.Write(false);
            packetIsHeld.Send();

            ModPacket packetText = mod.GetPacket();
            packetText.Write((byte)MessageType.RequestChatColored);
            packetText.Write($"{player.name} has dropped the blue team's gem!");
            packetText.Write(Color.Aqua.PackedValue);
            packetText.Send();

            ModPacket packet = mod.GetPacket();
            packet.Write((byte)MessageType.RequestAudioClientSide);
            packet.Write("CTG2/Content/ServerSide/GemDrop");
            packet.Send();
        }

        if (player.team == 3 && player.Hitbox.Intersects(blueGem.GemHitbox) && redGem.HeldBy == player.whoAmI && player.active && !player.dead && !player.ghost && redGem.IsHeld) //red gem capture code
        {
            redGem.IsCaptured = true;
            redGem.IsHeld = false;

            //need packets to update gem state from client->server side here

            ModPacket packetIsCaptured = mod.GetPacket();
            packetIsCaptured.Write((byte)MessageType.UpdateIsCaptured);
            packetIsCaptured.Write(1);
            packetIsCaptured.Write(true);
            packetIsCaptured.Send();

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(1);
            packetIsHeld.Write(false);
            packetIsHeld.Send();

            ModPacket packetCapture = mod.GetPacket();
            packetCapture.Write((byte)MessageType.RequestChatColored);
            packetCapture.Write($"{player.name} has captured the red team's gem!");
            packetCapture.Write(Color.Aqua.PackedValue);
            packetCapture.Send(); //send chat capture text
        }
        else if (player.team == 1 && player.Hitbox.Intersects(redGem.GemHitbox) && blueGem.HeldBy == player.whoAmI && player.active && !player.dead && !player.ghost && blueGem.IsHeld) //red gem capture code
        {
            blueGem.IsCaptured = true;
            blueGem.IsHeld = false;

            //need packets to update gem state from client->server side here

            ModPacket packetIsCaptured = mod.GetPacket();
            packetIsCaptured.Write((byte)MessageType.UpdateIsCaptured);
            packetIsCaptured.Write(2);
            packetIsCaptured.Write(true);
            packetIsCaptured.Send();

            ModPacket packetIsHeld = mod.GetPacket();
            packetIsHeld.Write((byte)MessageType.UpdateIsHeld);
            packetIsHeld.Write(2);
            packetIsHeld.Write(false);
            packetIsHeld.Send();

            ModPacket packetCapture = mod.GetPacket();
            packetCapture.Write((byte)MessageType.RequestChatColored);
            packetCapture.Write($"{player.name} has captured the blue team's gem!");
            packetCapture.Write(Color.Aqua.PackedValue);
            packetCapture.Send(); //send chat capture text
        }
    }
}
