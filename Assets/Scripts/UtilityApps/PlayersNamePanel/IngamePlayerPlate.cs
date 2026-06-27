using UnityEngine;
using UnityEngine.EventSystems;

public class IngamePlayerPlate : MonoBehaviour, IPointerClickHandler
{
    private IngameTeamPlate teamPlate;
    private UIEffector_Greyscale greyscaleEffector;

    private bool dimmed;
    public bool Dimmed => dimmed;

    /// <summary>
    /// Caches all child Graphics, their original colors and materials, and prepares the greyscale material.
    /// </summary>
    private void Awake()
    {
        teamPlate = GetComponentInParent<IngameTeamPlate>();
        greyscaleEffector = GetComponent<UIEffector_Greyscale>();
    }

    /// <summary>
    /// Toggles dark-greyscale dim on left click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        dimmed = !dimmed;

        greyscaleEffector.enabled = !greyscaleEffector.enabled;

        if (teamPlate != null)
            teamPlate.EvaluateDimState();
    }
}
