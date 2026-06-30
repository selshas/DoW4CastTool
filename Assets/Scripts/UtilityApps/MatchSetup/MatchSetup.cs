using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using static GlobalInputSystem;

using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Data.KeyCode;

public class MatchSetup : UtilityAppBase
{
    public static MatchSetup Instance { get; private set; }

    private static readonly MatchMode[] MODE_ORDER = 
    {
        MatchMode.OneOnOne, 
        MatchMode.TwoOnTwo, 
        MatchMode.ThreeOnThree, 
        MatchMode.FourOnFour,
        MatchMode.FreeForAll,
    };


    [SerializeField] public RectTransform MatchModeSelector;
    [SerializeField] public RectTransform TeamListContainer;
    [SerializeField] public RectTransform FFAPlayerListContainer;
    [SerializeField] private GameObject prefab_PlayerPlate;


    [Header("Map Selection")]
    [SerializeField] private TMP_Dropdown dropdown_Map;
    [SerializeField] private RawImage image_MapPreview;

    [Header("Actions")]
    [SerializeField] private Button button_StartMatch;

    private List<MapData> filteredMaps = new List<MapData>();

    private List<PlayerPlate> playerPlates = new List<PlayerPlate>();

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
        button_StartMatch.onClick.AddListener(StartMatch);
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
            (self) => OutgameAppController.Instance.SetAppActive<MatchSetup>(false)
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

        playerPlates.Clear();

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
        var matchData = MatchDataManager.Instance;
        filteredMaps = MapDataLoader.GetByFilter(matchData.CurrentMatchMode, matchData.TotalPlayerCount);
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

    /// <summary>
    /// Validates match data, persists player names, and loads the IngameOverlay scene.
    /// </summary>
    private void StartMatch()
    {
        var error = MatchDataManager.Instance.ValidateMatch();
        if (error != null)
        {
            Debug.LogWarning($"[{nameof(MatchSetup)}] StartMatch blocked: {error}");
            return;
        }

        var playerNames = MatchDataManager.Instance.Teams
            .SelectMany(t => t.PlayerIndices)
            .Select(i => MatchDataManager.Instance.Players[i].Name);

        PlayerDataLoader.Instance.MergeNames(playerNames);

        SceneManager.LoadScene(SceneNames.IngameOverlay);
    }

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
    public PlayerPlate CreatePlayerPlate(PlayerSlot playerSlot, int playerIndex = -1)
    {
        if (playerIndex < 0)
            playerIndex = MatchDataManager.Instance.AddPlayerToTeam(playerSlot.TeamIndex);

        var plateObject = Instantiate(prefab_PlayerPlate, playerSlot.transform);
        var plate = plateObject.GetComponent<PlayerPlate>();

        var playerData = MatchDataManager.Instance.Players[playerIndex];
        
        plate.ApplyPlayerData(playerData);
        playerSlot.AttachPlate(plate);
        playerPlates.Add(plate);

        RefreshMapDropdown();

        Debug.Log($"[{nameof(MatchSetup)}] CreatePlayerPlate: Player {playerIndex} attached to Team {playerSlot.TeamIndex}.");

        return plate;
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ChangeFaction(int playerIndex, string factionName)
    {
        var playerData = MatchDataManager.Instance.Players[playerIndex];
        if (!FactionDataLoader.Instance.Factions.TryGetValue(factionName, out var factionData))
            return;

        var plate = playerPlates.Find(x => x.PlayerIndex == playerIndex);
        if (plate == null)
            return;

        playerData.FactionName = factionName;
        playerData.HeroName = FactionDataLoader.Instance.GetRandomHeroName(factionName);
        plate.ApplyPlayerData(playerData);
    }

    public void ChangeHero(int playerIndex, string heroName)
    {
        var playerData = MatchDataManager.Instance.Players[playerIndex];
        if (!FactionDataLoader.Instance.Factions.TryGetValue(playerData.FactionName, out var factionData) 
            || !factionData.Heroes.TryGetValue(heroName, out var heroData)
        )
            return;

        var plate = playerPlates.Find(x => x.PlayerIndex == playerIndex);
        if (plate == null)
            return;

        playerData.HeroName = heroName;
        plate.ApplyPlayerData(playerData);
    }


    /// <summary>
    /// Removes the player from data and destroys this plate.
    /// </summary>
    public void RemovePlayerFromSlot(PlayerSlot playerSlot)
    {
        if (playerSlot.CurrentPlayerIndex >= 0)
            MatchDataManager.Instance.RemovePlayer(playerSlot.CurrentPlayerIndex);

        playerPlates.Remove(playerSlot.AttachedPlate);
        playerSlot.ClearPlate();

        RefreshMapDropdown();
    }

}
