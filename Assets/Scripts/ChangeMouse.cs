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
    [SerializeField]private bool InButton;//判断是否在按钮中

    public void OnPointerEnter(PointerEventData eventData)
    {
        InButton = true;
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
        InButton = false;
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
        if (InButton == false)
        {
            MouseManager.Instance.ChangeMouseState(MouseState.Default);
            return;
        }
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