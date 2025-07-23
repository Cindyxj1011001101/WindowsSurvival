using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class CustomVerticalLayout : MonoBehaviour
{
    //上方间距
    public float topSpacing;
    //下方间距
    public float bottomSpacing;
    //普通间距
    public float spacing;

    private RectTransform rectTransform;
    private List<RectTransform> children = new List<RectTransform>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        RefreshAllChildren();
    }


    public void RefreshAllChildren()
    {
        children.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child == null) continue;
            if (child.gameObject.activeSelf == false) continue;
            children.Add(child);
        }
        foreach (var child in children)
        {
            child.GetComponentInChildren<CustomTextBox>().RefreshSizeIfNeeded();
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        }
        float currentY = topSpacing;
        foreach (var child in children)
        {
            child.anchoredPosition = new Vector2(child.anchoredPosition.x, -currentY);
            currentY += child.sizeDelta.y + spacing;
        }
        float totalHeight = currentY + bottomSpacing;
        totalHeight = Mathf.Max(0, totalHeight);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
