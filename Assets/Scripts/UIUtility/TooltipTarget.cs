using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public string Content;

    private Graphic graphicComponent;

    private void Awake()
    {
        graphicComponent = GetComponent<Graphic>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.Trigger(this);

        OnPointerMove(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.Release();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        TooltipManager.Instance.PositionUpdate(eventData);
    }
}
