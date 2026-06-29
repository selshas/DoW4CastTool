using UnityEngine;
using UnityEngine.EventSystems;

public class DragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PlayerPlate plate;

    /// <summary>
    /// Caches the parent PlayerPlate reference.
    /// </summary>
    private void Awake()
    {
        plate = GetComponentInParent<PlayerPlate>();
    }

    /// <summary>
    /// Forwards begin drag to the PlayerPlate.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        plate.OnHandleBeginDrag(eventData);
    }

    /// <summary>
    /// Forwards drag to the PlayerPlate.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        plate.OnHandleDrag(eventData);
    }

    /// <summary>
    /// Forwards end drag to the PlayerPlate.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        plate.OnHandleEndDrag(eventData);
    }
}
