using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ChatLayoutGroup : MonoBehaviour, ILayoutGroup
{
    [Header("Spacing")]
    public float spacing = 0f;

    [Header("Margins")]
    public RectOffset padding;

    [Header("Child Control")]
    public bool controlChildWidth = false;
    public bool controlChildHeight = false;
    public float childWidth = 100f;
    public float childHeight = 100f;
    public bool ignoreInactive = true;

    private RectTransform rectTransform;
    private DrivenRectTransformTracker tracker;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        UpdateLayout();
    }

    private void OnDisable()
    {
        tracker.Clear();
    }

    private void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            UpdateLayout();
        }
    }

    public void UpdateLayout()
    {
        if (rectTransform == null)
            return;

        tracker.Clear();

        float yOffset = padding.top;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;

            if (child == null || (ignoreInactive && !child.gameObject.activeSelf))
                continue;

            // 保存原始锚点和轴心点
            var originalAnchorMin = child.anchorMin;
            var originalAnchorMax = child.anchorMax;
            var originalPivot = child.pivot;

            // 临时设置为顶部锚点以便计算位置
            child.anchorMin = new Vector2(0.5f, 1f); // 临时设置为顶部中心锚点
            child.anchorMax = new Vector2(0.5f, 1f);
            child.pivot = new Vector2(0.5f, 1f);

            // 计算位置
            Vector2 childSize = GetChildSize(child);

            // 水平对齐计算
            float xPosition = padding.left - padding.right;

            child.anchoredPosition = new Vector2(xPosition, -yOffset);

            // 恢复原始锚点和轴心点
            child.anchorMin = originalAnchorMin;
            child.anchorMax = originalAnchorMax;
            child.pivot = originalPivot;

            // 控制子物体大小
            if (controlChildWidth || controlChildHeight)
            {
                Vector2 sizeDelta = child.sizeDelta;
                if (controlChildWidth)
                {
                    sizeDelta.x = childWidth - (child.anchorMax.x - child.anchorMin.x) * rectTransform.rect.width;
                    tracker.Add(this, child, DrivenTransformProperties.SizeDeltaX);
                }
                if (controlChildHeight)
                {
                    sizeDelta.y = childHeight - (child.anchorMax.y - child.anchorMin.y) * rectTransform.rect.height;
                    tracker.Add(this, child, DrivenTransformProperties.SizeDeltaY);
                }
                child.sizeDelta = sizeDelta;
            }

            yOffset += childSize.y + spacing;
        }

        // 调整容器高度（可选）
        // rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, yOffset - spacing + marginBottom);
    }

    private Vector2 GetChildSize(RectTransform child)
    {
        if (controlChildWidth && controlChildHeight)
        {
            return new Vector2(childWidth, childHeight);
        }

        Vector2 size = child.rect.size;

        if (controlChildWidth)
        {
            size.x = Mathf.Min(childWidth, rectTransform.rect.width - padding.left - padding.right);
        }

        if (controlChildHeight)
        {
            size.y = childHeight;
        }

        return size;
    }

    public void SetLayoutHorizontal()
    {

    }

    public void SetLayoutVertical()
    {

    }

    private void OnTransformChildrenChanged()
    {
        UpdateLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateLayout();
    }
}