using Microsoft.Xna.Framework;
namespace CTG2.Content.ClientSide;

public static class GameInfo
{
    // 0 = Inactive, 1 = Class Selection, 2 = Game Active
    public static int matchStage = 0;
    public static int matchTime = 0;
    public static int blueGemX = 0;
    public static int redGemX = 0;
    public static string blueGemCarrier = "At Base";
    public static string redGemCarrier = "At Base";
    public static string blueGemCarrierName = "";
    public static string redGemCarrierName = "";
    public static bool overtime = false;
    public static int overtimeTimer = 0;
    public static string mapName = "";
    public static int blueTeamSize = 0;
    public static int redTeamSize = 0;
    public static int matchStartTime = 1800;
    public static int blueAttempts = 0;
    public static int redAttempts = 0;
    public static float blueFurthest = 0;
    public static float redFurthest = 0;
}
