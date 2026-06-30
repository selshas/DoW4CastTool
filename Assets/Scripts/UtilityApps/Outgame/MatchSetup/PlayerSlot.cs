using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerPlate AttachedPlate;
    private RectTransform rectTransform;
    private int teamId;

    /// <summary>
    /// Get currently attached playet's index 
    /// </summary>
    public int CurrentPlayerId
        => AttachedPlate.PlayerId;
    
    /// <summary>
    /// Returns whether this slot has an attached PlayerPlate.
    /// </summary>
    public bool HasPlate => (AttachedPlate != null);

    /// <summary>
    /// Returns the team index this slot belongs to.
    /// </summary>
    public int TeamId => teamId;

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
    public void SetTeamId(int teamId)
    {
        this.teamId = teamId;
    }

    /// <summary>
    /// Delegates plate creation to MatchSetup.
    /// </summary>
    private void OnClick()
    {
        if (AttachedPlate != null)
            return;

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

        var srcSlot = draggedPlate.OriginSlot;
        var dstSlot = this;

        if (srcSlot == dstSlot)
            return;

        if (HasPlate)
        {
            // Swap
            var srcPlate = srcSlot.DetachPlate(); // this must be same with draggedPlate but for consistency;
            var dstPlate = dstSlot.DetachPlate();

            dstSlot.AttachPlate(srcPlate);
            srcSlot.AttachPlate(dstPlate);
        }
        else
        {
            // Move
            draggedPlate.OriginSlot.DetachPlate();
            AttachPlate(draggedPlate);
        }
    }

    /// <summary>
    /// Attaches an existing PlayerPlate to this slot and updates team assignment.
    /// </summary>
    public void AttachPlate(PlayerPlate playerPlate)
    {
        AttachedPlate = playerPlate;

        playerPlate.SetOriginSlot(this);
        playerPlate.ReturnToSlot();

        if (playerPlate.PlayerId < 0)
            return;

        var playerData = MatchDataManager.Instance.GetPlayerData(playerPlate.PlayerId);
        if (playerData.TeamId == teamId)
            return;

        MatchDataManager.Instance.MovePlayerToTeam(playerPlate.PlayerId, teamId);
    }

    /// <summary>
    /// Detaches and destroys the current PlayerPlate if one exists.
    /// </summary>
    public void Clear()
    {
        Destroy(DetachPlate().gameObject);
    }

    /// <summary>
    /// Detach plate from this slot.
    /// </summary>
    public PlayerPlate DetachPlate()
    {
        if (AttachedPlate == null)
            return null;

        var playerPlate = AttachedPlate;
        AttachedPlate = null;

        return playerPlate;
    }

    /// <summary>
    /// Cleans up the attached plate when the slot is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        DetachPlate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Slot color effect
        GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        if (!HasPlate)
            return;

        // Plate color effect
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Slot color effect
        GetComponent<Image>().color = Color.white;

        if (!HasPlate)
            return;

        // Plate color effect
    }
}
