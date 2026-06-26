using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for scene-level app controllers that manage UtilityApp toggling via index-paired Toggles.
/// </summary>
public abstract class AppController : UtilityAppBase
{
    public UtilityAppBase[] Apps;
    public bool AllowAllOff = true;
    [SerializeField] private Transform appToggleContainer;

    private Toggle[] toggles;

    /// <summary>
    /// Caches Toggle components from appToggleContainer children and wires onValueChanged by index.
    /// </summary>
    protected void InitializeToggles()
    {
        toggles = appToggleContainer.GetComponentsInChildren<Toggle>(true);
        for (var i = 0; i < Apps.Length; i++)
        {
            toggles[i].SetIsOnWithoutNotify(Apps[i].gameObject.activeSelf);
            var index = i;
            toggles[i].onValueChanged.AddListener((active) => SetAppActive(index, active));
        }
    }

    /// <summary>
    /// Sets the app at the given index active or inactive, syncing its toggle and enforcing exclusion groups.
    /// </summary>
    public void SetAppActive(int index, bool active)
    {
        if (!active && !AllowAllOff)
        {
            var hasOtherActive = false;
            for (var i = 0; i < Apps.Length; i++)
            {
                if (i != index && Apps[i].gameObject.activeSelf)
                {
                    hasOtherActive = true;
                    break;
                }
            }

            if (!hasOtherActive)
            {
                toggles[index].SetIsOnWithoutNotify(true);
                return;
            }
        }

        Apps[index].gameObject.SetActive(active);
        toggles[index].SetIsOnWithoutNotify(active);

        if (active && Apps[index].ExclusionGroup >= 0)
        {
            var group = Apps[index].ExclusionGroup;
            for (var i = 0; i < Apps.Length; i++)
            {
                if ((i != index) && (Apps[i].ExclusionGroup == group))
                {
                    Apps[i].gameObject.SetActive(false);
                    toggles[i].SetIsOnWithoutNotify(false);
                }
            }
        }
    }

    /// <summary>
    /// Sets the first app matching type T active or inactive.
    /// </summary>
    public void SetAppActive<T>(bool active) where T : UtilityAppBase
    {
        for (var i = 0; i < Apps.Length; i++)
        {
            if (Apps[i] is T)
            {
                SetAppActive(i, active);
                return;
            }
        }
    }

    /// <summary>
    /// Toggles the first app matching type T.
    /// </summary>
    public void ToggleApp<T>() where T : UtilityAppBase
    {
        for (var i = 0; i < Apps.Length; i++)
        {
            if (Apps[i] is T)
            {
                SetAppActive(i, !Apps[i].gameObject.activeSelf);
                return;
            }
        }
    }

    /// <summary>
    /// Returns the first app matching type T, or null if not found.
    /// </summary>
    public T GetApp<T>() where T : UtilityAppBase
    {
        for (var i = 0; i < Apps.Length; i++)
        {
            if (Apps[i] is T result)
                return result;
        }
        return null;
    }
}
