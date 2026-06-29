using System.Collections.Generic;
using UnityEngine;

public class MatchTeam
{
    public string Name;
    public Color Color;
    public int Score;

    public readonly List<int> PlayerIndices = new List<int>();

    public int PlayerCount => PlayerIndices.Count;

    public MatchPlayer[] Players 
    {
        get
        {
            var result = new MatchPlayer[PlayerIndices.Count];
            for (var i = 0; i < PlayerIndices.Count; i++)
            {
                var playerIndex = PlayerIndices[i];
                var playerData = MatchDataManager.Instance.Players[playerIndex];

                result[i] = playerData;
            }

            return result;
        }
    }
}
