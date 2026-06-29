using UnityEngine;

public class MatchPanelTemplate_NvN : MatchPanelTemplate
{
    public override void LoadTeamData()
    {
        var teamPlates = GetComponentsInChildren<IngameTeamPlate>();
        for (var i = 0; i < MatchDataManager.Instance.Teams.Count; i++)
        {
            var teamData = MatchDataManager.Instance.Teams[i];
            var teamPlate = teamPlates[0];

            teamPlate.LoadTeamData(teamData);
        }
    }
}
