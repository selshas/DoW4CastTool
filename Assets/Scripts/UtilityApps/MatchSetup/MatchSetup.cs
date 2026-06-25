using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class MatchSetup : UtilityAppBase
{
    public static MatchSetup Instance { get; private set; }

    private static readonly Color[] TEAM_COLORS = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.2f, 0.4f, 0.9f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.9f, 0.85f, 0.1f),
    };

    private static readonly string[] TEAM_DEFAULT_NAMES = { "Red", "Blue", "Green", "Yellow" };

    private static readonly MatchMode[] MODE_ORDER = 
    {
        MatchMode.OneOnOne, MatchMode.TwoOnTwo, MatchMode.ThreeOnThree, MatchMode.FreeForAll
    };

    private const int TEAMS_PER_ROW = 2;


    [SerializeField] public RectTransform MatchModeSelector;
    [SerializeField] public RectTransform TeamListContainer;
    [SerializeField] public RectTransform FFAPlayerListContainer;
    [SerializeField] private GameObject prefab_PlayerPlate;


    [Header("Map Selection")]
    [SerializeField] private TMP_Dropdown dropdown_Map;
    [SerializeField] private RawImage image_MapPreview;

    private List<MapData> filteredMaps = new List<MapData>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        MapDataLoader.Load();

        dropdown_Map.onValueChanged.AddListener(OnMapSelected);
    }

    protected override void Start()
    {
        base.Start();

        InitializeMatchModeSelector();

        ApplyMatchMode(MatchDataManager.Instance.CurrentMatchMode);
    }

    public override void InitializeInputs()
    {
        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcEscape,
            InputState.Pressed,
            (self) => GlobalAppController.Instance.ToggleApp_MatchSetup()
        );
    }

    #region Mode Selection

    private void InitializeMatchModeSelector()
    {
        Debug.Log($"[{nameof(MatchSetup)}] MatchModeList Initialized.");

        var toggleGroup = MatchModeSelector.GetComponent<ToggleGroup>();
        var items = MODE_ORDER.Select(x => MatchDataManager.MATCH_CONFIGS[x]).ToArray();
        var uiItemList = new UIItemList<MatchConfig>(MatchModeSelector, items, (child, data) =>
        {
            var label = child.Find("Label").GetComponent<TextMeshProUGUI>();
            var toggle = child.GetComponentInChildren<Toggle>();
            
            label.text = data.Label;
            toggle.group = toggleGroup;

            var matchMode = data.Mode;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isSet) =>
            {
                if (!isSet)
                    return;

                ApplyMatchMode(matchMode);
            });
        });

        uiItemList.GetInstance(0).GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(true);
    }

    private void RebuildTeamList()
    {
        Debug.Log($"[{nameof(MatchSetup)}] Rebuild TeamList.");

        var config = MatchDataManager.MATCH_CONFIGS[MatchDataManager.Instance.CurrentMatchMode];
        var teamSize = config.TeamSize;


        var teams = MatchDataManager.Instance.Teams;
        var uiItemList = new UIItemList<MatchTeam>(TeamListContainer, teams, (child, data) =>
        {
            var currentTeamIndex = teams.IndexOf(data);
            var team = teams[currentTeamIndex];

            Debug.Log($"[{nameof(MatchSetup)}] RebuildTeamList: Binding Team {currentTeamIndex}, TeamSize={teamSize}, Players={team.PlayerIndices.Count}.");

            var playerSlotsContainer = child.Find("PlayerSlots");

            for (var i = 0; i < playerSlotsContainer.childCount; i++)
            {
                var slot = playerSlotsContainer.GetChild(i);
                slot.gameObject.SetActive(i < teamSize);

                var playerSlot = slot.GetComponent<PlayerSlot>();
                if (playerSlot == null)
                    continue;

                playerSlot.SetTeamIndex(currentTeamIndex);

                if (i < team.PlayerIndices.Count)
                    CreatePlayerPlate(playerSlot, team.PlayerIndices[i]);
            }
        });

        for (var i = 0; i < uiItemList.Count; i++)
            LayoutRebuilder.ForceRebuildLayoutImmediate(uiItemList.GetInstance(i) as RectTransform);

        LayoutRebuilder.ForceRebuildLayoutImmediate(TeamListContainer);
    }

    #endregion

    #region Map Selection

    private void RefreshMapDropdown()
    {
        filteredMaps = MapDataLoader.GetByMatchMode(MatchDataManager.Instance.CurrentMatchMode);
        dropdown_Map.ClearOptions();

        if (filteredMaps.Count == 0)
        {
            dropdown_Map.AddOptions(new List<string> { "No Maps" });
            dropdown_Map.interactable = false;

            ApplyMapPreview(null);

            return;
        }

        dropdown_Map.interactable = true;

        var options = new List<string>();
        foreach (var map in filteredMaps)
            options.Add(map.Name);

        dropdown_Map.AddOptions(options);
        dropdown_Map.value = 0;

        OnMapSelected(0);
    }

    private void OnMapSelected(int index)
    {
        if ((index < 0) || (index >= filteredMaps.Count))
        {
            MatchDataManager.Instance.CurrentMap = null;
            ApplyMapPreview(null);
            return;
        }

        MatchDataManager.Instance.CurrentMap = filteredMaps[index];
        ApplyMapPreview(filteredMaps[index]);
    }

    private void ApplyMapPreview(MapData map)
    {
        if (map != null)
        {
            image_MapPreview.texture = map.MinimapTexture;
            image_MapPreview.color = Color.white;
        }
        else
        {
            image_MapPreview.texture = null;
            image_MapPreview.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        }
    }

    #endregion

    #region Match Mode

    private void ApplyMatchMode(MatchMode mode)
    {
        MatchDataManager.Instance.ApplyMatchMode(mode);

        RefreshMapDropdown();
        RebuildTeamList();
    }

    #endregion

    #region Player Plate

    /// <summary>
    /// Instantiates a PlayerPlate and attaches it to the slot. Creates a new player if no playerIndex is given.
    /// </summary>
    public void CreatePlayerPlate(PlayerSlot slot, int playerIndex = -1)
    {
        if (playerIndex < 0)
            playerIndex = MatchDataManager.Instance.AddPlayerToTeam(slot.TeamIndex);

        var plateObject = Instantiate(prefab_PlayerPlate, slot.transform);
        var plate = plateObject.GetComponent<PlayerPlate>();

        plate.SetPlayerIndex(playerIndex);
        slot.AttachPlate(plate);

        Debug.Log($"[{nameof(MatchSetup)}] CreatePlayerPlate: Player {playerIndex} attached to Team {slot.TeamIndex}.");
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void ClearChildren(Transform parent)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private void AddClickHandler(GameObject target, UnityEngine.Events.UnityAction action)
    {
        var button = target.GetComponent<Button>();
        if (button == null)
        {
            button = target.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
        }

        button.onClick.AddListener(action);
    }
}
