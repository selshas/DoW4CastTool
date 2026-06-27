using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameTeamPlate : MonoBehaviour
{
    [SerializeField] private float dimming = 0.4f;
    [SerializeField] private RectTransform playersContainer;

    private IngamePlayerPlate[] plates;
    private Graphic[] teamGraphics;
    private Color[] originalColors;
    private Material[] originalMaterials;
    private bool[] isTmpGraphic;
    private bool dimmed;

    private static Material greyscaleMaterial;
    private static GameObject playerPlatePrefab;

    /// <summary>
    /// Initializes the greyscale material and loads the player plate prefab.
    /// </summary>
    private void Awake()
    {
        if (greyscaleMaterial == null)
        {
            var shader = Shader.Find("CFLTool/shd_uiGreyscale");
            greyscaleMaterial = new Material(shader);
        }

        if (playerPlatePrefab == null)
            playerPlatePrefab = Resources.Load<GameObject>("Prefab/PlayerNamePanel/IngamePlayerPlate");
    }

    /// <summary>
    /// Spawns player plates matching the team's player count and caches graphics for dimming.
    /// </summary>
    public void SetupTeam(int teamIndex)
    {
        for (var i = playersContainer.childCount - 1; i >= 0; i--)
            Destroy(playersContainer.GetChild(i).gameObject);

        var team = MatchDataManager.Instance.Teams[teamIndex];
        for (var i = 0; i < team.PlayerCount; i++)
            Instantiate(playerPlatePrefab, playersContainer);

        plates = playersContainer.GetComponentsInChildren<IngamePlayerPlate>(true);

        var filteredGraphics = new List<Graphic>();
        foreach (var graphic in GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.GetComponentInParent<IngamePlayerPlate>() == null)
                filteredGraphics.Add(graphic);
        }
        teamGraphics = filteredGraphics.ToArray();

        originalColors = new Color[teamGraphics.Length];
        originalMaterials = new Material[teamGraphics.Length];
        isTmpGraphic = new bool[teamGraphics.Length];

        for (var i = 0; i < teamGraphics.Length; i++)
        {
            originalColors[i] = teamGraphics[i].color;
            originalMaterials[i] = teamGraphics[i].material;
            isTmpGraphic[i] = teamGraphics[i] is TMP_Text;
        }
    }

    /// <summary>
    /// Re-evaluates whether the team plate should be dimmed based on child player plates.
    /// </summary>
    public void EvaluateDimState()
    {
        var allDimmed = true;
        foreach (var plate in plates)
        {
            if (plate.gameObject.activeInHierarchy && !plate.Dimmed)
            {
                allDimmed = false;
                break;
            }
        }

        if (allDimmed == dimmed)
            return;

        dimmed = allDimmed;
        ApplyDimState();
    }

    /// <summary>
    /// Applies or removes the dark-greyscale effect on team-level graphics.
    /// </summary>
    private void ApplyDimState()
    {
        for (var i = 0; i < teamGraphics.Length; i++)
        {
            if (dimmed)
            {
                if (isTmpGraphic[i])
                {
                    var orig = originalColors[i];
                    var lum = 0.299f * orig.r + 0.587f * orig.g + 0.114f * orig.b;
                    teamGraphics[i].color = new Color(lum * dimming, lum * dimming, lum * dimming, orig.a);
                }
                else
                {
                    teamGraphics[i].material = greyscaleMaterial;
                }
            }
            else
            {
                if (isTmpGraphic[i])
                    teamGraphics[i].color = originalColors[i];
                else
                    teamGraphics[i].material = originalMaterials[i];
            }
        }
    }
}
