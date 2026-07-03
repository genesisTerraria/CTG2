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
    public static string mapName = "";
    public static int blueTeamSize = 0;
    public static int redTeamSize = 0;
    public static int matchStartTime = 1800;
    public static int blueCaptures = 0;
    public static int redCaptures = 0;
    public static float blueFurthest = 0;
    public static float redFurthest = 0;
    public static bool paused = false;
    // AbilityID of the class each team is forbidden from picking (0 = no ban).
    // redTeamBannedClassID is set by the BLUE captain and applies to red players, and vice versa.
    public static int redTeamBannedClassID = 0;
    public static int blueTeamBannedClassID = 0;
}
