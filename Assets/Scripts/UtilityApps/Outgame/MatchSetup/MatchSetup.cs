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

    private List<TeamPlate> teamPlates = new List<TeamPlate>();

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
        var uiItemList = new UIItemList<MatchConfig>(MatchModeSelector, items, (child, data, i) =>
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
        var config = MatchDataManager.MATCH_CONFIGS[MatchDataManager.Instance.CurrentMatchMode];
        var teamSize = config.TeamSize;

        teamPlates.Clear();

        var teamDatas = MatchDataManager.Instance.GetTeamDatas().ToArray();
        var uiItemList = new UIItemList<MatchTeam>(TeamListContainer, teamDatas, (child, data, i) =>
        {
            var teamId = data.TeamId;
            var teamData = teamDatas[teamId];

            var teamPlate = child.GetComponentInChildren<TeamPlate>();
            teamPlates.Add(teamPlate);

            teamPlate.SetTeamId(teamId);
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
            image_MapPreview.texture = MapDataLoader.NotFoundTexture;
            image_MapPreview.color = Color.white;
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

        // Remove all empty slots from all teams.
        var teamDatas = MatchDataManager.Instance.GetTeamDatas();
        foreach (var teamId in teamDatas.Select(x => x.TeamId))
            MatchDataManager.Instance.RemoveEmptyPlayerSlots(teamId);

        var playerNames = teamDatas
            .SelectMany(x => x.PlayerIds)
            .Select(x => MatchDataManager.Instance.GetPlayerData(x).Name);

        PlayerDataLoader.Instance.MergeNames(playerNames);

        SceneManager.LoadScene(SceneNames.IngameOverlay);
    }

    /// <summary>
    /// Configure current match setup to given <see cref="MatchMode"/>
    /// </summary>
    private void ApplyMatchMode(MatchMode mode)
    {
        MatchDataManager.Instance.ApplyMatchMode(mode);

        RebuildTeamList();
        RefreshMapDropdown();
    }

    /// <summary>
    /// Add a player to the team
    /// </summary>
    public int AddPlayer(int teamId, bool preventRefresh = true)
    {
        var playerId = MatchDataManager.Instance.AddPlayerToTeam(teamId);
        var teamPlate = teamPlates.Find(x => x.TeamId == teamId);
        var emptySlot = teamPlate.PlayerSlots.Find(x => !x.HasPlate);
        
        CreatePlayerPlate(emptySlot, playerId);
        if (!preventRefresh)
        {
            RefreshMapDropdown();
        }

        return playerId;
    }

    public void RemovePlayer(int playerId, bool preventRefresh = true)
    {
        var playerData = MatchDataManager.Instance.GetPlayerData(playerId);
        var playerPlate = FindPlayerPlate(playerId);
        playerPlate.OriginSlot.Clear();

        MatchDataManager.Instance.RemovePlayer(playerId);

        if (!preventRefresh)
        {
            RefreshMapDropdown();
        }
    }

    #region Player Plate

    /// <summary>
    /// Instantiates a PlayerPlate and attaches it to the slot. Creates a new player if no playerId is given.
    /// </summary>
    public PlayerPlate CreatePlayerPlate(PlayerSlot playerSlot, int playerId = -1)
    {
        if (playerId < 0)
            playerId = MatchDataManager.Instance.AddPlayerToTeam(playerSlot.TeamId);

        var playerData = MatchDataManager.Instance.GetPlayerData(playerId);

        var plateObject = Instantiate(prefab_PlayerPlate, playerSlot.transform);
        var playerPlate = plateObject.GetComponent<PlayerPlate>();
        playerPlate.ApplyPlayerData(playerData);

        playerSlot.AttachPlate(playerPlate);

        RefreshMapDropdown();

        return playerPlate;
    }

    public PlayerPlate FindPlayerPlate(int playerId)
    {
        var playerData = MatchDataManager.Instance.GetPlayerData(playerId);

        var teamId = playerData.TeamId;
        var teamPlate = teamPlates.Find(x => x.TeamId == teamId);
        if (teamPlate == null)
            return null;

        var playerPlate = teamPlate.PlayerSlots.Where(x => x.HasPlate).First(x => x.CurrentPlayerId == playerId)?.AttachedPlate;
        if (playerPlate == null)
            return null;

        return playerPlate;
    }

    public void ChangeFaction(int playerId, string factionName)
    {
        var playerData = MatchDataManager.Instance.GetPlayerData(playerId);
        var playerPlate = FindPlayerPlate(playerId);
        if (playerPlate == null)
            return;

        if (!FactionDataLoader.Instance.Factions.TryGetValue(factionName, out var factionData))
            return;

        playerData.FactionName = factionName;
        playerData.HeroName = FactionDataLoader.Instance.GetRandomHeroName(factionName);

        playerPlate.ApplyPlayerData(playerData);
    }

    public void ChangeHero(int playerId, string heroName)
    {
        var playerData = MatchDataManager.Instance.GetPlayerData(playerId);
        var playerPlate = FindPlayerPlate(playerId);
        if (playerPlate == null)
            return;

        if (!FactionDataLoader.Instance.Factions.TryGetValue(playerData.FactionName, out var factionData))
            return;
        if (!factionData.Heroes.TryGetValue(heroName, out var heroData))
            return;

        playerData.HeroName = heroName;

        playerPlate.ApplyPlayerData(playerData);
    }

    #endregion

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

}
