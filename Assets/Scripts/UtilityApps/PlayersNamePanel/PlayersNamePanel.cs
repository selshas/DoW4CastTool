using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayersNamePanel : UtilityAppBase
{
    public List<Sprite> MiniPortraitSprites = new List<Sprite>();

    public List<Button> MiniPortraits = new List<Button>();

    public class MiniPortrait : MonoBehaviour
    {
        public PlayersNamePanel PlayersNamePanel;
        public int CurrentIndex = 0;

        private Image image;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        public void SwapSprite()
        {
            var num_factionPortraits = PlayersNamePanel.MiniPortraitSprites.Count;
            if (num_factionPortraits == 0)
                return;

            CurrentIndex = (CurrentIndex + 1) % num_factionPortraits;

            var sprite = PlayersNamePanel.MiniPortraitSprites[CurrentIndex];
            image.sprite = sprite;
        }
    }

    /// <summary>
    /// No hotkey bindings for PlayersNamePanel.
    /// </summary>
    public override void InitializeInputs() { }

    private void Awake()
    {
        LoadPortraitSprites();

        foreach (var button in MiniPortraits)
        {
            var miniPortrait = button.gameObject.AddComponent<MiniPortrait>();
            miniPortrait.PlayersNamePanel = this;
            button.onClick.AddListener(() => miniPortrait.SwapSprite());

            if (MiniPortraitSprites.Count > 0)
                button.GetComponent<Image>().sprite = MiniPortraitSprites[0];
        }
    }

    /// <summary> Populates emblem sprites from FactionDataLoader. </summary>
    private void LoadPortraitSprites()
    {
        MiniPortraitSprites.Clear();

        foreach (var factionData in FactionDataLoader.Instance.Factions.Values)
        {
            if (factionData.Symbol != null)
                MiniPortraitSprites.Add(factionData.Symbol);
        }
    }
}
