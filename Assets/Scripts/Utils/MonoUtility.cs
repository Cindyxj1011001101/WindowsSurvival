using System.Linq;
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
        // 解除父子关系并批量销毁
        Transform[] children = new Transform[parent.transform.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = parent.transform.GetChild(i);
        }
        parent.transform.DetachChildren(); // 解除所有父子关系

        foreach (Transform child in children)
        {
            Object.Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 更新容器高度
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="elementCount"></param>
    public static void UpdateLayoutSize(ILayoutGroup layout)
    {
        switch (layout)
        {
            case GridLayoutGroup gridLayout:
                UpdateGridLayoutSize(gridLayout);
                break;
            case VerticalLayoutGroup verticalLayout:
                UpdateVerticalLayoutSize(verticalLayout);
                break;
            case HorizontalLayoutGroup horizontalLayout:
                UpdateHorizontalLayoutSize(horizontalLayout);
                break;
            case ChatLayoutGroup chatLayout:
                UpdateChatLayoutSize(chatLayout);
                break;
            default:
                break;
        }
    }

    public static void UpdateChatLayoutSize(ChatLayoutGroup layout)
    {
        RectTransform layoutTransform = layout.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);

        float containerHeight = layout.padding.top + layout.padding.bottom;

        int activateChildCount = 0;
        for (int i = 0; i < layout.transform.childCount; i++)
        {
            if (!layout.transform.GetChild(i).gameObject.activeSelf) continue; // 只计算激活的子物体
            containerHeight += layout.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
            activateChildCount++;
        }

        containerHeight += Mathf.Max(0, activateChildCount - 1) * layout.spacing;

        layoutTransform.sizeDelta = new Vector2(layoutTransform.sizeDelta.x, containerHeight);

        // 立刻更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }

    public static void UpdateGridLayoutSize(GridLayoutGroup layout)
    {
        RectTransform layoutTransform = layout.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);

        int activateChildCount = layout.transform.Cast<Transform>().Count(child => child.gameObject.activeSelf); // 只计算激活的子物体

        float containerWidth = layoutTransform.rect.width;
        // 计算一行可以放几个格子
        int i = 1;
        while (layout.cellSize.x * i + layout.spacing.x * (i - 1) + layout.padding.left + layout.padding.right <= containerWidth)
        {
            i++;
        }
        int columns = Mathf.Max(1, i - 1);
        int totalRows = Mathf.CeilToInt((float)activateChildCount / columns);

        // 计算容器高度
        float containerHeight = totalRows * layout.cellSize.y + (totalRows - 1) * layout.spacing.y + layout.padding.top + layout.padding.bottom;

        layoutTransform.sizeDelta = new Vector2(layoutTransform.sizeDelta.x, containerHeight);

        // 立刻更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }


    public static void UpdateVerticalLayoutSize(VerticalLayoutGroup layout)
    {
        RectTransform layoutTransform = layout.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);

        float containerHeight = layout.padding.top + layout.padding.bottom;

        int activateChildCount = 0;
        for (int i = 0; i < layout.transform.childCount; i++)
        {
            if (!layout.transform.GetChild(i).gameObject.activeSelf) continue; // 只计算激活的子物体
            containerHeight += layout.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
            activateChildCount++;
        }

        containerHeight += Mathf.Max(0, activateChildCount - 1) * layout.spacing;

        layoutTransform.sizeDelta = new Vector2(layoutTransform.sizeDelta.x, containerHeight);

        // 立刻更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }

    public static void UpdateHorizontalLayoutSize(HorizontalLayoutGroup layout)
    {
        RectTransform layoutTransform = layout.transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);

        float containerWidth = layout.padding.left + layout.padding.right;

        int activateChildCount = 0;
        for (int i = 0; i < layout.transform.childCount; i++)
        {
            if (!layout.transform.GetChild(i).gameObject.activeSelf) continue; // 只计算激活的子物体
            containerWidth += layout.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.x;
            activateChildCount++;
        }

        containerWidth += Mathf.Max(0, activateChildCount - 1) * layout.spacing;

        layoutTransform.sizeDelta = new Vector2(containerWidth, layoutTransform.sizeDelta.y);
        // 立刻更新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
    }
}