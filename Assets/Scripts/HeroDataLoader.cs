using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Loads hero data from StreamingAssets/Heroes/.
// Each faction is a subfolder, each hero is a subfolder within:
//
//   StreamingAssets/Heroes/
//     {FactionName}/
//       {HeroName}/
//         portrait.png (or .jpg)
//
// Example:
//   StreamingAssets/Heroes/
//     SpaceMarine/
//       0/
//         portrait.png
//       1/
//         portrait.png
//     Ork/
//       0/
//         portrait.png
public static class HeroDataLoader
{
    public static Dictionary<string, List<HeroData>> Heroes { get; private set; } = new Dictionary<string, List<HeroData>>();

    public static void Load()
    {
        Heroes.Clear();

        var root = Path.Combine(Application.streamingAssetsPath, "Heroes");
        if (!Directory.Exists(root))
            return;

        var factionDirs = Directory.GetDirectories(root);
        System.Array.Sort(factionDirs);

        foreach (var dir in factionDirs)
        {
            var factionName = Path.GetFileName(dir);
            var heroList = LoadFaction(dir, factionName);
            Heroes[factionName] = heroList;
        }
    }

    public static List<HeroData> GetByFaction(string factionName)
    {
        if (Heroes.TryGetValue(factionName, out var list))
            return list;

        return new List<HeroData>();
    }

    private static List<HeroData> LoadFaction(string factionDirPath, string factionName)
    {
        var heroes = new List<HeroData>();

        var heroDirs = Directory.GetDirectories(factionDirPath);
        System.Array.Sort(heroDirs);

        foreach (var heroDir in heroDirs)
        {
            var heroData = LoadHero(heroDir, factionName);
            if (heroData != null)
                heroes.Add(heroData);
        }

        return heroes;
    }

    private static HeroData LoadHero(string heroDirPath, string factionName)
    {
        var portraitPath = FindImageFile(heroDirPath);
        if (portraitPath == null)
            return null;

        var bytes = File.ReadAllBytes(portraitPath);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
            return null;

        var heroName = Path.GetFileName(heroDirPath);
        tex.name = $"{factionName}_{heroName}";

        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sprite.name = tex.name;

        return new HeroData
        {
            Name = heroName,
            FactionName = factionName,
            Portrait = sprite,
        };
    }

    private static string FindImageFile(string dirPath)
    {
        var names = new[] { "portrait.png", "portrait.jpg" };
        foreach (var name in names)
        {
            var path = Path.Combine(dirPath, name);
            if (File.Exists(path))
                return path;
        }
        return null;
    }
}
