using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPlate : MonoBehaviour
{
    [SerializeField] private RectTransform handler;
    [SerializeField] private Button button_Remove;
    [SerializeField] private RawImage image_Faction;
    [SerializeField] private RawImage image_Hero;
    [SerializeField] private TMP_InputField inputField_PlayerName;

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
        inputField_PlayerName.onValueChanged.AddListener(OnPlayerNameChanged);
    }

    /// <summary>
    /// Syncs the input field text to the MatchPlayer data.
    /// </summary>
    private void OnPlayerNameChanged(string value)
    {
        if (playerIndex < 0)
            return;

        MatchDataManager.Instance.Players[playerIndex].Name = value;
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
    /// Sets the faction symbol and hero portrait from the player's data.
    /// </summary>
    public void ApplyPlayerData(MatchPlayer player)
    {
        var factionData = FactionDataLoader.Instance.GetByName(player.FactionName);
        if (factionData == null)
            return;

        if (factionData.Symbol != null)
            image_Faction.texture = factionData.Symbol.texture;

        if (factionData.Heroes.TryGetValue(player.HeroName, out var heroData) && heroData.Portrait != null)
            image_Hero.texture = heroData.Portrait.texture;
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
