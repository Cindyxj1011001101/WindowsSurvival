using UnityEngine;
using UnityEngine.EventSystems;

public enum ChangeMouseType
{
    Hover,
    Drag,
    Scaler,
}

public class ChangeMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public ChangeMouseType changeMouseType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (changeMouseType)
        {
            case ChangeMouseType.Scaler:
                GetComponent<DragScaleHandler>().ChangeMouseByDirection();
                break;
            default:
                MouseManager.Instance.ChangeMouseState(MouseState.Click);
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseManager.Instance.ChangeMouseState(MouseState.Default);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (changeMouseType)
        {
            case ChangeMouseType.Hover:
                MouseManager.Instance.ChangeMouseState(MouseState.ClickDown);
                break;
            case ChangeMouseType.Drag:
                MouseManager.Instance.ChangeMouseState(MouseState.Drag);
                break;
            default:
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (changeMouseType)
        {
            case ChangeMouseType.Hover:
                MouseManager.Instance.ChangeMouseState(MouseState.Click);
                break;
            case ChangeMouseType.Drag:
                MouseManager.Instance.ChangeMouseState(MouseState.Click);
                break;
            default:
                break;
        }
    }

    public void OnDisable()
    {
        MouseManager.Instance.ChangeMouseState(MouseState.Default);
    }

}