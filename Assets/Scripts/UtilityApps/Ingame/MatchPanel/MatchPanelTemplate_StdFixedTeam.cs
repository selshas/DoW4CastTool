using System.Linq;

public class MatchPanelTemplate_NvN : MatchPanelTemplate
{
    public override void LoadTeamData()
    {
        var teamPlates = GetComponentsInChildren<IngameTeamPlate>();
        var teamDatas = MatchDataManager.Instance.GetTeamDatas().ToArray();
        for (var i = 0; i < teamDatas.Length; i++)
        {
            var teamData = teamDatas[i];
            var teamPlate = teamPlates[0];

            teamPlate.LoadTeamData(teamData);
        }
    }
}
