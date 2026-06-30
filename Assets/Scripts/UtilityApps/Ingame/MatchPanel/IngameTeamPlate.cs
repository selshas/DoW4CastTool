using System.Linq;
using TMPro;
using UnityEngine;

public class IngameTeamPlate : MonoBehaviour
{
    public int TeamId
    {
        get => teamId;
        set => LoadTeamData(MatchDataManager.Instance.GetTeamData(value));
    }
    private int teamId;

    private MatchTeam teamData;

    [SerializeField] private Transform playerContainer;
    [SerializeField] private TextMeshProUGUI scoreText;

    private UIEffector_Greyscale greyscaleEffector;

    /// <summary>
    /// Caches all child Graphics, their original colors and materials, and prepares the greyscale material.
    /// </summary>
    private void Awake()
    {
        greyscaleEffector = GetComponent<UIEffector_Greyscale>();
    }

    public void LoadTeamData(MatchTeam teamData)
    {
        this.teamData = teamData;

        new UIItemList<MatchPlayer>(playerContainer, teamData.Players, (child, data, i) => 
        {
            var playerPlate = child.GetComponent<IngamePlayerPlate>();
            playerPlate.LoadPlayerData(data);
        });

        scoreText.text = teamData.Score.ToString();
    }

    private void SetState(bool dimmed)
    {
        greyscaleEffector.enabled = dimmed;
    }

    public void EvaluateDimState()
    {
        var dimmedPlayerCount = GetComponentsInChildren<UIEffector_Greyscale>().Where(x => x.enabled).Count();
        if (dimmedPlayerCount == 0)
            return;

        var dimmed = (teamData.PlayerCount == dimmedPlayerCount);
        SetState(dimmed);
    }

}
