using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ZoomHandler : MonoBehaviour, IScrollHandler
{
    [Header("Zoom Settings")]
    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 2f;
    public bool zoomToMousePosition = true;

    private ScrollRect scrollRect;
    private RectTransform content;
    private Vector3 initialScale;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        initialScale = content.localScale;
    }

    public void OnScroll(PointerEventData eventData)
    {
        // 获取滚轮输入
        float scrollDelta = eventData.scrollDelta.y;

        // 如果没有滚动输入，直接返回
        if (scrollDelta == 0) return;

        // 保存缩放前的鼠标位置（用于缩放中心点）
        Vector2 mousePositionBeforeZoom = eventData.position;

        // 计算新的缩放比例
        Vector3 newScale = content.localScale + scrollDelta * zoomSpeed * Vector3.one;
        newScale = ClampScale(newScale);

        // 应用缩放
        ApplyZoom(newScale, mousePositionBeforeZoom);
    }

    private Vector3 ClampScale(Vector3 scale)
    {
        scale.x = Mathf.Clamp(scale.x, minZoom, maxZoom);
        scale.y = Mathf.Clamp(scale.y, minZoom, maxZoom);
        scale.z = Mathf.Clamp(scale.z, minZoom, maxZoom);
        return scale;
    }

    private void ApplyZoom(Vector3 newScale, Vector2 screenPosition)
    {
        if (!zoomToMousePosition)
        {
            // 简单缩放，不保持鼠标位置
            content.localScale = newScale;
            return;
        }

        // 获取缩放前的局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPosition,
            null,
            out Vector2 localPointBeforeZoom);

        // 保存旧的缩放比例
        //Vector3 oldScale = content.localScale;

        // 应用新缩放比例
        content.localScale = newScale;

        // 获取缩放后的局部坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            content,
            screenPosition,
            null,
            out Vector2 localPointAfterZoom);

        // 计算偏移量以保持鼠标位置不变
        Vector2 offset = localPointAfterZoom - localPointBeforeZoom;

        // 调整内容位置
        content.anchoredPosition -= offset * newScale.x;
    }

    public void ResetZoom()
    {
        content.localScale = initialScale;
    }
}