using UnityEngine.EventSystems;
using UnityEngine;

public class DragMoveHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("移动目标")]
    public RectTransform targetToMove;

    [Header("下方收缩距离")] public float Distance;

    
    private Vector2 offset;
    

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetToMove == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = targetToMove.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    { if (targetToMove == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetToMove.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // 计算目标的新位置
        Vector2 newPosition = localPoint + offset;

        // 获取 Canvas 的 RectTransform
        RectTransform canvasRect = targetToMove.root.transform as RectTransform;

        // 将目标对象的大小转换为世界坐标系中的大小
        Rect targetRect = targetToMove.rect;
        Vector2 targetSize = new Vector2(targetRect.width, targetRect.height);

        // 计算目标对象的半宽和半高
        float halfWidth = targetSize.x * targetToMove.pivot.x;
        float halfHeight = targetSize.y * targetToMove.pivot.y;

        // 获取 Canvas 的大小
        Vector2 canvasSize = canvasRect.rect.size;
        
        

        // 限制 newPosition 在 Canvas 范围内
        newPosition.x = Mathf.Clamp(newPosition.x, -canvasSize.x / 2 + halfWidth, canvasSize.x / 2 - halfWidth);
        newPosition.y = Mathf.Clamp(newPosition.y, -canvasSize.y / 2 + halfHeight+Distance, canvasSize.y / 2 - halfHeight);

        // 设置新的锚点位置
        targetToMove.anchoredPosition = newPosition;
    }
}
