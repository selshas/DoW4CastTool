using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Loads map data from StreamingAssets/Maps/.
// Each map is a subfolder whose name becomes the map name:
//
//   StreamingAssets/Maps/
//     {Map Name}/
//       minimap.png        — minimap image (png or jpg)
//       matchmode.txt      — supported match modes, one tag per line
//
// matchmode.txt tags (case-insensitive):
//   1v1, 2v2, 3v3, FreeForAll, FFA
//
// Example:
//   StreamingAssets/Maps/Gorka Valley/
//     minimap.png
//     matchmode.txt  →  1v1
//                       2v2
public static class MapDataLoader
{
    public static List<MapData> Maps { get; private set; } = new List<MapData>();

    private static readonly Dictionary<string, eMatchMode> MatchModeTagLookup = new Dictionary<string, eMatchMode>()
    {
        { "1v1",        eMatchMode.OneOnOne },
        { "2v2",        eMatchMode.TwoOnTwo },
        { "3v3",        eMatchMode.ThreeOnThree },
        { "freeforall", eMatchMode.FreeForAll },
        { "ffa",        eMatchMode.FreeForAll },
    };

    public static void Load()
    {
        Maps.Clear();

        var root = Path.Combine(Application.streamingAssetsPath, "Maps");
        if (!Directory.Exists(root))
            return;

        var mapDirs = Directory.GetDirectories(root);
        System.Array.Sort(mapDirs);

        foreach (var dir in mapDirs)
        {
            var mapData = LoadMap(dir);
            if (mapData != null)
                Maps.Add(mapData);
        }
    }

    public static List<MapData> GetByMatchMode(eMatchMode mode)
    {
        var result = new List<MapData>();
        foreach (var map in Maps)
        {
            if ((map.MatchMode & mode) != 0)
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

        var matchMode = eMatchMode.None;
        var matchModePath = Path.Combine(dirPath, "matchmode.txt");
        if (File.Exists(matchModePath))
        {
            var lines = File.ReadAllLines(matchModePath);
            foreach (var line in lines)
            {
                var tag = line.Trim().ToLowerInvariant();
                if (tag.Length == 0)
                    continue;

                if (MatchModeTagLookup.TryGetValue(tag, out var mode))
                    matchMode |= mode;
            }
        }

        return new MapData
        {
            Name = mapName,
            MinimapTexture = tex,
            MatchMode = matchMode,
        };
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
