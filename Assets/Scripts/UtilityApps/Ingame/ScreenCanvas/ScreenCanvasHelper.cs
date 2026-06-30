using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCanvasHelper : Helper
{
    public ScreenCanvas ScreenCanvas;

    public GameObject FoldableArea;

    public GameObject PenColorTemplete;

    private void Awake()
    {
        var colors = ScreenCanvas.PenColors;
        var colorNames = colors.Keys.ToArray();

        new UIItemList<string>(PenColorTemplete.transform.parent, colorNames, (child, colorName, i) =>
        {
            var color = colors[colorName];

            child.name = $"{colorName}({i + 1})";
            child.GetComponent<RawImage>().color = color;
            child.GetComponentInChildren<TextMeshProUGUI>().text = $"{i + 1}";
        });
    }

    public void SetFolding(bool unfolded)
    {
        FoldableArea.SetActive(unfolded);
    }
}
