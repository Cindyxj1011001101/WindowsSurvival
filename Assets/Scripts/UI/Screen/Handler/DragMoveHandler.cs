using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.Events;

public class DragMoveHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WindowBase thisWindow;
    private RectTransform thisWindowRect;

    private Vector2 offset;

    public UnityEvent onPointerDown = new();

    private float snapThreshold = 10f; // 吸附阈值（像素）

    private void Awake()
    {
        thisWindow = GetComponentInParent<WindowBase>();
        thisWindowRect = thisWindow.transform as RectTransform;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (thisWindowRect == null) return;

        MouseManager.Instance.ChangeMouseState(MouseState.Drag);

        onPointerDown?.Invoke();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisWindowRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        offset = thisWindowRect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (thisWindowRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            thisWindowRect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        // 计算目标的新位置
        Vector2 newPosition = localPoint + offset;

        // 获取 Canvas 的 RectTransform
        RectTransform canvasRect = thisWindowRect.root.transform as RectTransform;

        // 将目标对象的大小转换为世界坐标系中的大小
        Rect targetRect = thisWindowRect.rect;
        Vector2 targetSize = new Vector2(targetRect.width, targetRect.height);

        // 计算目标对象的半宽和半高
        float halfWidth = targetSize.x * thisWindowRect.pivot.x;
        float halfHeight = targetSize.y * thisWindowRect.pivot.y;

        // 获取 Canvas 的大小
        Vector2 canvasSize = canvasRect.rect.size;

        // 限制 newPosition 在 Canvas 范围内
        newPosition.x = Mathf.Clamp(newPosition.x, -canvasSize.x / 2, canvasSize.x / 2);
        // 70 = 屏幕可视范围距离顶端的距离，60 = 顶边栏的高度，74 = 屏幕可视范围距离底端的距离，2 = 微调
        newPosition.y = Mathf.Clamp(newPosition.y, -canvasSize.y / 2 - halfHeight + 70 + 60 + 2, canvasSize.y / 2 - halfHeight - 74 + 2);

        // 设置新的锚点位置
        thisWindowRect.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MouseManager.Instance.ChangeMouseState(MouseState.Default);

        // 进行窗口边缘吸附
        SnapToEdges();
    }

    private void SnapToEdges()
    {
        // 优先检查与其他窗口的吸附
        //bool snappedToWindow = CheckWindowSnapping();

        // 如果没有吸附到其他窗口，则检查屏幕边缘吸附
        //if (!snappedToWindow)
        {
            CheckScreenEdgeSnapping();
        }
    }

    private bool CheckWindowSnapping()
    {
        Rect thisScreenRect = GetScreenRect(thisWindowRect);

        float minDistance = float.MaxValue;
        WindowBase closestWindow = null;
        SnapDirection snapDirection = SnapDirection.None;

        // 遍历所有窗口（排除自己）
        foreach (var window in WindowsManager.Instance.GetOpenedWindows(true).Values)
        {
            if (window == thisWindow || window == null) continue;

            RectTransform otherRect = window.transform as RectTransform;
            Rect otherScreenRect = GetScreenRect(otherRect);

            // 检查四个方向的吸附可能性
            CheckSnapDirection(thisScreenRect, otherScreenRect,
                              ref minDistance, ref closestWindow, ref snapDirection);
        }

        // 如果找到最近的窗口且距离小于阈值，则执行吸附
        if (closestWindow != null && minDistance < snapThreshold)
        {
            ExecuteWindowSnap(thisWindowRect, closestWindow.transform as RectTransform, snapDirection);
            return true;
        }

        return false;
    }

    private void CheckSnapDirection(Rect thisRect, Rect otherRect,
                                   ref float minDistance,
                                   ref WindowBase closestWindow,
                                   ref SnapDirection snapDirection)
    {
        // 计算四个方向的距离
        float leftDistance = Mathf.Abs(thisRect.xMax - otherRect.xMin);
        float rightDistance = Mathf.Abs(thisRect.xMin - otherRect.xMax);
        float topDistance = Mathf.Abs(thisRect.yMin - otherRect.yMax);
        float bottomDistance = Mathf.Abs(thisRect.yMax - otherRect.yMin);

        // 检查左吸附
        if (leftDistance < minDistance && leftDistance < snapThreshold)
        {
            minDistance = leftDistance;
            closestWindow = thisWindow;
            snapDirection = SnapDirection.Left;
        }

        // 检查右吸附
        if (rightDistance < minDistance && rightDistance < snapThreshold)
        {
            minDistance = rightDistance;
            closestWindow = thisWindow;
            snapDirection = SnapDirection.Right;
        }

        // 检查上吸附
        if (topDistance < minDistance && topDistance < snapThreshold)
        {
            minDistance = topDistance;
            closestWindow = thisWindow;
            snapDirection = SnapDirection.Top;
        }

        // 检查下吸附
        if (bottomDistance < minDistance && bottomDistance < snapThreshold)
        {
            minDistance = bottomDistance;
            closestWindow = thisWindow;
            snapDirection = SnapDirection.Bottom;
        }
    }

    private void ExecuteWindowSnap(RectTransform source, RectTransform target, SnapDirection direction)
    {
        Vector2 targetPosition = source.localPosition;
        Rect sourceRect = GetScreenRect(source);
        Rect targetRect = GetScreenRect(target);

        switch (direction)
        {
            case SnapDirection.Left:
                targetPosition.x = target.localPosition.x -
                                  (sourceRect.width / 2 + targetRect.width / 2);
                break;
            case SnapDirection.Right:
                targetPosition.x = target.localPosition.x +
                                  (sourceRect.width / 2 + targetRect.width / 2);
                break;
            case SnapDirection.Top:
                targetPosition.y = target.localPosition.y +
                                  (sourceRect.height / 2 + targetRect.height / 2);
                break;
            case SnapDirection.Bottom:
                targetPosition.y = target.localPosition.y -
                                  (sourceRect.height / 2 + targetRect.height / 2);
                break;
        }

        thisWindowRect.localPosition = targetPosition;
    }

    private void CheckScreenEdgeSnapping()
    {
        // 获取画布尺寸
        RectTransform screenRect = WindowsManager.Instance.Desktop;

        var centerOffset = screenRect.position;

        var (thisLeft, thisTop, thisRight, thisBottom) = GetFourBorders(thisWindowRect);
        var (screenLeft, screenTop, screenRight, screenBottom) = GetFourBorders(screenRect);

        Vector2 targetPosition = thisWindowRect.localPosition;
        bool snapped = false;

        // 检查左边缘
        if (Mathf.Abs(thisLeft - screenLeft) < snapThreshold)
        {
            targetPosition.x = screenLeft + thisWindowRect.rect.width / 2 - centerOffset.x;
            snapped = true;
        }
        // 检查右边缘
        else if (Mathf.Abs(thisRight - screenRight) < snapThreshold)
        {
            targetPosition.x = screenRight - thisWindowRect.rect.width / 2 - centerOffset.x;
            snapped = true;
        }

        // 检查上边缘
        if (Mathf.Abs(thisTop - screenTop) < snapThreshold)
        {
            targetPosition.y = screenTop - thisWindowRect.rect.height / 2 - centerOffset.y;
            snapped = true;
        }
        // 检查下边缘
        else if (Mathf.Abs(thisBottom - screenBottom) < snapThreshold)
        {
            targetPosition.y = screenBottom + thisWindowRect.rect.height / 2 - centerOffset.y;
            snapped = true;
        }

        if (snapped)
        {
            thisWindowRect.localPosition = targetPosition;
        }
    }

    private Rect GetScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        var canvas = FindObjectOfType<Canvas>();

        // 转换到屏幕坐标
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return new Rect(corners[0].x, corners[0].y,
                           corners[2].x - corners[0].x,
                           corners[2].y - corners[0].y);
        }
        else
        {
            // 对于摄像机渲染模式，需要转换坐标
            Vector2 min = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[2]);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }

    private (float, float, float, float) GetFourBorders(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        return (corners[1].x, corners[1].y, corners[3].x, corners[3].y);
    }

    private enum SnapDirection
    {
        None,
        Left,
        Right,
        Top,
        Bottom
    }
}
