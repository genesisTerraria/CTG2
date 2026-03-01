using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using CTG2.Content;

public class GemLogic : ModPlayer
{
    /*
    public bool hasRedGem = false;
    public bool hasBlueGem = false;
    bool firstMessage = true;
    private int fireworkTimer = 0;
    private int fireworkTimerBlue = 0;
    public static Player redGemHolder = null;
    public static Player blueGemHolder = null;
    
    
    
    public override void PostUpdate()
    {
        int minX = (int)(Player.Hitbox.Left / 16);
        int maxX = (int)(Player.Hitbox.Right / 16);
        int minY = (int)(Player.Hitbox.Top / 16);
        int maxY = (int)(Player.Hitbox.Bottom / 16);


        bool wasHoldingGem = hasRedGem; 
        bool nowTouchingGem = false;
        bool touchingBlueGem = false;

    
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Tile tile = Framing.GetTileSafely(x, y);

                if (Player.team == 3 && !Player.dead)
                {

                    bool isCrystal = tile.HasTile && tile.TileType == TileID.CrystalBlock;
                    bool isRed = tile.TileColor == PaintID.DeepRedPaint;
                    bool isActuated = tile.IsActuated;

                    if (isCrystal && isRed && isActuated)
                    {
                        nowTouchingGem = true;
                    }
                }

                if (Player.team == 1 && !Player.dead) // Red team
                {
                    bool isCrystal = tile.HasTile && tile.TileType == TileID.CrystalBlock;
                    bool isBlue = tile.TileColor == PaintID.DeepBluePaint; 
                    bool isActuated = tile.IsActuated;

                    if (isCrystal && isBlue && isActuated)
                    {
                        touchingBlueGem = true;
                    }
                }
            }
        }


        if (nowTouchingGem && !hasRedGem && redGemHolder == null)
        {
            hasRedGem = true;
            redGemHolder = Player;
            Main.NewText($"{Player.name} has picked up the RED gem!", Color.Red);
            fireworkTimer = 0;
        }

        if (Player.dead && hasRedGem && redGemHolder == Player)
        {
            hasRedGem = false;
            fireworkTimer = 0;
            redGemHolder = null;
        }






        if (hasRedGem)
        {
            fireworkTimer++;
            if (fireworkTimer >= 240)
            {
                fireworkTimer = 0;
                Projectile.NewProjectile(
                Player.GetSource_Misc("GemEffect"),
                Player.Center,
                new Vector2(Main.rand.NextFloat(-2f, 2f), -5f),
                ProjectileID.RocketFireworkRed,
                20, 0f, Player.whoAmI
);

            }
        }

        if (touchingBlueGem && !hasBlueGem && blueGemHolder == null)
        {
            hasBlueGem = true;
            fireworkTimerBlue = 0;
            blueGemHolder = Player;
            Main.NewText($"{Player.name} has picked up the BLUE gem!", Color.Blue);
        }

        if (Player.dead && hasBlueGem && blueGemHolder == Player)
        {
            hasBlueGem = false;
            blueGemHolder = null;
            fireworkTimer = 0;
        }

        if (hasBlueGem)
        {
            fireworkTimerBlue++;
            if (fireworkTimerBlue >= 240)
            {
                fireworkTimerBlue = 0;
                Projectile.NewProjectile(
                    Player.GetSource_Misc("GemEffectBlue"),
                    Player.Center,
                    new Vector2(Main.rand.NextFloat(-2f, 2f), -5f),
                    ProjectileID.RocketFireworkBlue,
                    20, 0f, Player.whoAmI
                );
            }
        }

        if (hasRedGem && Player.team == 3) 
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && tile.TileType == TileID.CrystalBlock &&
                        (tile.TileColor == PaintID.BluePaint || tile.TileColor == PaintID.DeepBluePaint) &&
                        tile.IsActuated)
                    {
                        EndGame("Blue Team Wins!");
                        return;
                    }
                }
            }
        }

        if (hasBlueGem && Player.team == 1) 
        {
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);

                    if (tile.HasTile && tile.TileType == TileID.CrystalBlock &&
                        (tile.TileColor == PaintID.RedPaint || tile.TileColor == PaintID.DeepRedPaint) &&
                        tile.IsActuated)
                    {
                        EndGame("Red Team Wins!");
                        return;
                    }
                }
            }
        }


        else
        {
            fireworkTimerBlue = 0;
        }
    }
    
    
    private void EndGame(string message)
{
    GameUI.matchStarted = false;
    GameUI.matchPrep = false;
    GameUI.matchTimer = 0;

    GemLogic.redGemHolder = null;
    GemLogic.blueGemHolder = null;

    hasRedGem = false;
    hasBlueGem = false;
    fireworkTimer = 0;
    fireworkTimerBlue = 0;

    Main.NewText(message, Color.GreenYellow);
}

    */
}
