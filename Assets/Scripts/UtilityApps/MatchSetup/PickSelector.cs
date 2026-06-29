using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public abstract class PickSelector<T> : SingletonBehaviour<PickSelector<T>>
{
    protected Toggle toggle_openner;
    protected MatchPlayer playerData;

    protected RectTransform rectTransform;
    protected ToggleGroup toggleGroup;
    protected UIItemList<T> itemList;

    protected override void OnInitialize()
    {
        rectTransform = GetComponent<RectTransform>();
        toggleGroup = GetComponent<ToggleGroup>();
        gameObject.SetActive(false);
    }

    public void LoadData(IList<T> dataSet)
    {
        if (itemList != null)
            itemList.Clear();

        itemList = new UIItemList<T>(transform, dataSet, OnOptionLoaded);
    }

    public void Open(MatchPlayer playerData, Toggle opener)
    {
        toggle_openner = opener;
        this.playerData = playerData;

        var dataSet = CollectData();
        LoadData(dataSet);

        var cornerBuffer = new Vector3[4];

        opener.transform.GetComponent<RectTransform>().GetWorldCorners(cornerBuffer);
        var anchorPos = cornerBuffer[0];
        anchorPos.z = 0;

        gameObject.SetActive(true);
        rectTransform.position = anchorPos;
    }

    /// <summary>
    /// Closes the selector when the user clicks outside its hierarchy.
    /// </summary>
    private void Update()
    {
        var scrolls = toggle_openner.GetComponentsInParent<ScrollRect>();
        var isScrolling = scrolls.Any(x => x.velocity.x != 0 || x.velocity.y != 0);
        if (isScrolling)
        {
            Close();
            return;
        }

        if (!Mouse.current.leftButton.wasReleasedThisFrame)
            return;

        if (UIRaycastTracker.Instance.IsHit(toggle_openner.transform))
            return;

        if (UIRaycastTracker.Instance.IsHit(transform))
            return;

        Close();
    }

    /// <summary>
    /// Hides the selector.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        toggle_openner.SetIsOnWithoutNotify(false);
    }

    protected abstract IList<T> CollectData();
    protected abstract void OnOptionLoaded(Transform child, T data);
}