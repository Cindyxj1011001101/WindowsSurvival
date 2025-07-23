using UnityEngine;
using UnityEngine.UI;

public static class MonoUtility
{
    /// <summary>
    /// 销毁所有子物体
    /// </summary>
    /// <param name="parent"></param>
    public static void DestroyAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Object.Destroy(parent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 更新容器高度
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="elementCount"></param>
    public static void UpdateContainerHeight(GridLayoutGroup layout, int elementCount)
    {
        RectTransform layoutTransform = layout.transform as RectTransform;

        float containerWidth = layoutTransform.rect.width;
        // 计算一行可以放几个格子
        int i = 1;
        while (layout.cellSize.x * i + layout.spacing.x * (i - 1) + layout.padding.left + layout.padding.right <= containerWidth)
        {
            i++;
        }
        int columns = Mathf.Max(1, i - 1);
        int totalRows = Mathf.CeilToInt((float)elementCount / columns);

        // 计算容器高度
        float containerHeight = totalRows * layout.cellSize.y + (totalRows - 1) * layout.spacing.y + layout.padding.top + layout.padding.bottom;

        layoutTransform.sizeDelta = new Vector2(layoutTransform.sizeDelta.x, containerHeight);

        // 立刻更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }

    public static void UpdateContainerHeight(VerticalLayoutGroup layout)
    {
        int elementCount = layout.transform.childCount;
    }
}