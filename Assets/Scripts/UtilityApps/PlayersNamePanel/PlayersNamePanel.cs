using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayersNamePanel : MonoBehaviour
{
    public List<Sprite> MiniPortraitSprites = new List<Sprite>();

    public List<Button> MiniPortraits = new List<Button>();

    public class MiniPortrait : MonoBehaviour
    {
        public PlayersNamePanel PlayersNamePanel;
        public int CurrentIndex = 0;

        private Image imgae;

        private void Awake()
        {
            imgae = GetComponent<Image>();
        }

        public void SwapSprite()
        { 
            var num_factionPortraits = PlayersNamePanel.MiniPortraitSprites.Count;
            if (num_factionPortraits == 0)
                return;

            CurrentIndex = (CurrentIndex + 1) % num_factionPortraits;

            var sprite = PlayersNamePanel.MiniPortraitSprites[CurrentIndex];
            imgae.sprite = sprite;
        }
    }

    private void Awake()
    {
        foreach (var button in MiniPortraits)
        {
            var miniPortrait = button.gameObject.AddComponent<MiniPortrait>();
            miniPortrait.PlayersNamePanel = this;
            button.onClick.AddListener(() => miniPortrait.SwapSprite());
        }
    }
}
