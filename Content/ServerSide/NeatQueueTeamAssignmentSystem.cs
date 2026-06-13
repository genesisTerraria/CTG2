using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private readonly Dictionary<string, QueueRosterEntry> _rosterByDiscordId = new();

    public bool PlayersReady { get; private set; }

    // Fingerprint of the current rosters discord ids. Prevents a duplicate /neatqueue/teams
    // call with the same players from resetting PlayersReady or re-firing the arrival broadcast
    private string _currentRosterFingerprint = string.Empty;

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

    public sealed class QueueRosterEntry
    {
        public string DiscordId;
        public string Username;
        public string Team;
        public int TeamId;
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

        // The final queued player identifying may complete the roster here
        TryMarkPlayersReady();
    }

    public (int storedCount, int skippedCount) ReplaceAssignments(IEnumerable<NeatQueueAssignment> assignments)
    {
        // Materialize once so we can both fingerprint and iterate without re-enumerating a lazy source.
        var assignmentList = assignments?.ToList() ?? new List<NeatQueueAssignment>();

        // A roster only counts as new when its set of Discord IDs has changed. A duplicate
        // /neatqueue/teams call with the same players keeps PlayersReady latched
        string newFingerprint = BuildRosterFingerprint(assignmentList);
        bool isNewRoster = newFingerprint != _currentRosterFingerprint;

        if (isNewRoster)
        {
            _currentRosterFingerprint = newFingerprint;
            PlayersReady = false;
            Mod.Logger.Info("[NeatQueue] New roster detected; PlayersReady reset to false");
        }

        _teamIdByDiscordId.Clear();
        _rosterByDiscordId.Clear();

        int stored = 0;
        int skipped = 0;

        foreach (var assignment in assignmentList)
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

            _rosterByDiscordId[assignment.DiscordId] = new QueueRosterEntry
            {
                DiscordId = assignment.DiscordId,
                Username = assignment.Name ?? string.Empty,
                Team = string.IsNullOrWhiteSpace(assignment.Team)
                    ? TeamIdToTeamName(teamId.Value)
                    : assignment.Team.Trim().ToLowerInvariant(),
                TeamId = teamId.Value
            };

            stored++;
        }

        Mod.Logger.Info($"[NeatQueue] Stored {stored} assignments (skipped {skipped} invalid)");

        // Players may already all be in the world before the roster arrives, so evaluate readiness now.
        TryMarkPlayersReady();

        return (stored, skipped);
    }

    public void ClearAssignments()
    {
        _teamIdByDiscordId.Clear();
        _rosterByDiscordId.Clear();
        PlayersReady = false;
        _currentRosterFingerprint = string.Empty;
        Mod.Logger.Info("[NeatQueue] Cleared assignment map");
    }

    public bool TryGetDiscordUsername(int whoAmI, out string username)
    {
        username = string.Empty;

        if (whoAmI < 0 || whoAmI >= Main.player.Length || !Main.player[whoAmI].active)
        {
            if (_discordIdentityByWhoAmI.TryGetValue(whoAmI, out DiscordIdentity staleIdentity))
            {
                _whoAmIByDiscordId.Remove(staleIdentity.DiscordId);
            }

            _discordIdentityByWhoAmI.Remove(whoAmI);
            return false;
        }

        if (!_discordIdentityByWhoAmI.TryGetValue(whoAmI, out DiscordIdentity identity))
            return false;

        username = identity.Username;
        return !string.IsNullOrWhiteSpace(username);
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
        Console.WriteLine($"[NeatQueue] Player team changed by DISCORD IDENTITY: {terrariaName} / {discordUsername} / {discordId} to {teamName}");
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

    public int GetRosterCount()
    {
        return _rosterByDiscordId.Count;
    }

    public IReadOnlyList<QueueRosterEntry> GetMissingPlayers()
    {
        CleanupStaleIdentities();

        List<QueueRosterEntry> missing = new();

        foreach (var entry in _rosterByDiscordId.Values)
        {
            if (!IsDiscordIdOnline(entry.DiscordId))
                missing.Add(entry);
        }

        return missing;
    }

    // Builds report for the /missing command
    public string BuildMissingReport()
    {
        int rosterCount = GetRosterCount();
        if (rosterCount == 0)
            return "No active NeatQueue scrim roster is loaded.";

        IReadOnlyList<QueueRosterEntry> missing = GetMissingPlayers();
        if (missing.Count == 0)
            return $"All {rosterCount} queued players are in the world.";

        StringBuilder builder = new();
        builder.Append($"Missing {missing.Count}/{rosterCount} queued players:");

        foreach (QueueRosterEntry entry in missing)
        {
            string displayName = string.IsNullOrWhiteSpace(entry.Username) ? entry.DiscordId : entry.Username;
            builder.Append($"\n- {displayName} — {entry.Team}");
        }

        return builder.ToString();
    }

    private bool IsDiscordIdOnline(string discordId)
    {
        if (string.IsNullOrEmpty(discordId))
            return false;

        if (!_whoAmIByDiscordId.TryGetValue(discordId, out int whoAmI))
            return false;

        if (whoAmI < 0 || whoAmI >= Main.player.Length || !Main.player[whoAmI].active)
            return false;

        return true;
    }
    //Once a scrim match ends we need to clear up their identities
    private void CleanupStaleIdentities()
    {
        List<int> staleWhoAmIs = new();

        foreach (var pair in _discordIdentityByWhoAmI)
        {
            int whoAmI = pair.Key;

            if (whoAmI < 0 || whoAmI >= Main.player.Length || !Main.player[whoAmI].active)
                staleWhoAmIs.Add(whoAmI);
        }

        foreach (int whoAmI in staleWhoAmIs)
        {
            if (_discordIdentityByWhoAmI.TryGetValue(whoAmI, out var identity))
                _whoAmIByDiscordId.Remove(identity.DiscordId);

            _discordIdentityByWhoAmI.Remove(whoAmI);
        }
    }

    public void UnregisterWhoAmI(int whoAmI)
    {
        if (_discordIdentityByWhoAmI.TryGetValue(whoAmI, out var identity))
        {
            _discordIdentityByWhoAmI.Remove(whoAmI);

            if (_whoAmIByDiscordId.TryGetValue(identity.DiscordId, out int mappedWhoAmI)
                && mappedWhoAmI == whoAmI)
            {
                _whoAmIByDiscordId.Remove(identity.DiscordId);
            }
        }
        // Intentionally does NOT touch PlayersReady: the readiness latch is sticky once set,
        // and before it fires a disconnect naturally drops this id from AreAllRosterPlayersCurrentlyOnline()
    }

    // true only when every Discord ID in the current roster is online at the same time
    private bool AreAllRosterPlayersCurrentlyOnline()
    {
        CleanupStaleIdentities();

        if (_rosterByDiscordId.Count == 0)
            return false;

        foreach (string discordId in _rosterByDiscordId.Keys)
        {
            if (!IsDiscordIdOnline(discordId))
                return false;
        }

        return true;
    }

    // once the whole roster is simultaneously present, PlayersReady stays true
    // (across later disconnects/rejoins) until the roster is cleared or genuinely replaced
    // This function is where the fullrosterjoined hook is called and matchstart commences
    private void TryMarkPlayersReady()
    {
        if (PlayersReady)
            return;

        if (!AreAllRosterPlayersCurrentlyOnline())
            return;

        PlayersReady = true;
        Hooks.OnFullRosterJoined();
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

    private static string TeamIdToTeamName(int teamId)
    {
        return teamId switch
        {
            TeamBlue => "blue",
            TeamRed => "red",
            _ => $"team {teamId}"
        };
    }

    // Stable fingerprint of the rosters Discord ID set so the same set of
    // players yields the same string regardless of order, team, or username
    private static string BuildRosterFingerprint(IEnumerable<NeatQueueAssignment> assignments)
    {
        return string.Join("|",
            assignments
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.DiscordId))
                .Select(a => a.DiscordId.Trim())
                .OrderBy(id => id, StringComparer.Ordinal));
    }
}
