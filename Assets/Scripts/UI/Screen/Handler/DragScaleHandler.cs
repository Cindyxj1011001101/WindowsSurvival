using UnityEngine;
using UnityEngine.EventSystems;

public class DragScaleHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum ScaleDirection
    {
        Left, Right, Top, Bottom,
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    public ScaleDirection direction;
    private RectTransform targetRect;
    private RectTransform canvasRect;

    public float minWidth = 300f;
    public float minHeight = 200f;
    public float maxWidth = 800f;
    public float maxHeight = 420f;

    private Vector2 startMouseLocalToParent;
    private Vector2 startOffsetMin;
    private Vector2 startOffsetMax;

    private bool isDragging = false;
    private bool isDirty = false;

    private float snapThreshold = 12f;
    private float constBorder = 2f;

    public void Awake()
    {
        targetRect = transform.parent.parent.GetComponent<RectTransform>();
        canvasRect = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
    }

    private void SnapDraggedEdgesAfterResize()
    {
        if (targetRect == null) return;
        if (WindowsManager.Instance == null) return;
        if (WindowsManager.Instance.Desktop == null) return;

        var parent = targetRect.parent as RectTransform;
        if (parent == null) return;

        var thisWindow = targetRect.GetComponent<WindowBase>();

        bool shouldSnapLeft = direction == ScaleDirection.Left || direction == ScaleDirection.TopLeft || direction == ScaleDirection.BottomLeft;
        bool shouldSnapRight = direction == ScaleDirection.Right || direction == ScaleDirection.TopRight || direction == ScaleDirection.BottomRight;
        bool shouldSnapTop = direction == ScaleDirection.Top || direction == ScaleDirection.TopLeft || direction == ScaleDirection.TopRight;
        bool shouldSnapBottom = direction == ScaleDirection.Bottom || direction == ScaleDirection.BottomLeft || direction == ScaleDirection.BottomRight;

        var (thisLeft, thisTop, thisRight, thisBottom) = MonoUtility.GetFourBorders(targetRect);
        var (screenLeft, screenTop, screenRight, screenBottom) = MonoUtility.GetFourBorders(WindowsManager.Instance.Desktop);

        float bestLeftTarget = thisLeft;
        float bestLeftDist = float.MaxValue;
        float bestRightTarget = thisRight;
        float bestRightDist = float.MaxValue;
        float bestTopTarget = thisTop;
        float bestTopDist = float.MaxValue;
        float bestBottomTarget = thisBottom;
        float bestBottomDist = float.MaxValue;

        if (shouldSnapLeft)
        {
            bestLeftTarget = screenLeft + constBorder;
            bestLeftDist = Mathf.Abs(bestLeftTarget - thisLeft);
        }

        if (shouldSnapRight)
        {
            bestRightTarget = screenRight - constBorder;
            bestRightDist = Mathf.Abs(bestRightTarget - thisRight);
        }

        if (shouldSnapTop)
        {
            bestTopTarget = screenTop - constBorder;
            bestTopDist = Mathf.Abs(bestTopTarget - thisTop);
        }

        if (shouldSnapBottom)
        {
            bestBottomTarget = screenBottom + constBorder;
            bestBottomDist = Mathf.Abs(bestBottomTarget - thisBottom);
        }

        foreach (var window in WindowsManager.Instance.GetOpenedWindows(true).Values)
        {
            if (window == null) continue;
            if (thisWindow != null && window == thisWindow) continue;

            var otherRect = window.transform as RectTransform;
            if (otherRect == null) continue;

            var (otherLeft, otherTop, otherRight, otherBottom) = MonoUtility.GetFourBorders(otherRect);

            if (shouldSnapLeft)
            {
                var dist = Mathf.Abs(otherRight - thisLeft);
                if (dist < bestLeftDist)
                {
                    bestLeftDist = dist;
                    bestLeftTarget = otherRight;
                }
            }

            if (shouldSnapRight)
            {
                var dist = Mathf.Abs(otherLeft - thisRight);
                if (dist < bestRightDist)
                {
                    bestRightDist = dist;
                    bestRightTarget = otherLeft;
                }
            }

            if (shouldSnapTop)
            {
                var dist = Mathf.Abs(otherBottom - thisTop);
                if (dist < bestTopDist)
                {
                    bestTopDist = dist;
                    bestTopTarget = otherBottom;
                }
            }

            if (shouldSnapBottom)
            {
                var dist = Mathf.Abs(otherTop - thisBottom);
                if (dist < bestBottomDist)
                {
                    bestBottomDist = dist;
                    bestBottomTarget = otherTop;
                }
            }
        }

        var offsetMin = targetRect.offsetMin;
        var offsetMax = targetRect.offsetMax;

        if (shouldSnapLeft && bestLeftDist < snapThreshold)
        {
            var deltaWorld = bestLeftTarget - thisLeft;
            var deltaLocal = parent.InverseTransformVector(new Vector3(deltaWorld, 0f, 0f)).x;
            offsetMin.x += deltaLocal;
            offsetMin.x = Mathf.Min(offsetMin.x, offsetMax.x - minWidth);
            offsetMin.x = Mathf.Max(offsetMin.x, offsetMax.x - maxWidth);
        }

        if (shouldSnapRight && bestRightDist < snapThreshold)
        {
            var deltaWorld = bestRightTarget - thisRight;
            var deltaLocal = parent.InverseTransformVector(new Vector3(deltaWorld, 0f, 0f)).x;
            offsetMax.x += deltaLocal;
            offsetMax.x = Mathf.Max(offsetMax.x, offsetMin.x + minWidth);
            offsetMax.x = Mathf.Min(offsetMax.x, offsetMin.x + maxWidth);
        }

        if (shouldSnapTop && bestTopDist < snapThreshold)
        {
            var deltaWorld = bestTopTarget - thisTop;
            var deltaLocal = parent.InverseTransformVector(new Vector3(0f, deltaWorld, 0f)).y;
            offsetMax.y += deltaLocal;
            offsetMax.y = Mathf.Max(offsetMax.y, offsetMin.y + minHeight);
            offsetMax.y = Mathf.Min(offsetMax.y, offsetMin.y + maxHeight);
        }

        if (shouldSnapBottom && bestBottomDist < snapThreshold)
        {
            var deltaWorld = bestBottomTarget - thisBottom;
            var deltaLocal = parent.InverseTransformVector(new Vector3(0f, deltaWorld, 0f)).y;
            offsetMin.y += deltaLocal;
            offsetMin.y = Mathf.Min(offsetMin.y, offsetMax.y - minHeight);
            offsetMin.y = Mathf.Max(offsetMin.y, offsetMax.y - maxHeight);
        }

        targetRect.offsetMin = offsetMin;
        targetRect.offsetMax = offsetMax;

        // 限制顶边栏不能拉出屏幕外
        targetRect.offsetMax = new Vector2(
            targetRect.offsetMax.x,
            Mathf.Clamp(targetRect.offsetMax.y, WindowsManager.Instance.Desktop.rect.yMin + 60, WindowsManager.Instance.Desktop.rect.yMax)
        );
    }

    public void ChangeMouseByDirection()
    {

        switch (direction)
        {
            case ScaleDirection.Left:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeX);
                break;
            case ScaleDirection.Right:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeX);
                break;
            case ScaleDirection.Top:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeY);
                break;
            case ScaleDirection.Bottom:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeY);
                break;
            case ScaleDirection.TopLeft:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeCounterDiagonal);
                break;
            case ScaleDirection.TopRight:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeMainDiagonal);
                break;
            case ScaleDirection.BottomLeft:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeMainDiagonal);
                break;
            case ScaleDirection.BottomRight:
                MouseManager.Instance.ChangeMouseState(MouseState.ResizeCounterDiagonal);
                break;

        }
    }

    public void OnBeginDrag(PointerEventData eventData)
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

        ChangeMouseByDirection();

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

        // 限制顶边栏不能拉出屏幕外
        targetRect.offsetMax = new Vector2(
            targetRect.offsetMax.x,
            Mathf.Clamp(targetRect.offsetMax.y, WindowsManager.Instance.Desktop.rect.yMin + 60, WindowsManager.Instance.Desktop.rect.yMax)
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 缩放结束后：仅对拖动的边/角进行吸附，保持未拖动边不变
        SnapDraggedEdgesAfterResize();
    }

    private void OnRectTransformDimensionsChange()
    {
        isDirty = true;
    }

    private void FixedUpdate()
    {
        if (isDragging && isDirty)
        {
            // 更新
            foreach (var item in targetRect.GetComponentsInChildren<IAdaptiveSize>())
            {
                item.UpdateSize();
            }
            isDirty = false;
        }
    }
}
