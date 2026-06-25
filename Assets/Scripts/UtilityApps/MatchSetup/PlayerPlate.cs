using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPlate : MonoBehaviour
{
    [SerializeField] private RectTransform handler;
    [SerializeField] private Button button_Remove;

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerSlot originSlot;
    private Vector2 dragOffset;
    private int playerIndex = -1;

    public PlayerSlot OriginSlot => originSlot;
    public int PlayerIndex => playerIndex;
    public bool Dragging { get; private set; }

    /// <summary>
    /// Caches references and wires the remove button.
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        button_Remove.onClick.AddListener(OnRemoveClicked);
    }

    /// <summary>
    /// Sets the slot this plate originated from.
    /// </summary>
    public void SetOriginSlot(PlayerSlot slot)
    {
        originSlot = slot;
    }

    /// <summary>
    /// Assigns the player index this plate represents.
    /// </summary>
    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }

    /// <summary>
    /// Removes the player from data and destroys this plate.
    /// </summary>
    private void OnRemoveClicked()
    {
        Debug.Log($"[{nameof(PlayerPlate)}] OnRemoveClicked: Removing Player {playerIndex}.");

        if (playerIndex >= 0)
            MatchDataManager.Instance.RemovePlayer(playerIndex);

        if (originSlot != null)
            originSlot.ClearPlate();

        Destroy(gameObject);
    }

    /// <summary>
    /// Detaches from the parent slot and begins floating over the canvas.
    /// </summary>
    public void OnHandleBeginDrag(PointerEventData eventData)
    {
        Dragging = true;
        SetRemoveButtonVisible(false);
        canvasGroup.blocksRaycasts = false;

        var slotRect = originSlot.GetComponent<RectTransform>();
        var slotSize = slotRect.rect.size;

        rectTransform.SetParent(rootCanvas.transform, true);
        rectTransform.SetAsLastSibling();

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = slotSize;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        dragOffset = (Vector2)rectTransform.localPosition - localPoint;
    }

    /// <summary>
    /// Follows the cursor while dragging.
    /// </summary>
    public void OnHandleDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        rectTransform.localPosition = localPoint + dragOffset;
    }

    /// <summary>
    /// Returns to the origin slot when the handle drag ends.
    /// </summary>
    public void OnHandleEndDrag(PointerEventData eventData)
    {
        Dragging = false;
        canvasGroup.blocksRaycasts = true;
        SetRemoveButtonVisible(true);

        ReturnToSlot();
    }

    /// <summary>
    /// Reattaches this plate to its origin slot as a child.
    /// </summary>
    public void ReturnToSlot()
    {
        if (originSlot == null)
            return;

        rectTransform.SetParent(originSlot.transform, false);
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// Toggles the remove button's visibility and interactability.
    /// </summary>
    private void SetRemoveButtonVisible(bool visible)
    {
        button_Remove.gameObject.SetActive(visible);
    }
}
