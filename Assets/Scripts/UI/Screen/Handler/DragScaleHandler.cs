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

    public void Awake()
    {
        targetRect = transform.parent.parent.GetComponent<RectTransform>();
        canvasRect = FindObjectOfType<Canvas>().GetComponent<RectTransform>();
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
