using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Flexible Grid Layout Group")]
public partial class FlexibleGridLayoutGroup : LayoutGroup
{
    public enum OrderPriority { Row, Column }

    [SerializeField] private OrderPriority m_OrderPriority = OrderPriority.Row;

    [SerializeField] private int m_GridSize;

    [SerializeField] private float m_RowSpacing;
    [SerializeField] private float m_ColumnSpacing;

    [SerializeField] private bool m_ReverseArrangement;

    [SerializeField] private bool m_ChildControlWidth = true;
    [SerializeField] private bool m_ChildControlHeight = true;

    [SerializeField] private bool m_ChildForceExpandWidth = true;
    [SerializeField] private bool m_ChildForceExpandHeight = true;

    [SerializeField] private bool m_ChildScaleWidth;
    [SerializeField] private bool m_ChildScaleHeight;

    public OrderPriority LayoutPriority { get => m_OrderPriority; set => SetProperty(ref m_OrderPriority, value); }

    public int GridSize { get => m_GridSize; set => SetProperty(ref m_GridSize, Mathf.Max(0, value)); }

    public float RowSpacing { get => m_RowSpacing; set => SetProperty(ref m_RowSpacing, value); }
    public float ColumnSpacing { get => m_ColumnSpacing; set => SetProperty(ref m_ColumnSpacing, value); }

    public bool ReverseArrangement { get => m_ReverseArrangement; set => SetProperty(ref m_ReverseArrangement, value); }

    public bool ChildControlWidth { get => m_ChildControlWidth; set => SetProperty(ref m_ChildControlWidth, value); }
    public bool ChildControlHeight { get => m_ChildControlHeight; set => SetProperty(ref m_ChildControlHeight, value); }

    public bool ChildForceExpandWidth { get => m_ChildForceExpandWidth; set => SetProperty(ref m_ChildForceExpandWidth, value); }
    public bool ChildForceExpandHeight { get => m_ChildForceExpandHeight; set => SetProperty(ref m_ChildForceExpandHeight, value); }

    public bool ChildScaleWidth { get => m_ChildScaleWidth; set => SetProperty(ref m_ChildScaleWidth, value); }
    public bool ChildScaleHeight { get => m_ChildScaleHeight; set => SetProperty(ref m_ChildScaleHeight, value); }

    private struct GroupData
    {
        public int StartIndex;
        public int LastIndex;
        public float CrossAxialSize;
    }

    private readonly List<GroupData> groups = new List<GroupData>();

    private bool IsRowPrior => (m_OrderPriority == OrderPriority.Row);
    private int MainAxis => (IsRowPrior) ? 0 : 1;
    private int CrossAxis => (IsRowPrior) ? 1 : 0;
    private float MainAxisSpacing => (IsRowPrior) ? m_ColumnSpacing : m_RowSpacing;
    private float CrossAxisSpacing => (IsRowPrior) ? m_RowSpacing : m_ColumnSpacing;


    #region Layout Calculation

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CalculateGroups();

        var min = default(float);
        var preferred = default(float);

        if (IsRowPrior)
            CalcMainAxisInput(0, out min, out preferred);
        else
            CalcCrossAxisInput(0, out min, out preferred);

        SetLayoutInputForAxis(min + padding.horizontal, preferred + padding.horizontal, -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        var min = default(float);
        var preferred = default(float);

        if (IsRowPrior)
            CalcCrossAxisInput(1, out min, out preferred);
        else
            CalcMainAxisInput(1, out min, out preferred);

        SetLayoutInputForAxis(min + padding.vertical, preferred + padding.vertical, -1, 1);
    }

    private void CalcMainAxisInput(int axis, out float min, out float preferred)
    {
        min = 0;
        preferred = 0;

        for (var i = 0; i < rectChildren.Count; i++)
        {
            var childSize = GetChildSize(rectChildren[i], axis);

            min = Mathf.Max(min, childSize);

            if (i > 0)
                preferred += MainAxisSpacing;

            preferred += childSize;
        }
    }

    private void CalcCrossAxisInput(int axis, out float min, out float preferred)
    {
        min = 0;
        preferred = 0;

        for (var g = 0; g < groups.Count; g++)
        {
            var crossAxialSize = groups[g].CrossAxialSize;

            min = Mathf.Max(min, crossAxialSize);

            if (g > 0)
                preferred += CrossAxisSpacing;

            preferred += crossAxialSize;
        }
    }

    #endregion

    #region Layout Apply

    /// <summary>
    /// Arrange items along horizontal axis
    /// </summary>
    public override void SetLayoutHorizontal()
    {
        if (MainAxis == 0)
            SetAlongMainAxis(0);
        else
            SetAlongCrossAxis(0);
    }

    /// <summary>
    /// Arrange items along vertical axis
    /// </summary>
    public override void SetLayoutVertical()
    {
        if (CrossAxis == 1)
            SetAlongCrossAxis(1);
        else
            SetAlongMainAxis(1);
    }

    private void SetAlongMainAxis(int axis)
    {
        var isControlled = (axis == 0) ? m_ChildControlWidth : m_ChildControlHeight;
        var isForceExpanded = (axis == 0) ? m_ChildForceExpandWidth : m_ChildForceExpandHeight;

        var spacing = MainAxisSpacing;
        var availableSpace = (rectTransform.rect.size[axis] - GetPaddingTotal(axis));
        var paddingStart = GetPaddingStart(axis);

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];
            var itemCount = (group.LastIndex - group.StartIndex);

            var totalPreferredSize = 0f;
            var totalFlexibleSize = 0f;
            for (var i = group.StartIndex; i < group.LastIndex; i++)
            {
                var child = rectChildren[OrderedIndex(i)];
                totalPreferredSize += GetChildSize(child, axis);

                if (isControlled)
                    totalFlexibleSize += LayoutUtility.GetFlexibleSize(child, axis);
            }

            var totalSpacing = spacing * (itemCount - 1);
            var remainingSpace = (availableSpace - (totalPreferredSize + totalSpacing));
            var shouldDistribute = (isControlled) && (remainingSpace > 0.001f) && ((isForceExpanded) || (totalFlexibleSize > 0));

            var position = (shouldDistribute)
                ? paddingStart
                : paddingStart + Mathf.Max(0, remainingSpace) * AlignmentOnAxis(axis);

            for (var i = group.StartIndex; i < group.LastIndex; i++)
            {
                var child = rectChildren[OrderedIndex(i)];
                var childSize = GetChildSize(child, axis);

                if (shouldDistribute)
                {
                    var flexibleSize = LayoutUtility.GetFlexibleSize(child, axis);
                    var distributionRatio = (totalFlexibleSize > 0)
                        ? flexibleSize / totalFlexibleSize
                        : 1f / itemCount;

                    childSize += remainingSpace * distributionRatio;
                }

                if (isControlled)
                    SetChildAlongAxis(child, axis, position, childSize);
                else
                    SetChildAlongAxis(child, axis, position);

                position += childSize + spacing;
            }
        }
    }

    private void SetAlongCrossAxis(int axis)
    {
        var isControlled = (axis == 0) ? m_ChildControlWidth : m_ChildControlHeight;
        var isForceExpanded = (axis == 0) ? m_ChildForceExpandWidth : m_ChildForceExpandHeight;

        var spacing = CrossAxisSpacing;
        var paddingStart = GetPaddingStart(axis);

        var totalCrossAxialSize = 0f;
        for (var g = 0; g < groups.Count; g++)
        {
            if (g > 0)
                totalCrossAxialSize += spacing;

            totalCrossAxialSize += groups[g].CrossAxialSize;
        }

        var availableSpace = (rectTransform.rect.size[axis] - GetPaddingTotal(axis));
        var remainingSpace = (availableSpace - totalCrossAxialSize);
        // Epsilon guards against float drift from SetChildAlongAxis write-read round-trip.
        var shouldDistribute = (isControlled) && (isForceExpanded) && (remainingSpace > 0.001f);

        var position = (shouldDistribute)
            ? paddingStart
            : paddingStart + Mathf.Max(0, remainingSpace) * AlignmentOnAxis(axis);

        for (var g = 0; g < groups.Count; g++)
        {
            var groupCrossAxialSize = groups[g].CrossAxialSize;

            if (shouldDistribute)
                groupCrossAxialSize += remainingSpace / groups.Count;

            for (var i = groups[g].StartIndex; i < groups[g].LastIndex; i++)
            {
                var child = rectChildren[OrderedIndex(i)];

                if (isControlled)
                {
                    var childSize = (isForceExpanded)
                        ? groupCrossAxialSize
                        : Mathf.Min(GetChildSize(child, axis), groupCrossAxialSize);

                    var alignmentOffset = (groupCrossAxialSize - childSize) * AlignmentOnAxis(axis);
                    SetChildAlongAxis(child, axis, position + alignmentOffset, childSize);
                }
                else
                {
                    SetChildAlongAxis(child, axis, position);
                }
            }

            position += groupCrossAxialSize + spacing;
        }
    }

    #endregion

    #region Grouping

    private void CalculateGroups()
    {
        groups.Clear();

        if (rectChildren.Count == 0)
            return;

        var mainAxis = MainAxis;
        var crossAxis = CrossAxis;

        var spacing = MainAxisSpacing;
        var availableSpace = (rectTransform.rect.size[mainAxis] - GetPaddingTotal(mainAxis));

        var groupStartIndex = 0;

        var filled = 0f;
        var maxCrossAxialSize = 0f;

        for (var i = 0; i < rectChildren.Count; i++)
        {
            var child = rectChildren[OrderedIndex(i)];

            var childSizeInMainAxis = GetChildSize(child, mainAxis);
            var childSizeInCrossAxis = GetChildSize(child, crossAxis);

            var isFirstItemInGroup = (i == groupStartIndex);
            var requiredSpace = (isFirstItemInGroup)
                ? childSizeInMainAxis
                : (filled + spacing + childSizeInMainAxis);

            requiredSpace -= availableSpace;

            // Epsilon guards against float drift from SetChildAlongAxis write-read round-trip.
            var overflowed = (!isFirstItemInGroup) && (requiredSpace > 0.001f);
            var groupFilled = (m_GridSize > 0) && (!isFirstItemInGroup) && ((i - groupStartIndex) >= m_GridSize);

            if (overflowed || groupFilled)
            {
                groups.Add(new GroupData
                {
                    StartIndex = groupStartIndex,
                    LastIndex = i,
                    CrossAxialSize = maxCrossAxialSize,
                });

                groupStartIndex = i;
                filled = childSizeInMainAxis;
                maxCrossAxialSize = childSizeInCrossAxis;
            }
            else
            {
                filled = requiredSpace;
                maxCrossAxialSize = Mathf.Max(maxCrossAxialSize, childSizeInCrossAxis);
            }
        }

        // The Last group
        groups.Add(new GroupData
        {
            StartIndex = groupStartIndex,
            LastIndex = rectChildren.Count,
            CrossAxialSize = maxCrossAxialSize,
        });
    }

    #endregion

    #region Helpers

    private int OrderedIndex(int i)
    {
        if (m_ReverseArrangement)
            return ((rectChildren.Count - 1) - i);

        return i;
    }

    private float AlignmentOnAxis(int axis)
    {
        if (axis == 0)
            return ((int)childAlignment % 3) * 0.5f;

        return ((int)childAlignment / 3) * 0.5f;
    }

    private float GetPaddingStart(int axis)
    {
        if (axis == 0)
            return padding.left;

        return padding.top;
    }

    private float GetPaddingTotal(int axis)
    {
        if (axis == 0)
            return padding.horizontal;

        return padding.vertical;
    }

    private float GetChildSize(RectTransform child, int axis)
    {
        var isControlled = (axis == 0) ? m_ChildControlWidth : m_ChildControlHeight;
        var isScaled = (axis == 0) ? m_ChildScaleWidth : m_ChildScaleHeight;

        var size = (isControlled)
            ? LayoutUtility.GetPreferredSize(child, axis)
            : child.sizeDelta[axis];

        if (isScaled)
            size *= child.localScale[axis];

        return size;
    }

    #endregion

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
