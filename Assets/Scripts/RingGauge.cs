using UnityEngine;
using UnityEngine.UI;

public class RingGauge : MaskableGraphic
{
    [SerializeField] private float thickness = 20f;
    [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;
    [SerializeField] private Color backgroundColor = new Color(1, 1, 1, 0.2f);
    [SerializeField] private int segments = 64;

    public float Thickness
    {
        get => thickness;
        set
        {
            thickness = value;
            SetVerticesDirty();
        }
    }

    public float FillAmount
    {
        get => fillAmount;
        set
        {
            fillAmount = Mathf.Clamp01(value);
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// Draws a background ring at reduced alpha, then the fill ring on top.
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (thickness <= 0)
            return;

        AppendRing(vh, backgroundColor, 1f);

        if (fillAmount > 0)
            AppendRing(vh, color, fillAmount);
    }

    /// <summary>
    /// Appends a ring arc to the vertex buffer.
    /// </summary>
    private void AppendRing(VertexHelper vh, Color vertexColor, float fill)
    {
        var rect = rectTransform.rect;
        var outerRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
        var innerRadius = Mathf.Max(outerRadius - thickness, 0);
        var center = rect.center;

        var segmentCount = Mathf.Max(Mathf.CeilToInt(segments * fill), 1);
        var totalAngle = fill * Mathf.PI * 2f;
        var angleStep = totalAngle / segmentCount;
        var startAngle = Mathf.PI * 0.5f;
        var baseIndex = vh.currentVertCount;

        for (var i = 0; i <= segmentCount; i++)
        {
            var angle = startAngle - i * angleStep;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            var outerPos = center + new Vector2(cos * outerRadius, sin * outerRadius);
            var innerPos = center + new Vector2(cos * innerRadius, sin * innerRadius);

            vh.AddVert(outerPos, vertexColor, Vector2.zero);
            vh.AddVert(innerPos, vertexColor, Vector2.zero);

            if (i > 0)
            {
                var idx = baseIndex + i * 2;
                vh.AddTriangle(idx - 2, idx, idx - 1);
                vh.AddTriangle(idx - 1, idx, idx + 1);
            }
        }
    }

    /// <summary>
    /// Rebuilds the mesh when Inspector values change.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
}
