using System.Collections.Generic;
using UnityEngine;

public class MatchTeam
{
    public string Name;
    public Color Color;
    public int Score;

    public readonly List<int> PlayerIndices = new List<int>();

    public int PlayerCount => PlayerIndices.Count;
}
