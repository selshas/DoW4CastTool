using System.Collections.Generic;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    public static MatchDataManager Instance { get; private set; }

    public MatchMode Mode;
    public MapData Map;
    public readonly List<MatchTeam> Teams = new List<MatchTeam>();
    public readonly List<MatchPlayer> Players = new List<MatchPlayer>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Reset()
    {
        Mode = MatchMode.None;
        Map = null;
        Teams.Clear();
        Players.Clear();
    }
}
