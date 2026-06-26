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
    /// Removes a name from the known list and saves to disk.
    /// </summary>
    public bool RemoveName(string name)
    {
        var removed = knownNames.Remove(name);
        if (removed)
            Save();

        return removed;
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
    /// Finds up to count candidate names matching the keyword. Prioritizes prefix matches via sorted range lookup, then fills remaining slots with substring matches. Reuses the provided list to avoid GC allocation. Returns the number of candidates found.
    /// </summary>
    public int FindCandidates(string keyword, int count, ref List<string> results)
    {
        if (results == null)
            results = new List<string>(count);
        else
            results.Clear();

        if (count <= 0 || string.IsNullOrEmpty(keyword))
            return 0;

        var upperBound = keyword + char.MaxValue;
        var prefixRange = knownNames.GetViewBetween(keyword, upperBound);

        foreach (var name in prefixRange)
        {
            if (results.Count >= count)
                return count;

            results.Add(name);
        }

        if (results.Count >= count)
            return count;

        foreach (var name in knownNames)
        {
            if (results.Count >= count)
                break;

            if (name.StartsWith(keyword, System.StringComparison.OrdinalIgnoreCase))
                continue;

            if (name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                results.Add(name);
        }

        return results.Count;
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
