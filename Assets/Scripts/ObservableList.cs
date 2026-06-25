using System;
using System.Collections;
using System.Collections.Generic;

public class ObservableList<T> : IList<T>
{
    public enum ChangeType
    {
        Add,
        Insert,
        Remove,
        Replace,
        Clear,
    }

    public delegate void ChangeHandler(ChangeType type, int index, T item);

    public event ChangeHandler Changed;

    private readonly List<T> inner = new List<T>();
    private readonly List<int> hashes = new List<int>();

    public int Count => inner.Count;
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => inner[index];
        set
        {
            inner[index] = value;
            hashes[index] = value.GetHashCode();
            Changed?.Invoke(ChangeType.Replace, index, value);
        }
    }

    /// <summary>
    /// Adds an item and notifies observers.
    /// </summary>
    public void Add(T item)
    {
        inner.Add(item);
        hashes.Add(item.GetHashCode());
        Changed?.Invoke(ChangeType.Add, inner.Count - 1, item);
    }

    /// <summary>
    /// Inserts an item at the specified index and notifies observers.
    /// </summary>
    public void Insert(int index, T item)
    {
        inner.Insert(index, item);
        hashes.Insert(index, item.GetHashCode());
        Changed?.Invoke(ChangeType.Insert, index, item);
    }

    /// <summary>
    /// Removes the first occurrence of an item and notifies observers.
    /// </summary>
    public bool Remove(T item)
    {
        var index = inner.IndexOf(item);
        if (index < 0)
            return false;

        inner.RemoveAt(index);
        hashes.RemoveAt(index);
        Changed?.Invoke(ChangeType.Remove, index, item);
        return true;
    }

    /// <summary>
    /// Removes the item at the specified index and notifies observers.
    /// </summary>
    public void RemoveAt(int index)
    {
        var item = inner[index];
        inner.RemoveAt(index);
        hashes.RemoveAt(index);
        Changed?.Invoke(ChangeType.Remove, index, item);
    }

    /// <summary>
    /// Clears all items and notifies observers.
    /// </summary>
    public void Clear()
    {
        inner.Clear();
        hashes.Clear();
        Changed?.Invoke(ChangeType.Clear, -1, default);
    }

    /// <summary>
    /// Compares stored hashes against current values and fires Replace for any changed items.
    /// </summary>
    public void DetectChanges()
    {
        for (var i = 0; i < inner.Count; i++)
        {
            var item = inner[i];
            var currentHash = item.GetHashCode();
            if (currentHash == hashes[i])
                continue;

            hashes[i] = currentHash;
            Changed?.Invoke(ChangeType.Replace, i, item);
        }
    }

    /// <summary>
    /// Returns the index of the specified item.
    /// </summary>
    public int IndexOf(T item) => inner.IndexOf(item);

    /// <summary>
    /// Returns whether the list contains the specified item.
    /// </summary>
    public bool Contains(T item) => inner.Contains(item);

    /// <summary>
    /// Copies the list contents to the target array.
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex) => inner.CopyTo(array, arrayIndex);

    /// <summary>
    /// Returns an enumerator over the list.
    /// </summary>
    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();

    /// <summary>
    /// Returns an enumerator over the list.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();
}
