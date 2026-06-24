using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    public static MatchDataManager Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentMatchMode = MatchMode.OneOnOne;

        DontDestroyOnLoad(gameObject);
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
        if (!reservedPlayerIndices.Contains(playerIndex))
            throw new Exception($"Player index '{playerIndex}' is already used.");

        if (playerIndex < 0 || playerIndex >= Players.Count)
            throw new Exception($"Player index '{playerIndex}' is out of valid range.");

        var player = Players[playerIndex];
        var team = Teams[player.TeamIndex];

        team.PlayerIndices.Remove(playerIndex);
        reservedPlayerIndices.Push(playerIndex);
    }

    public void AddTeam()
    {
        var team = new MatchTeam
        {
            Name = "",
            Color = Color.white,
        };

        var firstPlayerIndex = AddPlayer();
        team.PlayerIndices.Add(firstPlayerIndex);

        Teams.Add(team);
    }

    public void RemoveTeam(int teamIndex)
    {
        var team = Teams[teamIndex];

        var playerIndices = team.PlayerIndices;
        foreach (var playerIndex in playerIndices)
            RemovePlayer(playerIndex);

        Teams.RemoveAt(teamIndex);
    }

    public void ApplyMatchMode(MatchMode mode)
    {
        CurrentMatchMode = mode;

        var config = MATCH_CONFIGS[mode];

        Teams.Clear();
        Players.Clear();

        // Change TeamCount
        var teamCount = config.TeamCount;
        if (Teams.Count > teamCount)
        {
            // TeamCount is going to reduce.

            for (var i = (Teams.Count - 1); i >= teamCount; i--)
                RemoveTeam(i);
        }
        else
        {
            // TeamCount is going to increase.

            for (var i = Teams.Count; i < teamCount; i++)
                AddTeam();
        }

        Debug.Log($"[{nameof(MatchDataManager)}] TeamCount: {Teams.Count}");
    }
}
