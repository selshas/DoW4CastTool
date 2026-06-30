using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GlobalInputSystem;

public class KnownPlayerListManager : UtilityAppBase
{
    [SerializeField] private Transform playerDataContainer;
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchResetButton;

    private UIItemList<string> playerItems;
    private List<string> playerNames = new List<string>();

    /// <summary>
    /// No dedicated hotkey; toggled by OutgameAppController.
    /// </summary>
    public override void InitializeInputs()
    {
    }

    /// <summary>
    /// Rebuilds the list from PlayerDataLoader each time the panel becomes visible.
    /// </summary>
    private void OnEnable()
    {
        if (!PlayerDataLoader.Instance.IsLoaded)
            return;

        searchInputField.onValueChanged.AddListener(OnSearchChanged);
        searchResetButton.onClick.AddListener(OnSearchReset);
        searchResetButton.gameObject.SetActive(!string.IsNullOrEmpty(searchInputField.text));
        RebuildList(searchInputField.text);
    }

    /// <summary>
    /// Unregisters the search callback when the panel is hidden.
    /// </summary>
    private void OnDisable()
    {
        searchInputField.onValueChanged.RemoveListener(OnSearchChanged);
        searchResetButton.onClick.RemoveListener(OnSearchReset);
    }

    /// <summary>
    /// Called when the search bar text changes.
    /// </summary>
    private void OnSearchChanged(string keyword)
    {
        searchResetButton.gameObject.SetActive(!string.IsNullOrEmpty(keyword));
        RebuildList(keyword);
    }

    /// <summary>
    /// Clears the search input field.
    /// </summary>
    private void OnSearchReset()
    {
        searchInputField.text = string.Empty;
    }

    /// <summary>
    /// Rebuilds the UI list filtered by keyword. Shows all names when keyword is empty.
    /// </summary>
    private void RebuildList(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            playerNames.Clear();
            playerNames.AddRange(PlayerDataLoader.Instance.KnownNames);
        }
        else
        {
            PlayerDataLoader.Instance.FindCandidates(keyword, int.MaxValue, ref playerNames);
        }

        if (playerItems == null)
            playerItems = new UIItemList<string>(playerDataContainer, playerNames, BindItem);
        else
            playerItems.UpdateItems(playerNames);
    }

    /// <summary>
    /// Binds a single list item with the player name label and delete button.
    /// </summary>
    private void BindItem(Transform child, string name)
    {
        var label = child.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
        label.text = name;

        var deleteButton = child.Find("Button_Delete").GetComponent<Button>();
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => RemovePlayer(name));
    }

    /// <summary>
    /// Removes the player name from persistent storage and rebuilds the list.
    /// </summary>
    private void RemovePlayer(string name)
    {
        PlayerDataLoader.Instance.RemoveName(name);
        RebuildList(searchInputField.text);
    }
}
