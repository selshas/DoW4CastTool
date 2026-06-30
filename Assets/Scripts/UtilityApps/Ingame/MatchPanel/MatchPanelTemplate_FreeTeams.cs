using System.Linq;

public class MatchPanelTemplate_FreeTeams : MatchPanelTemplate
{
    public override void LoadTeamData()
    {
        var teamDatas = MatchDataManager.Instance.GetTeamDatas().ToArray();
        new UIItemList<MatchTeam>(TeamPlateContainer, teamDatas, (child, data, i) =>
        {
            var teamPlate = child.GetComponent<IngameTeamPlate>();
            teamPlate.LoadTeamData(data);
        });
    }
}
