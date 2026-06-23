using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class MatchSetup : UtilityAppBase
{
    private static readonly Color[] TEAM_COLORS = {
        new Color(0.9f, 0.2f, 0.2f),
        new Color(0.2f, 0.4f, 0.9f),
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.9f, 0.85f, 0.1f),
    };

    private static readonly string[] TEAM_DEFAULT_NAMES = { "Red", "Blue", "Green", "Yellow" };

    private static readonly MatchMode[] MODE_ORDER = {
        MatchMode.OneOnOne, MatchMode.TwoOnTwo, MatchMode.ThreeOnThree, MatchMode.FreeForAll
    };

    private static readonly Dictionary<MatchMode, MatchConfig> MATCH_CONFIGS = new Dictionary<MatchMode, MatchConfig>
    {
        { MatchMode.OneOnOne, new MatchConfig
            {
                Label = "1v1",
                TeamCount = 2,
                TeamSize = 1,
            }
        },
        { MatchMode.TwoOnTwo, new MatchConfig
            {
                Label = "2v2",
                TeamCount = 2,
                TeamSize = 2,
            }
        },
        { MatchMode.ThreeOnThree, new MatchConfig
            {
                Label = "3v3",
                TeamCount = 2,
                TeamSize = 3,
            }
        },
        { MatchMode.FreeForAll, new MatchConfig
            {
                Label = "FFA",
                TeamCount = 4,
                TeamSize = 1,
            }
        },
    };

    private const int TEAMS_PER_ROW = 2;


    [Header("Mode Selection")]
    [SerializeField] public RectTransform Proto_ModeToggles;
    [SerializeField] private RectTransform inst_ModeToggles;

    [Header("Team Setup")]
    [SerializeField] private GameObject proto_TeamListRow;
    [SerializeField] private GameObject proto_TeamSlot;
    [SerializeField] private GameObject proto_PlayerSlot;

    [SerializeField] private RectTransform inst_TeamList;

    [Header("Map Selection")]
    [SerializeField] private TMP_Dropdown dropdown_Map;
    [SerializeField] private RawImage image_MapPreview;

    private Faction[] allFactions;

    private List<MapData> filteredMaps = new List<MapData>();

    private void Awake()
    {
        MapDataLoader.Load();

        var factionList = new List<Faction>();
        foreach (Faction faction in System.Enum.GetValues(typeof(Faction)))
        {
            if (faction != Faction.None)
                factionList.Add(faction);
        }
        allFactions = factionList.ToArray();

        proto_TeamListRow.SetActive(false);
        ClearInstantiatedRows();

        InitModeToggles();
        dropdown_Map.onValueChanged.AddListener(OnMapSelected);

        ApplyMatchMode(MatchMode.OneOnOne);
    }

    protected override void Start()
    {
        base.Start();
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

    private void InitModeToggles()
    {
        var toggleGroup = inst_ModeToggles.GetComponent<ToggleGroup>();
        var modeIndex = 0;

        foreach (Transform child in inst_ModeToggles)
        {
            var toggle = child.GetComponent<Toggle>();
            if ((toggle == null) || (modeIndex >= MODE_ORDER.Length))
                continue;

            var mode = MODE_ORDER[modeIndex];

            toggle.group = toggleGroup;
            toggle.isOn = (modeIndex == 0);
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    ApplyMatchMode(mode);
            });

            modeIndex++;
        }
    }

    #endregion

    #region Map Selection

    private void RefreshMapDropdown()
    {
        filteredMaps = MapDataLoader.GetByMatchMode(MatchDataManager.Instance.Mode);
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
            MatchDataManager.Instance.Map = null;
            ApplyMapPreview(null);
            return;
        }

        MatchDataManager.Instance.Map = filteredMaps[index];
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
        var state = MatchDataManager.Instance;
        var config = MATCH_CONFIGS[mode];

        state.Mode = mode;
        state.Teams.Clear();
        state.Players.Clear();

        var playerIndex = 0;
        for (var t = 0; t < config.TeamCount; t++)
        {
            var team = new MatchTeam
            {
                Name = TEAM_DEFAULT_NAMES[t % TEAM_DEFAULT_NAMES.Length],
                Color = TEAM_COLORS[t % TEAM_COLORS.Length],
            };

            for (var p = 0; p < config.TeamSize; p++)
            {
                state.Players.Add(new MatchPlayer());
                team.PlayerIndices.Add(playerIndex++);
            }

            state.Teams.Add(team);
        }

        RebuildTeamUI();
        RefreshMapDropdown();
    }

    #endregion

    #region Team / Player UI

    private void RebuildTeamUI()
    {
        ClearInstantiatedRows();

        var state = MatchDataManager.Instance;
        for (var i = 0; i < state.Teams.Count; i += TEAMS_PER_ROW)
        {
            var rowGO = Instantiate(proto_TeamListRow, inst_TeamList);
            rowGO.SetActive(true);

            ClearChildren(rowGO.transform);

            var rowEndIndex = Mathf.Min(i + TEAMS_PER_ROW, state.Teams.Count);
            for (var t = i; t < rowEndIndex; t++)
            {
                var teamGO = Instantiate(proto_TeamSlot, rowGO.transform);
                teamGO.SetActive(true);
                BindTeamSlot(teamGO.transform, state.Teams[t]);
            }
        }
    }

    private void BindTeamSlot(Transform slot, MatchTeam team)
    {
        var label = slot.Find("Label").GetComponent<TextMeshProUGUI>();
        label.text = team.Name;
        label.color = team.Color;

        var container = slot.Find("PlayerSlots");
        ClearChildren(container);

        var state = MatchDataManager.Instance;
        foreach (var playerIndex in team.PlayerIndices)
        {
            var playerGO = Instantiate(proto_PlayerSlot, container);
            playerGO.SetActive(true);
            BindPlayerSlot(playerGO.transform, state.Players[playerIndex]);
        }
    }

    private void BindPlayerSlot(Transform slot, MatchPlayer player)
    {
        var nameInput = slot.Find("PlayerName").GetComponent<TMP_InputField>();
        nameInput.text = player.Name ?? "";
        nameInput.onValueChanged.AddListener(val => player.Name = val);

        if ((player.Faction == Faction.None) && (allFactions.Length > 0))
            player.Faction = allFactions[0];

        var factionImage = slot.Find("Faction").GetComponent<RawImage>();
        var heroImage = slot.Find("Hero").GetComponent<RawImage>();

        ApplyHeroPortrait(player, heroImage);

        AddClickHandler(factionImage.gameObject, () =>
        {
            CycleFaction(player);
            ApplyHeroPortrait(player, heroImage);
        });

        AddClickHandler(heroImage.gameObject, () =>
        {
            CycleHero(player);
            ApplyHeroPortrait(player, heroImage);
        });
    }

    #endregion

    #region Faction / Hero

    private void CycleFaction(MatchPlayer player)
    {
        if (allFactions.Length == 0)
            return;

        var factionIndex = System.Array.IndexOf(allFactions, player.Faction);
        factionIndex = (factionIndex + 1) % allFactions.Length;

        player.Faction = allFactions[factionIndex];
        player.HeroName = null;
    }

    private void CycleHero(MatchPlayer player)
    {
        var factionData = FactionDataLoader.Instance.GetByName(player.Faction.ToString());
        if (factionData == null)
            return;

        var heroes = factionData.Heroes;
        if (heroes.Count == 0)
            return;

        var heroIndex = heroes.FindIndex(h => h.Name == player.HeroName);
        heroIndex = (heroIndex + 1) % heroes.Count;

        player.HeroName = heroes[heroIndex].Name;
    }

    private void ApplyHeroPortrait(MatchPlayer player, RawImage image)
    {
        var factionData = FactionDataLoader.Instance.GetByName(player.Faction.ToString());
        var heroes = (factionData != null)
            ? factionData.Heroes
            : new List<HeroData>();

        var matchedHero = default(HeroData);
        if (player.HeroName != null)
            matchedHero = heroes.Find(h => h.Name == player.HeroName);

        if ((matchedHero != null) && (matchedHero.Portrait != null))
        {
            image.texture = matchedHero.Portrait.texture;
            image.color = Color.white;
        }
        else
        {
            image.texture = null;
            image.color = new Color(0.3f, 0.3f, 0.3f);
        }
    }

    #endregion

    #region Helpers

    private void ClearInstantiatedRows()
    {
        for (var i = inst_TeamList.childCount - 1; i >= 0; i--)
        {
            var child = inst_TeamList.GetChild(i).gameObject;
            if (child != proto_TeamListRow)
                Destroy(child);
        }
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

    #endregion
}
