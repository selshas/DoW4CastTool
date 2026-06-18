using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class PlayersNamePanel : MonoBehaviour
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

    private void LoadPortraitSprites()
    {
        MiniPortraitSprites.Clear();

        var path = Path.Combine(Application.streamingAssetsPath, "FactionPortraits");
        if (!Directory.Exists(path))
            return;

        var files = Directory.GetFiles(path, "*.png");
        System.Array.Sort(files);

        foreach (var file in files)
        {
            var bytes = File.ReadAllBytes(file);
            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(bytes))
                continue;

            tex.name = Path.GetFileNameWithoutExtension(file);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sprite.name = tex.name;
            MiniPortraitSprites.Add(sprite);
        }
    }
}
