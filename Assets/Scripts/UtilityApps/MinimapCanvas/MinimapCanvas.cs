using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using SharpHook.Native;
using static GlobalInputSystem;
using DeviceType = GlobalInputSystem.DeviceType;
using KeyCode = SharpHook.Native.KeyCode;

public class MinimapCanvas : DrawableCanvas
{
    public RawImage RawImg_MinimapCanvas_Background;

    public Texture2D Tex_NotFound;

    public int CurrentPlayerCount = 2;

    public List<int> PlayerCounts = new List<int>()
    { 
        2,
        4,
        6,
    };

    public Dictionary<string, Texture2D> TexturesLookup = new Dictionary<string, Texture2D>();
    public List<Texture2D> Textures_minimap = new List<Texture2D>();

    public Dictionary<int, TMP_Dropdown> dropdowns_maps = new Dictionary<int, TMP_Dropdown>();

    public Transform Transform_DropdownGroup_Maps;

    protected override void Awake()
    {
        base.Awake();

        #region Load and Init Minimap Textures
        foreach (var texture in Textures_minimap)
        {
            var mapName = texture.name.Trim();
            if (TexturesLookup.ContainsKey(mapName))
                continue;

#if UNITY_EDITOR
            Debug.Log($"Load Minimap Texture for: {mapName}");
#endif

            TexturesLookup.Add(mapName, texture);
        }

        foreach (var dropdown in Transform_DropdownGroup_Maps.GetComponentsInChildren<TMP_Dropdown>())
        {
            if (!int.TryParse(dropdown.gameObject.name, out var playerCount))
                continue;
            
            if (!PlayerCounts.Contains(playerCount))
                continue;

            dropdowns_maps.Add(playerCount, dropdown);
        }
        #endregion Load and Init Minimap Textures

        ChangePlayerCount(0);
    }

    public override void InitializeInputs()
    {
        base.InitializeInputs();

        AddInputCmd(
            DeviceType.Keyboard, (uint)KeyCode.VcTab,
            InputState.Pressed,
            (self) =>
            {
                GlobalAppController.Instance.ToggleApp_MinimapCanvas();
                GlobalAppController.Instance.ToggleApp_ScreenCanvas();
            }
        );
    }

    public void ChangePlayerCount(int i)
    {
        var playerCount = PlayerCounts[i];

        CurrentPlayerCount = playerCount;

        foreach ((var key, var dropdown) in dropdowns_maps)
        {
            dropdown.gameObject.SetActive(key == playerCount);
        }

        ChangeMap(0);
    }

    public void ChangeMap(int i)
    {
        Clear();

        var texture_minimap = Tex_NotFound;

        var dropdown = dropdowns_maps[CurrentPlayerCount];
        if (dropdown.options.Count != 0 && dropdown.options[i].text != "")
        {
            var mapName = dropdown.options[i].text;

            texture_minimap = TexturesLookup[mapName];

#if UNITY_EDITOR
            Debug.Log($"ChangeMap: {mapName}");
#endif
        }

        RawImg_MinimapCanvas_Background.texture = texture_minimap;
    }
}
