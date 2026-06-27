using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngamePlayerPlate : MonoBehaviour, IPointerClickHandler
{
    public int PlayerIndex 
    {
        get => playerIndex;
        set => LoadPlayerData(value);
    }
    private int playerIndex;

    private MatchPlayer playerData;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RawImage factionSymbol;
    [SerializeField] private RawImage heroPortrait;

    private IngameTeamPlate teamPlate;

    private UIEffector_Greyscale greyscaleEffector;

    private bool dimmed;
    public bool Dimmed => dimmed;

    /// <summary>
    /// Caches all child Graphics, their original colors and materials, and prepares the greyscale material.
    /// </summary>
    private void Awake()
    {
        teamPlate = GetComponentInParent<IngameTeamPlate>();
        greyscaleEffector = GetComponent<UIEffector_Greyscale>();
    }

    private void LoadPlayerData(int playerIndex)
    {
        this.playerIndex = playerIndex;
        playerData = MatchDataManager.Instance.Players[playerIndex];

        nameText.text = playerData.Name;
        scoreText.text = playerData.Score.ToString();

        var factionData = FactionDataLoader.Instance.GetByName(playerData.FactionName);
        if (factionData == null || factionData.Heroes.TryGetValue(playerData.HeroName, out var heroData))
        {
            factionSymbol.texture = FactionDataLoader.Instance.FactionSymbolPlaceholderTexture;
            heroPortrait.texture = FactionDataLoader.Instance.HeroPortraitPlaceholderTexture;

            return;
        }

        factionSymbol.texture = factionData.SymbolTexture;
        heroPortrait.texture = heroData.PortraitTexture;
    }

    /// <summary>
    /// Toggles dark-greyscale dim on left click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        dimmed = !dimmed;

        greyscaleEffector.enabled = !greyscaleEffector.enabled;

        if (teamPlate != null)
            teamPlate.EvaluateDimState();
    }
}
