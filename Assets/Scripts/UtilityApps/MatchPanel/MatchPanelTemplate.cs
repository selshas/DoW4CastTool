using UnityEngine;

public abstract class MatchPanelTemplate : MonoBehaviour
{
    public MatchMode MatchMode = MatchMode.OneOnOne;
    public Transform TeamPlateContainer;

    public abstract void LoadTeamData();
}
