using CTG2.Content.Configs;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content;

public class DashInputSystem : ModSystem
{
    internal enum DashPacketType : byte
    {
        Request = 0,
        Perform = 1
    }

    public override void Load()
    {
        On_Player.DoCommonDashHandle += VanillaDashDetour;
        On_Player.DashMovement += CustomDashHandle;
    }

    public override void Unload()
    {
        On_Player.DoCommonDashHandle -= VanillaDashDetour;
        On_Player.DashMovement -= CustomDashHandle;
    }

    // Suppress vanilla double-tap dash for the local player only.
    // Other players still run vanilla logic so they animate correctly on your client.
    private static void VanillaDashDetour(
    On_Player.orig_DoCommonDashHandle orig,
    Player self,
    out int dir,
    out bool dashing,
    Player.DashStartAction dashStartAction)
    {
        // Server always runs vanilla for all players
        if (Main.netMode == NetmodeID.Server)
        {
            orig(self, out dir, out dashing, dashStartAction);
            return;
        }

        // For remote players in multiplayer, always suppress vanilla.
        // Their dash state is driven entirely by the keybind packet system.
        if (Main.netMode == NetmodeID.MultiplayerClient && self.whoAmI != Main.myPlayer)
        {
            dir = 0;
            dashing = false;
            return;
        }

        // For the local player, respect the config toggle
        var config = ModContent.GetInstance<CTG2Config>();
        if (config.IsVanillaDashEnabled)
        {
            orig(self, out dir, out dashing, dashStartAction);
            return;
        }

        dir = 0;
        dashing = false;
    }

    private static void CustomDashHandle(On_Player.orig_DashMovement orig, Player self)
    {
        orig(self);
    }

    internal static void HandlePacket(BinaryReader reader, int sender)
    {
        DashPacketType packetType = (DashPacketType)reader.ReadByte();

        switch (packetType)
        {
            case DashPacketType.Request:
                {
                    if (Main.netMode != NetmodeID.Server) break;

                    sbyte direction = reader.ReadSByte();
                    byte dashTypeHint = reader.ReadByte();

                    direction = ClampDirection(direction);
                    if (direction == 0) direction = 1;

                    Player player = Main.player[sender];
                    if (!player.active || player.dead) break;

                    var dashPlayer = player.GetModPlayer<DashInputPlayer>();
                    if (dashPlayer.PerformDash(direction, force: false, out byte dashTypeUsed, dashTypeHint > 0 ? dashTypeHint : (int?)null))
                        dashPlayer.BroadcastDash(direction, dashTypeUsed);

                    break;
                }
            case DashPacketType.Perform:
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient) break;

                    byte playerIndex = reader.ReadByte();
                    sbyte direction = reader.ReadSByte();
                    byte dashTypeUsed = reader.ReadByte();

                    if (playerIndex >= Main.maxPlayers) break;

                    Player remotePlayer = Main.player[playerIndex];
                    if (!remotePlayer.active) break;

                    direction = ClampDirection(direction);
                    if (direction == 0) direction = 1;

                    var dashPlayer = remotePlayer.GetModPlayer<DashInputPlayer>();
                    dashPlayer.PerformDash(direction, force: true, out _, dashTypeUsed > 0 ? dashTypeUsed : (int?)null);
                    break;
                }
        }
    }

    private static sbyte ClampDirection(int direction)
    {
        if (direction > 1) return 1;
        if (direction < -1) return -1;
        return (sbyte)direction;
    }
}