using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerPlate : MonoBehaviour
{
    [SerializeField] private RectTransform handler;
    [SerializeField] private Button button_Remove;

    [SerializeField] private Toggle toggle_factionSelection;
    [SerializeField] private Toggle toggle_heroSelection;

    [SerializeField] private RawImage image_Faction;
    [SerializeField] private RawImage image_Hero;

    [SerializeField] private TMP_InputField inputField_PlayerName;

    private static readonly Vector3[] cornerBuffer = new Vector3[4];

    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private PlayerSlot originSlot;
    private PlayerNameAutoCompletionList autoCompletionList;
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

        toggle_factionSelection.onValueChanged.AddListener((isOn) => 
        {
            if (isOn)
            {
                var playerData = MatchDataManager.Instance.Players[playerIndex];
                FactionSelector.Instance.Open(playerData, toggle_factionSelection);
            }
            else
            {
                FactionSelector.Instance.Close();
            }
        });
        toggle_heroSelection.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                var playerData = MatchDataManager.Instance.Players[playerIndex];
                HeroSelector.Instance.Open(playerData, toggle_heroSelection);
            }
            else
            {
                HeroSelector.Instance.Close();
            }
        });

        button_Remove.onClick.AddListener(Remove);
        inputField_PlayerName.onValueChanged.AddListener(OnPlayerNameChanged);
    }

    /// <summary>
    /// Syncs the input field text to the MatchPlayer data and triggers auto-completion.
    /// </summary>
    private void OnPlayerNameChanged(string value)
    {
        if (playerIndex < 0)
            return;

        MatchDataManager.Instance.Players[playerIndex].Name = value;

        if (!string.IsNullOrEmpty(value))
            ShowAutoCompletion(value);
        else
            HideAutoCompletion();
    }

    /// <summary>
    /// Positions the auto-completion list below the input field and refreshes candidates.
    /// </summary>
    private void ShowAutoCompletion(string keyword)
    {
        if (autoCompletionList == null)
            autoCompletionList = FindAnyObjectByType<PlayerNameAutoCompletionList>(FindObjectsInactive.Include);

        if (autoCompletionList == null)
            return;

        autoCompletionList.gameObject.SetActive(true);
        autoCompletionList.AttachTo(inputField_PlayerName);

        var listRect = autoCompletionList.GetComponent<RectTransform>();
        var inputRect = inputField_PlayerName.GetComponent<RectTransform>();

        inputRect.GetWorldCorners(cornerBuffer);
        listRect.position = new Vector3(cornerBuffer[0].x, cornerBuffer[0].y, 0f);
        listRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inputRect.rect.width);

        autoCompletionList.Refresh(keyword);
    }

    /// <summary>
    /// Hides the auto-completion list.
    /// </summary>
    private void HideAutoCompletion()
    {
        if (autoCompletionList == null)
            return;

        autoCompletionList.Hide();
    }

    /// <summary>
    /// Sets the slot this plate originated from.
    /// </summary>
    public void SetOriginSlot(PlayerSlot slot)
    {
        originSlot = slot;
    }

    /// <summary>
    /// Sets the faction symbol and hero portrait from the player's data.
    /// </summary>
    public void ApplyPlayerData(MatchPlayer player)
    {
        playerIndex = player.PlayerIndex;

        var factionData = FactionDataLoader.Instance.GetByName(player.FactionName);
        if (factionData == null)
            return;

        if (factionData.SymbolTexture != null)
            image_Faction.texture = factionData.SymbolTexture;

        if (factionData.Heroes.TryGetValue(player.HeroName, out var heroData) && heroData.PortraitTexture != null)
            image_Hero.texture = heroData.PortraitTexture;
    }

    private void Remove()
    {
        MatchSetup.Instance.RemovePlayerFromSlot(originSlot);
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
