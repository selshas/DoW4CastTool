public class MatchPanelTemplate_FreeTeams : MatchPanelTemplate
{
    public override void LoadTeamData()
    {
        new UIItemList<MatchTeam>(TeamPlateContainer, MatchDataManager.Instance.Teams, (child, data) =>
        {
            var teamPlate = child.GetComponent<IngameTeamPlate>();
            teamPlate.LoadTeamData(data);
        });
    }
}
