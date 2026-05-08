using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CTG2.Content.ServerSide;

public class NeatQueueTeamAssignmentSystem : ModSystem
{
    private const int TeamRed = 1;
    private const int TeamBlue = 3;

    private readonly Dictionary<int, DiscordIdentity> _discordIdentityByWhoAmI = new();
    private readonly Dictionary<string, int> _whoAmIByDiscordId = new();
    private readonly Dictionary<string, int> _teamIdByDiscordId = new();

    public sealed class DiscordIdentity
    {
        public string DiscordId;
        public string Username;
        public int WhoAmI;
    }

    public sealed class NeatQueueAssignment
    {
        public string DiscordId;
        public string Name;
        public string Team;
        public int? TeamNum;
    }

    public void RegisterDiscordIdentity(int whoAmI, string discordId, string username)
    {
        if (string.IsNullOrEmpty(discordId))
        {
            Mod.Logger.Warn($"[NeatQueue] RegisterDiscordIdentity called with empty discordId for player {whoAmI}");
            return;
        }

        if (_whoAmIByDiscordId.TryGetValue(discordId, out int previousWhoAmI) && previousWhoAmI != whoAmI)
        {
            _discordIdentityByWhoAmI.Remove(previousWhoAmI);
        }

        var identity = new DiscordIdentity
        {
            DiscordId = discordId,
            Username = username ?? string.Empty,
            WhoAmI = whoAmI
        };

        _discordIdentityByWhoAmI[whoAmI] = identity;
        _whoAmIByDiscordId[discordId] = whoAmI;

        string terrariaName = whoAmI >= 0 && whoAmI < Main.player.Length
            ? Main.player[whoAmI].name
            : "(unknown)";
        Mod.Logger.Info($"[NeatQueue] Registered identity for player {whoAmI} ({terrariaName}) discord_id={discordId}");

        TryAssignByDiscordId(discordId);
    }

    public (int storedCount, int skippedCount) ReplaceAssignments(IEnumerable<NeatQueueAssignment> assignments)
    {
        _teamIdByDiscordId.Clear();

        int stored = 0;
        int skipped = 0;

        if (assignments == null)
            return (0, 0);

        foreach (var assignment in assignments)
        {
            if (assignment == null || string.IsNullOrEmpty(assignment.DiscordId))
            {
                Mod.Logger.Warn("[NeatQueue] Skipping assignment with empty discord_id");
                skipped++;
                continue;
            }

            int? teamId = ResolveTeamId(assignment.Team, assignment.TeamNum);
            if (teamId == null)
            {
                Mod.Logger.Warn($"[NeatQueue] Skipping assignment for discord_id={assignment.DiscordId}: unknown team='{assignment.Team}' team_num={assignment.TeamNum}");
                skipped++;
                continue;
            }

            _teamIdByDiscordId[assignment.DiscordId] = teamId.Value;
            stored++;
        }

        Mod.Logger.Info($"[NeatQueue] Stored {stored} assignments (skipped {skipped} invalid)");
        return (stored, skipped);
    }

    public void ClearAssignments()
    {
        _teamIdByDiscordId.Clear();
        Mod.Logger.Info("[NeatQueue] Cleared assignment map");
    }

    public bool TryAssignByDiscordId(string discordId)
    {
        if (string.IsNullOrEmpty(discordId))
            return false;

        if (!_teamIdByDiscordId.TryGetValue(discordId, out int teamId))
            return false;

        if (!_whoAmIByDiscordId.TryGetValue(discordId, out int whoAmI))
        {
            Mod.Logger.Info($"[NeatQueue] Have assignment for discord_id={discordId} but no online player yet (will assign on connect)");
            return false;
        }

        if (whoAmI < 0 || whoAmI >= Main.player.Length || !Main.player[whoAmI].active)
        {
            // Defensive cleanup of stale identity entries
            _whoAmIByDiscordId.Remove(discordId);
            _discordIdentityByWhoAmI.Remove(whoAmI);
            Mod.Logger.Info($"[NeatQueue] Have assignment for discord_id={discordId} but no online player yet (will assign on connect)");
            return false;
        }

        ModContent.GetInstance<GameManager>().HandlePlayerTeamChange(whoAmI, teamId);

        string terrariaName = Main.player[whoAmI].name;
        string discordUsername = _discordIdentityByWhoAmI.TryGetValue(whoAmI, out var identity) ? identity.Username : "(unknown)";
        string teamName = teamId == TeamBlue ? "blue" : "red";
        Mod.Logger.Info($"[NeatQueue] Auto-assigned {terrariaName} / {discordUsername} / {discordId} to {teamName}");
        return true;
    }

    public int TryAssignAllOnline()
    {
        int assigned = 0;
        // Snapshot keys so the loop is safe if TryAssignByDiscordId mutates _whoAmIByDiscordId via stale-entry cleanup.
        var discordIds = new List<string>(_teamIdByDiscordId.Keys);
        foreach (var discordId in discordIds)
        {
            if (TryAssignByDiscordId(discordId))
                assigned++;
        }
        return assigned;
    }

    private static int? ResolveTeamId(string team, int? teamNum)
    {
        if (!string.IsNullOrWhiteSpace(team))
        {
            switch (team.Trim().ToLowerInvariant())
            {
                case "blue":
                    return TeamBlue;
                case "red":
                    return TeamRed;
            }
        }

        if (teamNum.HasValue)
        {
            switch (teamNum.Value)
            {
                case 0:
                    return TeamBlue;
                case 1:
                    return TeamRed;
            }
        }

        return null;
    }
}
