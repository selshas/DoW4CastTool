using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Loads map data from StreamingAssets/Maps/.
// Each map is a subfolder whose name becomes the map name:
//
//   StreamingAssets/Maps/
//     {Map Name}/
//       minimap.png                — minimap image (png or jpg)
//       matchmode_whitelist.txt    — supported match modes, one tag per line
//       maxplayercount.txt           — number of start points (single integer)
//
// matchmode_whitelist.txt tags (case-insensitive):
//   1v1, 2v2, 3v3, FreeForAll, FFA
//
// Example:
//   StreamingAssets/Maps/Gorka Valley/
//     minimap.png
//     matchmode_whitelist.txt  →  1v1
//     maxplayercount.txt          →  2
public static class MapDataLoader
{
    public static List<MapData> Maps { get; private set; } = new List<MapData>();
    public static Texture2D NotFoundTexture { get; private set; }

    private static readonly Dictionary<string, MatchMode> MatchModeTagLookup = new Dictionary<string, MatchMode>()
    {
        { "1v1",        MatchMode.OneOnOne },
        { "2v2",        MatchMode.TwoOnTwo },
        { "3v3",        MatchMode.ThreeOnThree },
        { "freeforall", MatchMode.FreeForAll },
        { "ffa",        MatchMode.FreeForAll },
    };

    public static void Load()
    {
        Maps.Clear();

        var root = Path.Combine(Application.streamingAssetsPath, "Maps");
        if (!Directory.Exists(root))
            return;

        NotFoundTexture = LoadNotFoundTexture(root);

        var mapDirs = Directory.GetDirectories(root);
        System.Array.Sort(mapDirs);

        foreach (var dir in mapDirs)
        {
            var mapData = LoadMap(dir);
            if (mapData != null)
                Maps.Add(mapData);
        }
    }

    /// <summary>
    /// Filters maps by match mode whitelist and total player count against start points.
    /// </summary>
    public static List<MapData> GetByFilter(MatchMode mode, int playerCount)
    {
        var result = new List<MapData>();
        foreach (var map in Maps)
        {
            if ((map.MatchMode & mode) == 0)
                continue;

            if (map.MaxPlayerCount < playerCount)
                continue;

            result.Add(map);
        }
        return result;
    }

    private static MapData LoadMap(string dirPath)
    {
        var mapName = Path.GetFileName(dirPath);

        var texturePath = FindImageFile(dirPath);
        if (texturePath == null)
            return null;

        var bytes = File.ReadAllBytes(texturePath);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
            return null;

        tex.name = mapName;

        var matchMode = MatchMode.None;
        var whitelistPath = Path.Combine(dirPath, "matchmode_whitelist.txt");
        if (File.Exists(whitelistPath))
        {
            var lines = File.ReadAllLines(whitelistPath);
            foreach (var line in lines)
            {
                var tag = line.Trim().ToLowerInvariant();
                if (tag.Length == 0)
                    continue;

                if (MatchModeTagLookup.TryGetValue(tag, out var mode))
                    matchMode |= mode;
            }
        }

        var startPointCount = 0;
        var startPointsPath = Path.Combine(dirPath, "maxplayercount.txt");
        if (File.Exists(startPointsPath))
        {
            var text = File.ReadAllText(startPointsPath).Trim();
            int.TryParse(text, out startPointCount);
        }

        return new MapData
        {
            Name = mapName,
            MinimapTexture = tex,
            MatchMode = matchMode,
            MaxPlayerCount = startPointCount,
        };
    }

    /// <summary>
    /// Loads the NotFound.png fallback texture from the Maps root directory.
    /// </summary>
    private static Texture2D LoadNotFoundTexture(string mapsRoot)
    {
        var path = Path.Combine(mapsRoot, "NotFound.png");
        if (!File.Exists(path))
            return null;

        var bytes = File.ReadAllBytes(path);
        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(bytes))
            return null;

        tex.name = "NotFound";
        return tex;
    }

    private static string FindImageFile(string dirPath)
    {
        var extensions = new[] { "minimap.png", "minimap.jpg" };
        foreach (var name in extensions)
        {
            var path = Path.Combine(dirPath, name);
            if (File.Exists(path))
                return path;
        }
        return null;
    }
}
