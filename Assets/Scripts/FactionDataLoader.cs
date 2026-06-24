using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

// Loads faction data from StreamingAssets/Factions/.
//
//   StreamingAssets/Factions/
//     {FactionName}/
//       Emblem.png (or .jpg/.jpeg)
//       Heroes/
//         {HeroName}/
//           Portrait.png (or .jpg/.jpeg)
public class FactionDataLoader : MonoBehaviour
{
    public static FactionDataLoader Instance { get; private set; }

    public bool IsLoaded { get; private set; } = false;
    public Dictionary<string, FactionData> Factions { get; private set; } = new Dictionary<string, FactionData>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();

        IsLoaded = true;
    }

    /// <summary> Returns the FactionData for the given name, or null if not found. </summary>
    public FactionData GetByName(string factionName)
    {
        if (Factions.TryGetValue(factionName, out var data))
            return data;

        return null;
    }

    /// <summary> Loads all faction data from StreamingAssets/Factions/. </summary>
    private void Load()
    {
        Factions.Clear();

        var root = Path.Combine(Application.streamingAssetsPath, "Factions");
        if (!Directory.Exists(root))
            return;

        var factionDirs = Directory.GetDirectories(root);
        System.Array.Sort(factionDirs);

        foreach (var dir in factionDirs)
        {
            var factionName = Path.GetFileName(dir);
            var factionData = LoadFaction(dir, factionName);
            Factions[factionName] = factionData;
        }
    }

    /// <summary> Loads a single faction's emblem and hero list from its directory. </summary>
    private static FactionData LoadFaction(string factionDirPath, string factionName)
    {
        var emblem = LoadSprite(factionDirPath, "Emblem", factionName);
        var heroes = LoadHeroes(factionDirPath, factionName);

        return new FactionData
        {
            Name = factionName,
            Emblem = emblem,
            Heroes = heroes,
        };
    }

    /// <summary> Loads all hero data from a faction's Heroes subdirectory. </summary>
    private static Dictionary<string, HeroData> LoadHeroes(string factionDirPath, string factionName)
    {
        var heroes = new Dictionary<string, HeroData>();

        var heroesDir = Path.Combine(factionDirPath, "Heroes");
        if (!Directory.Exists(heroesDir))
            return heroes;

        var heroDirs = Directory.GetDirectories(heroesDir);
        System.Array.Sort(heroDirs);

        foreach (var heroDir in heroDirs)
        {
            var heroName = Path.GetFileName(heroDir);
            var portrait = LoadSprite(heroDir, "Portrait", $"{factionName}_{heroName}");
            if (portrait == null)
                continue;

            heroes.Add(heroName, new HeroData
            {
                Name = heroName,
                FactionName = factionName,
                Portrait = portrait,
            });
        }

        return heroes;
    }

    /// <summary> Loads an image file as a Sprite from the given directory and base name. </summary>
    private static Sprite LoadSprite(string dirPath, string baseName, string spriteName)
    {
        var imagePath = FindImageFile(dirPath, baseName);
        if (imagePath == null)
            return null;

        var bytes = File.ReadAllBytes(imagePath);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
            return null;

        tex.name = spriteName;
        var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sprite.name = spriteName;

        return sprite;
    }

    /// <summary> Finds an image file with the given base name trying common extensions. </summary>
    private static string FindImageFile(string dirPath, string baseName)
    {
        var extensions = new[] { ".png", ".jpg", ".jpeg" };
        foreach (var ext in extensions)
        {
            var path = Path.Combine(dirPath, baseName + ext);
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    public string GetRandomFactionName()
    {
        var factionNames = Factions.Keys.ToArray();
        var factionCount = Factions.Count;

        return factionNames[Random.Range(0, factionCount)];
    }

    public string[] GetHeroNames(string factionName)
    {
        if (!Factions.TryGetValue(factionName, out var faction))
            throw new System.Exception($"FactionName '{factionName}' does not exist");

        return faction.Heroes.Keys.OrderBy(x => x).ToArray();
    }

    public string GetRandomHeroName(string factionName)
    {
        if (!Factions.TryGetValue(factionName, out var faction))
            throw new System.Exception($"FactionName '{factionName}' does not exist");

        var heroNames = faction.Heroes.Keys.ToArray();
        var heroCount = faction.Heroes.Count;

        return heroNames[Random.Range(0, heroCount)];
    }
}
