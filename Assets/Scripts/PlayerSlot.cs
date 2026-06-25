using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour, IDropHandler
{
    private PlayerPlate attachedPlate;
    private RectTransform rectTransform;
    private int teamIndex;

    /// <summary>
    /// Returns whether this slot has an attached PlayerPlate.
    /// </summary>
    public bool HasPlate => attachedPlate != null;

    /// <summary>
    /// Returns the team index this slot belongs to.
    /// </summary>
    public int TeamIndex => teamIndex;

    /// <summary>
    /// Initializes references and sets up the click handler.
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        var button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
        }

        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Sets the team index this slot belongs to.
    /// </summary>
    public void SetTeamIndex(int index)
    {
        teamIndex = index;
    }

    /// <summary>
    /// Delegates plate creation to MatchSetup.
    /// </summary>
    private void OnClick()
    {
        if (attachedPlate != null)
            return;

        Debug.Log($"[{nameof(PlayerSlot)}] OnClick: Requesting plate for Team {teamIndex}.");

        MatchSetup.Instance.CreatePlayerPlate(this);
    }

    /// <summary>
    /// Receives a dropped PlayerPlate and attaches it to this slot.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var draggedPlate = eventData.pointerDrag?.GetComponentInParent<PlayerPlate>();
        if (draggedPlate == null)
            return;

        if (HasPlate)
            return;

        draggedPlate.OriginSlot.ClearPlate();
        AttachPlate(draggedPlate);
    }

    /// <summary>
    /// Attaches an existing PlayerPlate to this slot and updates team assignment.
    /// </summary>
    public void AttachPlate(PlayerPlate plate)
    {
        attachedPlate = plate;
        plate.SetOriginSlot(this);
        plate.ReturnToSlot();

        if (plate.PlayerIndex >= 0)
        {
            var player = MatchDataManager.Instance.Players[plate.PlayerIndex];
            if (player.TeamIndex != teamIndex)
            {
                Debug.Log($"[{nameof(PlayerSlot)}] AttachPlate: Player {plate.PlayerIndex} moved to Team {teamIndex}.");
                MatchDataManager.Instance.MovePlayerToTeam(plate.PlayerIndex, teamIndex);
            }
        }
    }

    /// <summary>
    /// Clears the plate reference without destroying the plate.
    /// </summary>
    public void ClearPlate()
    {
        attachedPlate = null;
    }

    /// <summary>
    /// Detaches and destroys the current PlayerPlate if one exists.
    /// </summary>
    public void DetachPlate()
    {
        if (attachedPlate == null)
            return;

        Destroy(attachedPlate.gameObject);
        attachedPlate = null;
    }

    /// <summary>
    /// Cleans up the attached plate when the slot is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        DetachPlate();
    }
}
