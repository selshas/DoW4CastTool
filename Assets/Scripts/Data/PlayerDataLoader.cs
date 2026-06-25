using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerDataLoader : SingletonBehaviour<PlayerDataLoader>
{
    private static readonly string FilePath = Path.Combine(Application.streamingAssetsPath, "PlayerNames.txt");

    private readonly SortedSet<string> knownNames = new SortedSet<string>(System.StringComparer.OrdinalIgnoreCase);

    public bool IsLoaded { get; private set; } = false;
    public IReadOnlyCollection<string> KnownNames => knownNames;

    /// <summary>
    /// Loads known player names from disk on initialization.
    /// </summary>
    protected override void OnInitialize()
    {
        Load();

        IsLoaded = true;
    }

    /// <summary>
    /// Reads player names from the text file.
    /// </summary>
    private void Load()
    {
        knownNames.Clear();

        if (!File.Exists(FilePath))
            return;

        var lines = File.ReadAllLines(FilePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
                knownNames.Add(trimmed);
        }
    }

    /// <summary>
    /// Adds names that don't already exist and saves the sorted list to disk.
    /// </summary>
    public void MergeNames(IEnumerable<string> names)
    {
        foreach (var name in names)
        {
            var trimmed = name.Trim();
            if (trimmed.Length > 0)
                knownNames.Add(trimmed);
        }

        Save();
    }

    /// <summary>
    /// Writes the sorted name list to disk.
    /// </summary>
    private void Save()
    {
        var directory = Path.GetDirectoryName(FilePath);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllLines(FilePath, knownNames);
    }
}
