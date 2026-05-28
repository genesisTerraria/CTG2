using System.Collections.Generic;

namespace CTG2.Content.ClientSide
{
    public static class DamageBoardData
    {
        public class DamageBoardEntry
        {
            public string Name;
            public int Team;
            public int Kills;
            public int Deaths;
            public int Damage;
        }
        public static readonly List<DamageBoardEntry> Players = new();
        public static bool Visible = false;
    }
}