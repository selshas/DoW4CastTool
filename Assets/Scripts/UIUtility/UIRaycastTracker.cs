using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Tracks which UI elements are under the cursor each frame via EventSystem raycast.
/// </summary>
[DefaultExecutionOrder(-100)]
public class UIRaycastTracker : GlobalSingletonBehaviour<UIRaycastTracker>
{
    public readonly List<RaycastResult> Results = new List<RaycastResult>();

    public int ResultCount => Results.Count;

    /// <summary>
    /// Raycasts all active canvases at the current cursor position.
    /// </summary>
    private void Update()
    {
        Results.Clear();

        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return;

        var ped = new PointerEventData(eventSystem)
        {
            position = Mouse.current.position.ReadValue()
        };

        eventSystem.RaycastAll(ped, Results);
    }

    /// <summary>
    /// Returns whether any raycast hit belongs to the given transform's hierarchy.
    /// </summary>
    public bool IsHit(Transform parent)
        => Results.Any(x => x.gameObject.transform == parent || x.gameObject.transform.IsChildOf(parent));
}
