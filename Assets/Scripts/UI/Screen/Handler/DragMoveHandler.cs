using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragMoveHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WindowBase thisWindow;
    private RectTransform thisWindowRect;

    private Vector2 offset;

    public UnityEvent onPointerDown = new();

    private float snapThreshold = 10f; // 吸附阈值（像素）

    private float constBorder = 2f;

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

        offset = new Vector2(thisWindowRect.position.x, thisWindowRect.position.y) - localPoint;
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
        Vector3 newPosition = localPoint + offset;

        ClampWindowPosition(ref newPosition);

        // 设置新的锚点位置
        thisWindowRect.position = newPosition;
    }

    private void ClampWindowPosition(ref Vector3 newPosition)
    {
        var (screenLeft, screenTop, screenRight, screenBottom) = GetFourBorders(WindowsManager.Instance.Desktop);

        float halfHeight = thisWindowRect.rect.height / 2;

        // 限制newPosition在桌面范围内
        newPosition.x = Mathf.Clamp(newPosition.x, screenLeft, screenRight);

        var barHeight = (transform as RectTransform).rect.height + 3 * constBorder;

        newPosition.y = Mathf.Clamp(newPosition.y, screenBottom - (halfHeight - barHeight), screenTop - halfHeight);
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
        bool snappedToWindow = CheckWindowSnapping();

        // 如果没有吸附到其他窗口，则检查屏幕边缘吸附
        if (!snappedToWindow)
        {
            CheckScreenEdgeSnapping();
        }

        // 限制窗口不要超出屏幕范围
        var targetPosition = thisWindowRect.position;
        ClampWindowPosition(ref targetPosition);
        thisWindowRect.position = targetPosition;
    }

    /// <summary>
    /// 检查是否存在窗口边缘吸附的可能
    /// </summary>
    /// <returns></returns>
    private bool CheckWindowSnapping()
    {
        float minDistanceHorizontal = float.MaxValue;
        float minDistanceVertical = float.MaxValue;

        RectTransform closestWindowRectHorizontal = null;
        RectTransform closestWindowRectVertical = null;

        SnapDirection snapDirectionHorizontal = SnapDirection.None;
        SnapDirection snapDirectionVertical = SnapDirection.None;

        var (thisLeft, thisTop, thisRight, thisBottom) = GetFourBorders(thisWindowRect);

        // 遍历所有窗口（排除自己）
        foreach (var window in WindowsManager.Instance.GetOpenedWindows(true).Values)
        {
            if (window == thisWindow || window == null) continue;

            // 检查四个方向的吸附可能性
            CheckSnapDirection(thisLeft, thisTop, thisRight, thisBottom,
                window.transform as RectTransform,
                ref minDistanceHorizontal,
                ref minDistanceVertical,
                ref closestWindowRectHorizontal,
                ref closestWindowRectVertical,
                ref snapDirectionHorizontal,
                ref snapDirectionVertical);
        }

        bool snapped = false;

        // 如果找到最近的窗口且距离小于阈值，则执行吸附
        // 水平方向
        if (closestWindowRectHorizontal != null && minDistanceHorizontal < snapThreshold)
        {
            ExecuteWindowSnap(closestWindowRectHorizontal, snapDirectionHorizontal);
            snapped = true;
        }
        // 垂直方向
        if (closestWindowRectVertical != null && minDistanceVertical < snapThreshold)
        {
            ExecuteWindowSnap(closestWindowRectVertical, snapDirectionVertical);
            snapped = true;
        }

        return snapped;
    }


    /// <summary>
    /// 检查四个方向的吸附可能性
    /// </summary>
    private void CheckSnapDirection(float thisLeft, float thisTop, float thisRight, float thisBottom,
                                   RectTransform otherWindowRect,
                                   ref float minDistanceHorizontal,
                                   ref float minDistanceVertical,
                                   ref RectTransform closestWindowRectHorizontal,
                                   ref RectTransform closestWindowRectVertical,
                                   ref SnapDirection snapDirectionHorizontal,
                                   ref SnapDirection snapDirectionVertical)
    {
        // 计算四个方向的距离
        var (otherLeft, otherTop, otherRight, otherBottom) = GetFourBorders(otherWindowRect);
        var leftDistance = Mathf.Abs(otherRight - thisLeft);
        var rightDistance = Mathf.Abs(otherLeft - thisRight);
        var topDistance = Mathf.Abs(otherBottom - thisTop);
        var bottomDistance = Mathf.Abs(otherTop - thisBottom);

        // 检查左吸附
        if (leftDistance < minDistanceHorizontal && leftDistance < snapThreshold)
        {
            minDistanceHorizontal = leftDistance;
            closestWindowRectHorizontal = otherWindowRect;
            snapDirectionHorizontal = SnapDirection.Left;
        }

        // 检查右吸附
        if (rightDistance < minDistanceHorizontal && rightDistance < snapThreshold)
        {
            minDistanceHorizontal = rightDistance;
            closestWindowRectHorizontal = otherWindowRect;
            snapDirectionHorizontal = SnapDirection.Right;
        }

        // 检查上吸附
        if (topDistance < minDistanceVertical && topDistance < snapThreshold)
        {
            minDistanceVertical = topDistance;
            closestWindowRectVertical = otherWindowRect;
            snapDirectionVertical = SnapDirection.Top;
        }

        // 检查下吸附
        if (bottomDistance < minDistanceVertical && bottomDistance < snapThreshold)
        {
            minDistanceVertical = bottomDistance;
            closestWindowRectVertical = otherWindowRect;
            snapDirectionVertical = SnapDirection.Bottom;
        }
    }

    /// <summary>
    /// 执行窗口边缘吸附
    /// </summary>
    /// <param name="target"></param>
    /// <param name="direction"></param>
    private void ExecuteWindowSnap(RectTransform target, SnapDirection direction)
    {
        Vector2 targetPosition = thisWindowRect.position;

        var (thisLeft, thisTop, thisRight, thisBottom) = GetFourBorders(thisWindowRect);
        var (otherLeft, otherTop, otherRight, otherBottom) = GetFourBorders(target);

        switch (direction)
        {
            case SnapDirection.Left:
                targetPosition.x -= thisLeft - otherRight + constBorder;
                break;
            case SnapDirection.Right:
                targetPosition.x -= thisRight - otherLeft - constBorder;
                break;
            case SnapDirection.Top:
                targetPosition.y -= thisTop - otherBottom - constBorder;
                break;
            case SnapDirection.Bottom:
                targetPosition.y -= thisBottom - otherTop + constBorder;
                break;
        }

        thisWindowRect.position = targetPosition;
    }

    /// <summary>
    /// 执行屏幕边缘吸附
    /// </summary>
    private void CheckScreenEdgeSnapping()
    {
        // 获取画布尺寸
        RectTransform screenRect = WindowsManager.Instance.Desktop;

        var (thisLeft, thisTop, thisRight, thisBottom) = GetFourBorders(thisWindowRect);
        var (screenLeft, screenTop, screenRight, screenBottom) = GetFourBorders(screenRect);

        Vector2 targetPosition = thisWindowRect.position;
        bool snapped = false;

        var halfWidth = thisWindowRect.rect.width / 2;
        var halfHeight = thisWindowRect.rect.height / 2;

        // 检查左边缘
        if (Mathf.Abs(thisLeft - screenLeft) < snapThreshold)
        {
            targetPosition.x = screenLeft + halfWidth;
            snapped = true;
        }
        // 检查右边缘
        else if (Mathf.Abs(thisRight - screenRight) < snapThreshold)
        {
            targetPosition.x = screenRight - halfWidth;
            snapped = true;
        }

        // 检查上边缘
        if (Mathf.Abs(thisTop - screenTop) < snapThreshold)
        {
            targetPosition.y = screenTop - halfHeight;
            snapped = true;
        }
        // 检查下边缘
        else if (Mathf.Abs(thisBottom - screenBottom) < snapThreshold)
        {
            targetPosition.y = screenBottom + halfHeight;
            snapped = true;
        }

        if (snapped)
        {
            thisWindowRect.position = targetPosition;
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
