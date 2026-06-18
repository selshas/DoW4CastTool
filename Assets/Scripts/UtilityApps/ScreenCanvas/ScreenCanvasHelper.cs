using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCanvasHelper : Helper
{
    public ScreenCanvas ScreenCanvas;

    public GameObject PenColorTemplete;

    private void Awake()
    {
        var colors = ScreenCanvas.PenColors;
        var colorNames = colors.Keys.ToArray();

        var parentTransform = PenColorTemplete.transform.parent;
        for (var i = 0; i < colorNames.Length; i++)
        {
            var colorName = colorNames[i];
            var color = colors[colorName];

            var intance = Instantiate(PenColorTemplete, parentTransform);
            intance.name = $"{colorName}({i + 1})";
            intance.GetComponent<RawImage>().color = color;
            intance.GetComponentInChildren<TextMeshProUGUI>().text = $"{i + 1}";
        }

        DestroyImmediate(PenColorTemplete);
    }
}
