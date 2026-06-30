using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TeamPlate : MonoBehaviour
{
    private int teamId = -1;
    public int TeamId
    {
        get => teamId;
        set => SetTeamId(value);
    }

    public readonly List<PlayerSlot> PlayerSlots = new List<PlayerSlot>();

    [SerializeField] private RectTransform playerSlotContainer;
    [SerializeField] private Button button_increaseSlot;
    [SerializeField] private Button button_decreaseSlot;

    [SerializeField] private Button button_removeTeam;

    private UIItemList<int> playerIdList;

    public void SetTeamId(int teamId)
    {
        this.teamId = teamId;
        Rebuild();
    }

    public void Rebuild()
    {
        if (playerIdList != null)
            playerIdList.Clear();

        PlayerSlots.Clear();

        var teamData = MatchDataManager.Instance.GetTeamData(teamId);
        if (teamData == null)
            throw new KeyNotFoundException($"Team ID '{teamId}' does not exist.");

        playerIdList = new UIItemList<int>(playerSlotContainer, teamData.PlayerIds.ToArray(), (child, playerId, i) =>
        {
            var playerSlot = child.GetComponent<PlayerSlot>();
            if (playerSlot == null)
                throw new MissingComponentException($"Component '{nameof(PlayerSlot)}' does not present on the prototype item object.");

            PlayerSlots.Add(playerSlot);
            playerSlot.SetTeamId(teamId);


            if (playerId == -1)
                return;

            MatchSetup.Instance.CreatePlayerPlate(playerSlot, playerId);
        });
    }

}
