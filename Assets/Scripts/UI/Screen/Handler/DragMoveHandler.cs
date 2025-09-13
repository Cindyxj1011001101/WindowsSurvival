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
        // 进行窗口吸附
        CheckWindowSnapping();

        // 限制窗口不要超出屏幕范围
        var targetPosition = thisWindowRect.position;
        ClampWindowPosition(ref targetPosition);
        thisWindowRect.position = targetPosition;
    }

    /// <summary>
    /// 检查是否存在窗口边缘吸附的可能
    /// </summary>
    /// <returns></returns>
    private void CheckWindowSnapping()
    {
        float minDistanceHorizontal = float.MaxValue;
        float minDistanceVertical = float.MaxValue;

        float horizontalBorder = 0;
        float verticalBorder = 0;

        SnapDirection snapDirectionHorizontal = SnapDirection.None;
        SnapDirection snapDirectionVertical = SnapDirection.None;

        var (thisLeft, thisTop, thisRight, thisBottom) = GetFourBorders(thisWindowRect);

        // 遍历所有窗口（排除自己）
        foreach (var window in WindowsManager.Instance.GetOpenedWindows(true).Values)
        {
            if (window == thisWindow || window == null) continue;

            // 检查四个方向的吸附可能性
            CheckWindowEdgeSnapping(thisLeft, thisTop, thisRight, thisBottom,
                window.transform as RectTransform,
                ref minDistanceHorizontal,
                ref minDistanceVertical,
                ref horizontalBorder,
                ref verticalBorder,
                ref snapDirectionHorizontal,
                ref snapDirectionVertical);
        }

        // 再检查和屏幕边缘的吸附可能性
        CheckScreenEdgeSnapping(thisLeft, thisTop, thisRight, thisBottom,
                ref minDistanceHorizontal,
                ref minDistanceVertical,
                ref horizontalBorder,
                ref verticalBorder,
                ref snapDirectionHorizontal,
                ref snapDirectionVertical);

        // 如果找到最近的窗口且距离小于阈值，则执行吸附
        // 水平方向
        if (snapDirectionHorizontal != SnapDirection.None && minDistanceHorizontal < snapThreshold)
        {
            ExecuteWindowSnap(horizontalBorder, snapDirectionHorizontal);
        }
        // 垂直方向
        if (snapDirectionVertical != SnapDirection.None && minDistanceVertical < snapThreshold)
        {
            ExecuteWindowSnap(verticalBorder, snapDirectionVertical);
        }
    }


    /// <summary>
    /// 检查和窗口边缘的吸附可能性
    /// </summary>
    private void CheckWindowEdgeSnapping(float thisLeft, float thisTop, float thisRight, float thisBottom,
                                   RectTransform otherWindowRect,
                                   ref float minDistanceHorizontal,
                                   ref float minDistanceVertical,
                                   ref float horizontalBorder,
                                   ref float verticalBorder,
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
            horizontalBorder = otherRight;
            snapDirectionHorizontal = SnapDirection.Left;
        }

        // 检查右吸附
        if (rightDistance < minDistanceHorizontal && rightDistance < snapThreshold)
        {
            minDistanceHorizontal = rightDistance;
            horizontalBorder = otherLeft;
            snapDirectionHorizontal = SnapDirection.Right;
        }

        // 检查上吸附
        if (topDistance < minDistanceVertical && topDistance < snapThreshold)
        {
            minDistanceVertical = topDistance;
            verticalBorder = otherBottom;
            snapDirectionVertical = SnapDirection.Top;
        }

        // 检查下吸附
        if (bottomDistance < minDistanceVertical && bottomDistance < snapThreshold)
        {
            minDistanceVertical = bottomDistance;
            verticalBorder = otherTop;
            snapDirectionVertical = SnapDirection.Bottom;
        }
    }


    /// <summary>
    /// 检查和屏幕边缘的吸附可能性
    /// </summary>
    private void CheckScreenEdgeSnapping(float thisLeft, float thisTop, float thisRight, float thisBottom,
                                   ref float minDistanceHorizontal,
                                   ref float minDistanceVertical,
                                   ref float horizontalBorder,
                                   ref float verticalBorder,
                                   ref SnapDirection snapDirectionHorizontal,
                                   ref SnapDirection snapDirectionVertical)
    {
        var (screenLeft, screenTop, screenRight, screenBottom) = GetFourBorders(WindowsManager.Instance.Desktop);

        var leftDistance = Mathf.Abs(screenLeft - thisLeft);
        var rightDistance = Mathf.Abs(screenRight - thisRight);
        var topDistance = Mathf.Abs(screenTop - thisTop);
        var bottomDistance = Mathf.Abs(screenBottom - thisBottom);

        // 检查左吸附
        if (leftDistance < minDistanceHorizontal && leftDistance < snapThreshold)
        {
            minDistanceHorizontal = leftDistance;
            horizontalBorder = screenLeft + constBorder;
            snapDirectionHorizontal = SnapDirection.Left;
        }

        // 检查右吸附
        if (rightDistance < minDistanceHorizontal && rightDistance < snapThreshold)
        {
            minDistanceHorizontal = rightDistance;
            horizontalBorder = screenRight - constBorder;
            snapDirectionHorizontal = SnapDirection.Right;
        }

        // 检查上吸附
        if (topDistance < minDistanceVertical && topDistance < snapThreshold)
        {
            minDistanceVertical = topDistance;
            verticalBorder = screenTop - constBorder;
            snapDirectionVertical = SnapDirection.Top;
        }

        // 检查下吸附
        if (bottomDistance < minDistanceVertical && bottomDistance < snapThreshold)
        {
            minDistanceVertical = bottomDistance;
            verticalBorder = screenBottom + constBorder;
            snapDirectionVertical = SnapDirection.Bottom;
        }
    }


    /// <summary>
    /// 执行窗口边缘吸附
    /// </summary>
    /// <param name="target"></param>
    /// <param name="direction"></param>
    private void ExecuteWindowSnap(float border, SnapDirection direction)
    {
        Vector2 targetPosition = thisWindowRect.position;

        var halfWidth = thisWindowRect.rect.width / 2;
        var halfHeight = thisWindowRect.rect.height / 2;

        switch (direction)
        {
            case SnapDirection.Left:
                targetPosition.x = border + halfWidth - constBorder;
                break;
            case SnapDirection.Right:
                targetPosition.x = border - halfWidth + constBorder;
                break;
            case SnapDirection.Top:
                targetPosition.y = border - halfHeight + constBorder;
                break;
            case SnapDirection.Bottom:
                targetPosition.y = border + halfHeight - constBorder;
                break;
        }

        thisWindowRect.position = targetPosition;
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
