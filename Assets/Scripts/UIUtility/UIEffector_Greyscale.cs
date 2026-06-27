using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEffector_Greyscale : MonoBehaviour
{
    [SerializeField] private float dimming = 0.4f;

    private Graphic[] graphics;
    private Color[] originalColors;
    private Material[] originalMaterials;
    private bool[] isTmpGraphic;

    private static Material greyscaleMaterial;

    void Awake()
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

    private void OnEnable()
    {
        for (var i = 0; i < graphics.Length; i++)
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
    }

    private void OnDisable()
    {
        for (var i = 0; i < graphics.Length; i++)
        {
            if (isTmpGraphic[i])
                graphics[i].color = originalColors[i];
            else
                graphics[i].material = originalMaterials[i];
        }
    }

}
