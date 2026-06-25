using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchDataManager : SingletonBehaviour<MatchDataManager>
{
    public MatchMode CurrentMatchMode;
    public MapData CurrentMap;

    public readonly List<MatchTeam> Teams = new List<MatchTeam>();
    public readonly List<MatchPlayer> Players = new List<MatchPlayer>();

    private readonly Stack<int> reservedPlayerIndices = new Stack<int>();

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
        CurrentMatchMode = MatchMode.OneOnOne;
    }

    public void Reset()
    {
        CurrentMatchMode = MatchMode.OneOnOne;
        CurrentMap = null;

        Teams.Clear();
        Players.Clear();
    }

    public int AddPlayer()
    {
        var player = default(MatchPlayer);
        if (reservedPlayerIndices.TryPop(out var playerIndex))
        {
            player = Players[playerIndex];
        }
        else
        {
            playerIndex = Players.Count;
            player = new MatchPlayer()
            {
                PlayerIndex = playerIndex,
            };

            Players.Add(player);
        }

        // Clear the data.
        player.FactionName = FactionDataLoader.Instance.GetRandomFactionName();
        player.HeroName = FactionDataLoader.Instance.GetRandomHeroName(player.FactionName);
        player.TeamIndex = -1;
        player.Name = "";

        return playerIndex;
    }

    public void RemovePlayer(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Players.Count)
            throw new Exception($"Player index '{playerIndex}' is out of valid range.");

        if (reservedPlayerIndices.Contains(playerIndex))
            throw new Exception($"Player index '{playerIndex}' is already removed.");

        var player = Players[playerIndex];

        if (player.TeamIndex >= 0 && player.TeamIndex < Teams.Count)
            Teams[player.TeamIndex].PlayerIndices.Remove(playerIndex);

        player.TeamIndex = -1;
        reservedPlayerIndices.Push(playerIndex);
    }

    /// <summary>
    /// Creates a new player and assigns it to the specified team.
    /// </summary>
    public int AddPlayerToTeam(int teamIndex)
    {
        var playerIndex = AddPlayer();
        var player = Players[playerIndex];

        player.TeamIndex = teamIndex;
        Teams[teamIndex].PlayerIndices.Add(playerIndex);

        Debug.Log($"[{nameof(MatchDataManager)}] AddPlayerToTeam: Player {playerIndex} added to Team {teamIndex}. Team now has {Teams[teamIndex].PlayerIndices.Count} players.");

        return playerIndex;
    }

    /// <summary>
    /// Moves an existing player from its current team to a new team.
    /// </summary>
    public void MovePlayerToTeam(int playerIndex, int newTeamIndex)
    {
        var player = Players[playerIndex];
        var oldTeamIndex = player.TeamIndex;

        if (oldTeamIndex >= 0 && oldTeamIndex < Teams.Count)
            Teams[oldTeamIndex].PlayerIndices.Remove(playerIndex);

        player.TeamIndex = newTeamIndex;
        Teams[newTeamIndex].PlayerIndices.Add(playerIndex);

        Debug.Log($"[{nameof(MatchDataManager)}] MovePlayerToTeam: Player {playerIndex} moved from Team {oldTeamIndex} to Team {newTeamIndex}.");
    }

    /// <summary>
    /// Creates a new team with one initial player.
    /// </summary>
    public void AddTeam()
    {
        var team = new MatchTeam
        {
            Name = "",
            Color = Color.white,
        };

        Teams.Add(team);

        var teamIndex = Teams.Count - 1;
        AddPlayerToTeam(teamIndex);

        Debug.Log($"[{nameof(MatchDataManager)}] AddTeam: Team {teamIndex} created.");
    }

    public void RemoveTeam(int teamIndex)
    {
        var team = Teams[teamIndex];

        var playerIndices = team.PlayerIndices;
        foreach (var playerIndex in playerIndices)
            RemovePlayer(playerIndex);

        Teams.RemoveAt(teamIndex);
    }

    /// <summary>
    /// Applies the given match mode, adjusting team count and trimming excess players.
    /// </summary>
    public void ApplyMatchMode(MatchMode mode)
    {
        CurrentMatchMode = mode;

        var config = MATCH_CONFIGS[mode];

        var teamCount = config.TeamCount;
        if (Teams.Count > teamCount)
        {
            for (var i = Teams.Count - 1; i >= teamCount; i--)
                RemoveTeam(i);
        }
        else
        {
            for (var i = Teams.Count; i < teamCount; i++)
                AddTeam();
        }

        TrimPlayersToTeamSize(config.TeamSize);

        Debug.Log($"[{nameof(MatchDataManager)}] TeamCount: {Teams.Count}, TeamSize: {config.TeamSize}");
    }

    /// <summary>
    /// Removes players exceeding the given team size from all teams.
    /// </summary>
    private void TrimPlayersToTeamSize(int teamSize)
    {
        foreach (var team in Teams)
        {
            while (team.PlayerIndices.Count > teamSize)
            {
                var lastPlayerIndex = team.PlayerIndices[team.PlayerIndices.Count - 1];
                RemovePlayer(lastPlayerIndex);
            }
        }
    }
}
