using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum WindowState
{
    Normal = 0,
    Maximized = 1,
    Minimized = 2,
    Closed = 3,
}


public abstract class WindowBase : PanelBase, IPointerDownHandler
{
    public string AppName => GetType().Name.Replace("Window", "");

    private Button closeButton;
    private Button maximizeButton;
    private Button minimizeButton;

    private WindowState lastState = WindowState.Closed;
    private WindowState state = WindowState.Closed;

    private Vector2 lastPosition;
    private Vector2 lastScale;

    private bool focused = false;

    protected override void Start()
    {
        closeButton = transform.Find("TopBar/CloseButton").GetComponent<Button>();
        maximizeButton = transform.Find("TopBar/MaximizeButton").GetComponent<Button>();
        minimizeButton = transform.Find("TopBar/MinimizeButton").GetComponent<Button>();

        closeButton.onClick.AddListener(() => WindowsManager.Instance.CloseWindow(AppName));
        maximizeButton.onClick.AddListener(() => WindowsManager.Instance.MaximizeWindow(AppName));
        minimizeButton.onClick.AddListener(() => WindowsManager.Instance.MinimizeWindow(AppName));

        lastPosition = transform.position;
        lastScale = transform.localScale;
        base.Start();
    }

    public void Open()
    {
        // 根据当前是否最小化采用不同的显示方式
        if (state == WindowState.Normal || state == WindowState.Maximized) return;

        if (state == WindowState.Closed)
        {
            Show();
            lastState = state;
            state = WindowState.Normal;
        }
        else
        {
            if (lastState == WindowState.Normal)
            {

            }
            else if (lastState == WindowState.Maximized)
            {

            }
        }
    }

    public void Close()
    {
        state = lastState = WindowState.Closed;
        Hide(onFinished: () => Destroy(gameObject));
    }

    public void Minimize()
    {
        lastState = state;
        state = WindowState.Minimized;
    }

    public void Maximize()
    {
        lastState = state;
        state = WindowState.Maximized;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        WindowsManager.Instance.FocusWindow(this);
        Debug.Log("down");
    }

    // 不要由自己调用
    // 不要由自己调用
    // 不要由自己调用
    public void SetFocused(bool focused)
    {
        if (this.focused == focused) return;

        if (focused)
        {

        }
        else
        {

        }
    }
}