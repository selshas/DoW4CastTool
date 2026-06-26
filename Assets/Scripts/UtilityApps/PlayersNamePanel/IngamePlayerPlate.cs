using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IngamePlayerPlate : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float dimming = 0.4f;

    private bool dimmed;
    private Graphic[] graphics;
    private Color[] originalColors;
    private Material[] originalMaterials;
    private bool[] isTmpGraphic;

    private static Material greyscaleMaterial;

    /// <summary>
    /// Caches all child Graphics, their original colors and materials, and prepares the greyscale material.
    /// </summary>
    private void Awake()
    {
        if (greyscaleMaterial == null)
        {
            var shader = Shader.Find("CFLTool/shd_uiGreyscale");
            greyscaleMaterial = new Material(shader);
        }

        graphics = GetComponentsInChildren<Graphic>(true);
        originalColors = new Color[graphics.Length];
        originalMaterials = new Material[graphics.Length];
        isTmpGraphic = new bool[graphics.Length];

        for (var i = 0; i < graphics.Length; i++)
        {
            originalColors[i] = graphics[i].color;
            originalMaterials[i] = graphics[i].material;
            isTmpGraphic[i] = graphics[i] is TMP_Text;
        }
    }

    /// <summary>
    /// Toggles dark-greyscale dim on left click.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        dimmed = !dimmed;
        ApplyDimState();
    }

    /// <summary>
    /// Applies or removes the dark-greyscale effect on all child Graphics.
    /// </summary>
    private void ApplyDimState()
    {
        for (var i = 0; i < graphics.Length; i++)
        {
            if (dimmed)
            {
                if (isTmpGraphic[i])
                {
                    var orig = originalColors[i];
                    var lum = 0.299f * orig.r + 0.587f * orig.g + 0.114f * orig.b;
                    graphics[i].color = new Color(lum * dimming, lum * dimming, lum * dimming, orig.a);
                }
                else
                {
                    graphics[i].material = greyscaleMaterial;
                }
            }
            else
            {
                if (isTmpGraphic[i])
                    graphics[i].color = originalColors[i];
                else
                    graphics[i].material = originalMaterials[i];
            }
        }
    }
}
