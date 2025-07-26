using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragScaleHandler : MonoBehaviour,
    IPointerDownHandler, IDragHandler
{
    public enum ScaleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    public ScaleDirection direction;
    private RectTransform targetRect;
    private RectTransform canvasRect;
    private RectTransform rectMask;

    public float minWidth = 300f;
    public float minHeight = 200f;
    public float maxWidth = 800f;
    public float maxHeight = 420f;

    private Vector2 startMouseLocalToParent;
    private Vector2 startOffsetMin;
    private Vector2 startOffsetMax;

    private bool isDragging = false;

    public void Awake()
    {
        targetRect = transform.parent.parent.GetComponent<RectTransform>();
        canvasRect = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        rectMask = FindObjectOfType<RectMask2D>().GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetRect == null || canvasRect == null) return;

        isDragging = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out startMouseLocalToParent);

        startOffsetMin = targetRect.offsetMin;
        startOffsetMax = targetRect.offsetMax;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || targetRect == null || canvasRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 currentMouseLocalToParent);

        Vector2 delta = currentMouseLocalToParent - startMouseLocalToParent;

        Vector2 newOffsetMin = startOffsetMin;
        Vector2 newOffsetMax = startOffsetMax;

        switch (direction)
        {
            case ScaleDirection.Left:
                newOffsetMin.x = Mathf.Min(startOffsetMin.x + delta.x, startOffsetMax.x - minWidth);
                newOffsetMin.x = Mathf.Max(newOffsetMin.x, startOffsetMax.x - maxWidth);
                break;

            case ScaleDirection.Right:
                newOffsetMax.x = Mathf.Max(startOffsetMax.x + delta.x, startOffsetMin.x + minWidth);
                newOffsetMax.x = Mathf.Min(newOffsetMax.x, startOffsetMin.x + maxWidth);
                break;

            case ScaleDirection.Top:
                newOffsetMax.y = Mathf.Max(startOffsetMax.y + delta.y, startOffsetMin.y + minHeight);
                newOffsetMax.y = Mathf.Min(newOffsetMax.y, startOffsetMin.y + maxHeight);
                break;

            case ScaleDirection.Bottom:
                newOffsetMin.y = Mathf.Min(startOffsetMin.y + delta.y, startOffsetMax.y - minHeight);
                newOffsetMin.y = Mathf.Max(newOffsetMin.y, startOffsetMax.y - maxHeight);
                break;

            case ScaleDirection.TopLeft:
                newOffsetMin.x = Mathf.Min(startOffsetMin.x + delta.x, startOffsetMax.x - minWidth);
                newOffsetMin.x = Mathf.Max(newOffsetMin.x, startOffsetMax.x - maxWidth);

                newOffsetMax.y = Mathf.Max(startOffsetMax.y + delta.y, startOffsetMin.y + minHeight);
                newOffsetMax.y = Mathf.Min(newOffsetMax.y, startOffsetMin.y + maxHeight);
                break;

            case ScaleDirection.TopRight:
                newOffsetMax.x = Mathf.Max(startOffsetMax.x + delta.x, startOffsetMin.x + minWidth);
                newOffsetMax.x = Mathf.Min(newOffsetMax.x, startOffsetMin.x + maxWidth);

                newOffsetMax.y = Mathf.Max(startOffsetMax.y + delta.y, startOffsetMin.y + minHeight);
                newOffsetMax.y = Mathf.Min(newOffsetMax.y, startOffsetMin.y + maxHeight);
                break;

            case ScaleDirection.BottomLeft:
                newOffsetMin.x = Mathf.Min(startOffsetMin.x + delta.x, startOffsetMax.x - minWidth);
                newOffsetMin.x = Mathf.Max(newOffsetMin.x, startOffsetMax.x - maxWidth);

                newOffsetMin.y = Mathf.Min(startOffsetMin.y + delta.y, startOffsetMax.y - minHeight);
                newOffsetMin.y = Mathf.Max(newOffsetMin.y, startOffsetMax.y - maxHeight);
                break;

            case ScaleDirection.BottomRight:
                newOffsetMax.x = Mathf.Max(startOffsetMax.x + delta.x, startOffsetMin.x + minWidth);
                newOffsetMax.x = Mathf.Min(newOffsetMax.x, startOffsetMin.x + maxWidth);

                newOffsetMin.y = Mathf.Min(startOffsetMin.y + delta.y, startOffsetMax.y - minHeight);
                newOffsetMin.y = Mathf.Max(newOffsetMin.y, startOffsetMax.y - maxHeight);
                break;
        }

        targetRect.offsetMin = newOffsetMin;
        targetRect.offsetMax = newOffsetMax;

        targetRect.offsetMin = new Vector2(
            Mathf.Clamp(targetRect.offsetMin.x, rectMask.rect.xMin, rectMask.rect.xMax),
            Mathf.Clamp(targetRect.offsetMin.y, rectMask.rect.yMin, rectMask.rect.yMax)
        );
        targetRect.offsetMax = new Vector2(
            Mathf.Clamp(targetRect.offsetMax.x, rectMask.rect.xMin, rectMask.rect.xMax),
            Mathf.Clamp(targetRect.offsetMax.y, rectMask.rect.yMin, rectMask.rect.yMax)
        );
    }
}
