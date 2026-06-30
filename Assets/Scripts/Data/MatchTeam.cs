using System.Collections.Generic;
using UnityEngine;

public class MatchTeam
{
    public int TeamId = -1;
    public string Name;
    public Color Color;
    public int Score;

    public readonly List<int> PlayerIds = new List<int>();

    public int PlayerCount => PlayerIds.Count;

    public MatchPlayer[] Players 
    {
        get
        {
            var result = new MatchPlayer[PlayerIds.Count];
            for (var i = 0; i < PlayerIds.Count; i++)
            {
                var playerId = PlayerIds[i];
                var playerData = MatchDataManager.Instance.GetPlayerData(playerId);

                result[i] = playerData;
            }

            return result;
        }
    }
}
