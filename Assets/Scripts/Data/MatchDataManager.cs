using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class MatchDataManager : GlobalSingletonBehaviour<MatchDataManager>
{
    public bool Lock = false;

    public MatchMode CurrentMatchMode;
    public MapData CurrentMap;


    private readonly List<MatchPlayer> players = new List<MatchPlayer>();
    private readonly Stack<int> reservedPlayerIds = new Stack<int>();

    private readonly List<MatchTeam> teams = new List<MatchTeam>();
    private readonly Stack<int> reservedTeamIds = new Stack<int>();

    public int TotalPlayerCount => teams.Sum(x => x.PlayerCount);

    public static readonly Dictionary<MatchMode, MatchConfig> MATCH_CONFIGS = new Dictionary<MatchMode, MatchConfig>
    {
        {
            MatchMode.OneOnOne, new MatchConfig
            {
                Mode = MatchMode.OneOnOne,
                Label = "1v1",
                TeamCount = 2,
                TeamSize = 1,
            }
        },
        {
            MatchMode.TwoOnTwo, new MatchConfig
            {
                Mode = MatchMode.TwoOnTwo,
                Label = "2v2",
                TeamCount = 2,
                TeamSize = 2,
            }
        },
        {
            MatchMode.ThreeOnThree, new MatchConfig
            {
                Mode = MatchMode.ThreeOnThree,
                Label = "3v3",
                TeamCount = 2,
                TeamSize = 3,
            }
        },
        {
            MatchMode.FourOnFour, new MatchConfig
            {
                Mode = MatchMode.FourOnFour,
                Label = "4v4",
                TeamCount = 2,
                TeamSize = 4,
            }
        },
        {
            MatchMode.FreeForAll, new MatchConfig
            {
                Mode = MatchMode.FreeForAll,
                Label = "FFA",
                TeamCount = 4,
                TeamSize = 1,
            }
        },
    };

    /// <summary>
    /// Sets the default match mode.
    /// </summary>
    protected override void OnInitialize()
    {
        ApplyMatchMode(MatchMode.OneOnOne);
        AddPlayerToTeam(0);
        AddPlayerToTeam(1);
    }

    public void Reset()
    {
        CurrentMatchMode = MatchMode.OneOnOne;
        CurrentMap = null;

        teams.Clear();
        players.Clear();
    }

    /// <summary>
    /// Applies the given match mode, adjusting team count and trimming excess players.
    /// </summary>
    public void ApplyMatchMode(MatchMode mode)
    {
        CheckLockState();

        CurrentMatchMode = mode;

        var config = MATCH_CONFIGS[mode];
        var teamCount = config.TeamCount;

        if (teams.Count > teamCount)
        {
            for (var i = teams.Count - 1; i >= teamCount; i--)
                RemoveTeam(i);
        }
        else
        {
            for (var i = teams.Count; i < teamCount; i++)
                AddTeam();
        }

        foreach (var teamData in GetTeamDatas())
            SetPlayerSlotCount(teamData.TeamId, config.TeamSize);

        Debug.Log($"[{nameof(MatchDataManager)}] Apply MatchMode = '{mode}' TeamCount: {teams.Count}, TeamSize: {config.TeamSize}");
    }


    #region TeamManagement

    public MatchTeam GetTeamData(int teamId)
        => (teamId < 0 || teamId >= teams.Count) ? null : teams[teamId];

    public IEnumerable<MatchTeam> GetTeamDatas()
        => teams.Where(x => x != null);

    /// <summary>
    /// Creates a new team with one initial player.
    /// </summary>
    public int AddTeam()
    {
        CheckLockState();

        var teamId = -1;
        if (reservedTeamIds.Count > 0)
        {
            teamId = reservedTeamIds.Pop();
        }
        else 
        {
            teamId = teams.Count;
            teams.Add(null);
        }

        var teamData = new MatchTeam
        {
            TeamId = teamId,
            Name = "",
            Color = Color.white,
        };
        teams[teamId] = teamData;

        Debug.Log($"[{nameof(MatchDataManager)}] AddTeam: Team {teamId} created.");

        return teamId;
    }

    /// <summary>
    /// Remove a team from the 
    /// </summary>
    /// <param name="teamId"></param>
    public void RemoveTeam(int teamId)
    {
        CheckLockState();

        var teamData = GetTeamData(teamId);
        if (teamData == null)
            throw new KeyNotFoundException($"Team ID '{teamId}' does not exist.");

        var playerIds = teamData.PlayerIds;
        foreach (var playerId in playerIds.Where(x => x != -1))
            RemovePlayer(playerId);

        reservedTeamIds.Push(teamId);
        teams[teamId] = null;
    }

    /// <summary>
    /// Add a player slot to the team
    /// </summary>
    public int AddPlayerSlot(int teamId)
    {
        CheckLockState();

        var teamData = teams[teamId];
        teamData.PlayerIds.Add(-1); // add empty slot.

        return (teamData.PlayerIds.Count - 1);
    }

    public bool HasEmptyPlayerSlot(int teamId)
        => (GetEmptyPlayerSlotIndex(teamId) != -1);

    public int GetEmptyPlayerSlotIndex(int teamId)
    {
        var teamData = teams[teamId];
        var emptySlotIndex = teamData.PlayerIds.IndexOf(-1);

        return emptySlotIndex;
    }

    public void SetPlayerSlotCount(int teamId, int count)
    {
        CheckLockState();

        var teamData = teams[teamId];
        if (teamData.PlayerIds.Count == count)
            return;

        if (teamData.PlayerIds.Count > count)
        {
            for (var i = teamData.PlayerIds.Count; i > count; i--)
                teamData.PlayerIds.RemoveAt(i - 1); // add an empty slot.
        }
        else
        {
            for (var i = teamData.PlayerIds.Count; i < count; i++)
                teamData.PlayerIds.Add(-1); // add an empty slot.
        }

        Debug.Log($"[{nameof(MatchDataManager)}] Set Team '{teamId}'s slot count to {teamData.PlayerIds.Count}");
    }

    public void RemovePlayerSlot(int teamId, int count = 1)
    {
        CheckLockState();

        var teamData = teams[teamId];
        var lastIndex = teamData.PlayerIds.Count - 1;
        for (var i = 0; i < count; i++)
        {
            var playerId = teamData.PlayerIds[lastIndex];
            if (playerId != -1)
                RemovePlayer(lastIndex);

            lastIndex--;
        }
    }

    public void RemoveEmptyPlayerSlots(int teamId)
    {
        CheckLockState();

        var teamData = teams[teamId];
        for (var i = teamData.PlayerIds.Count - 1; i >= 0; i--)
        {
            if (teamData.PlayerIds[i] != -1)
                continue;

            teamData.PlayerIds.RemoveAt(i);
        }
    }


    #endregion TeamManagement


    #region PlayerManagement

    /// <summary>
    /// Get a player data existing in current match setup
    /// </summary>
    public MatchPlayer GetPlayerData(int playerId)
        => (playerId < 0 || playerId >= players.Count) ? null : players[playerId];

    /// <summary>
    /// Get all player datas existing in current match setup
    /// </summary>
    public IEnumerable<MatchPlayer> GetPlayerDatas()
    {
        var teamDatas = GetTeamDatas();
        return teamDatas.SelectMany(t => t.PlayerIds) // extract all slot data into 1D enumerable.
            .Where(x => x != -1) // Slot is not empty
            .Select(x => players[x]); // get PlayerData
    }

    /// <summary>
    /// Add a player data to the match.
    /// </summary>
    public int AddPlayer(string factionName = "", string heroName = "")
    {
        CheckLockState();

        var playerData = default(MatchPlayer);
        if (reservedPlayerIds.TryPop(out var playerId))
        {
            playerData = players[playerId];
        }
        else
        {
            playerId = players.Count;
            playerData = new MatchPlayer()
            {
                PlayerId = playerId,
            };

            players.Add(playerData);
        }

        // Clear the data.
        playerData.FactionName = (factionName == "") ? FactionDataLoader.Instance.GetRandomFactionName() : factionName;
        playerData.HeroName = (heroName == "") ? FactionDataLoader.Instance.GetRandomHeroName(playerData.FactionName) : heroName;
        playerData.TeamId = -1;
        playerData.Name = "";

        return playerId;
    }

    /// <summary>
    /// Remove a player from the current match
    /// </summary>
    public void RemovePlayer(int playerId)
    {
        CheckLockState();

        if (playerId < 0 || playerId >= players.Count)
            throw new Exception($"Player index '{playerId}' is out of valid range.");

        if (reservedPlayerIds.Contains(playerId))
            throw new Exception($"Player index '{playerId}' is already removed.");

        var playerData = players[playerId];
        var teamData = teams[playerData.TeamId];

        if (playerData.TeamId >= 0 && playerData.TeamId < teams.Count)
            teamData.PlayerIds.Remove(playerId);

        playerData.TeamId = -1;
        reservedPlayerIds.Push(playerId);
    }

    /// <summary>
    /// Creates a new player and assigns it to the specified team.
    /// </summary>
    public int AddPlayerToTeam(int teamId)
    {
        CheckLockState();

        var playerId = AddPlayer();

        var playerData = players[playerId];
        playerData.TeamId = teamId;

        var slotId = GetEmptyPlayerSlotIndex(teamId);
        if (slotId == -1)
            slotId = AddPlayerSlot(teamId);

        var teamData = teams[teamId];
        teamData.PlayerIds[slotId] = playerId;

        return playerId;
    }

    /// <summary>
    /// Moves an existing player from its current team to a new team.
    /// </summary>
    public void MovePlayerToTeam(int playerId, int targetTeamId)
    {
        CheckLockState();

        var playerData = players[playerId];
        if (playerData.TeamId < 0)
            return;

        var srcTeamData = teams[playerData.TeamId];
        var dstTeamData = teams[targetTeamId];

        var srcPlayerIndex = srcTeamData.PlayerIds.IndexOf(playerId);
        var dstPlayerIndex = GetEmptyPlayerSlotIndex(targetTeamId);
        if (dstPlayerIndex == -1)
            dstPlayerIndex = AddPlayerSlot(playerId);

        playerData.TeamId = targetTeamId;

        srcTeamData.PlayerIds[srcPlayerIndex] = -1;
        dstTeamData.PlayerIds[dstPlayerIndex] = playerId;

        Debug.Log($"[{nameof(MatchDataManager)}] MovePlayerToTeam: Player {playerId} moved from Team {srcTeamData.TeamId} to Team {dstTeamData.TeamId}.");
    }

    #endregion PlayerManagement


    /// <summary>
    /// Validates match data readiness. Returns null if valid, or an error message describing the first failure.
    /// </summary>
    public string ValidateMatch()
    {
        if (teams.Count < 2)
            return "At least two teams are required.";

        for (var i = 0; i < teams.Count; i++)
        {
            if (teams[i].PlayerCount == 0)
                return $"Team {i + 1} has no players.";
        }

        var activePlayers = GetPlayerDatas();
        foreach (var playerData in activePlayers)
        {
            if (string.IsNullOrWhiteSpace(playerData.Name))
                return $"Player {playerData.PlayerId + 1} has no name.";
        }

        return null;
    }

    private void CheckLockState()
    {
        if (Lock)
            throw new OperationCanceledException($"[{nameof(MatchDataManager)}] MatchData can not be modified while it is locked.");
    }
}
