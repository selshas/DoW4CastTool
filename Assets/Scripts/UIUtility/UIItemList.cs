using System.Collections.Generic;
using UnityEngine;

public class UIItemList<T>
{
    public delegate void BindHandler(Transform instance, T item);

    private readonly Transform container;
    private readonly GameObject prototype;
    private readonly BindHandler onBind;

    private readonly List<GameObject> instances = new List<GameObject>();

    private ObservableList<T> observedList;

    public int Count => instances.Count;

    /// <summary>
    /// Creates a UIItemList with static items, no observation.
    /// </summary>
    public UIItemList(Transform container, IList<T> items, BindHandler onBind)
    {
        this.container = container;
        this.onBind = onBind;

        prototype = container.GetChild(0).gameObject;
        prototype.SetActive(false);

        DestroyInstances();

        for (var i = 0; i < items.Count; i++)
            InstantiateItem(items[i], i);
    }

    /// <summary>
    /// Creates a UIItemList that observes the given list and updates UI on changes.
    /// </summary>
    public UIItemList(Transform container, ObservableList<T> items, BindHandler onBind) : base()
    {
        observedList = items;
        observedList.Changed += OnListChanged;
    }

    /// <summary>
    /// Clears all instantiated UI elements and stops observing.
    /// </summary>
    public void Clear()
    {
        StopObserving();
        DestroyInstances();
    }

    /// <summary>
    /// Updates the list with a new dataset. Reuses existing instances, instantiates more if needed, and disables excess.
    /// </summary>
    public void UpdateItems(IList<T> items)
    {
        var newCount = items.Count;

        for (var i = instances.Count; i < newCount; i++)
            InstantiateItem(items[i], i);

        for (var i = 0; i < newCount; i++)
        {
            instances[i].SetActive(true);
            onBind(instances[i].transform, items[i]);
        }

        for (var i = newCount; i < instances.Count; i++)
            instances[i].SetActive(false);
    }

    /// <summary>
    /// Returns the Transform of the instantiated element at the given index.
    /// </summary>
    public Transform GetInstance(int index)
    {
        return instances[index].transform;
    }

    private void OnListChanged(ObservableList<T>.ChangeType type, int index, T item)
    {
        switch (type)
        {
            case ObservableList<T>.ChangeType.Add:
                InstantiateItem(item, instances.Count);
                break;

            case ObservableList<T>.ChangeType.Insert:
                InstantiateItem(item, index);
                instances[index].transform.SetSiblingIndex(index + 1);
                break;

            case ObservableList<T>.ChangeType.Remove:
                Object.Destroy(instances[index]);
                instances.RemoveAt(index);
                break;

            case ObservableList<T>.ChangeType.Replace:
                onBind(instances[index].transform, item);
                break;

            case ObservableList<T>.ChangeType.Clear:
                DestroyInstances();
                break;
        }
    }

    private void InstantiateItem(T item, int index)
    {
        var instanceObject = Object.Instantiate(prototype, container);
        instanceObject.SetActive(true);
        onBind(instanceObject.transform, item);
        instances.Insert(index, instanceObject);
    }

    private void StopObserving()
    {
        if (observedList != null)
        {
            observedList.Changed -= OnListChanged;
            observedList = null;
        }
    }

    private void DestroyInstances()
    {
        // Remain prototype
        for (var i = (container.childCount - 1); i > 0; i--)
        {
            var child = container.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }

        instances.Clear();
    }
}
