using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
public enum WindowState
{
    Normal = 0,
    Maximized = 1,
    Minimized = 2,
    Closed = 3,
}


public abstract class WindowBase : PanelBase, IPointerDownHandler
{
    [SerializeField] private bool destroyAfterClosed = false;

    public string AppName => GetType().Name.Replace("Window", "");

    private Button closeButton;
    private Button maximizeButton;
    private Button minimizeButton;

    private WindowState lastState = WindowState.Closed;
    private WindowState state = WindowState.Closed;

    private Vector2 lastPosition;
    private Vector2 lastScale;
    private Vector2 lastSizeDelta;

    private bool focused = false;

    private Transform topBar;

    protected override void Awake()
    {
        base.Awake();

        // 添加拖拽支持
        topBar = transform.Find("TopBar");
        DragMoveHandler dragMoveHandler = topBar.GetComponent<DragMoveHandler>();
        if (dragMoveHandler == null)
            dragMoveHandler = topBar.gameObject.AddComponent<DragMoveHandler>();
        dragMoveHandler.targetToMove = transform as RectTransform;
        dragMoveHandler.onPointerDown.AddListener(Focus);

        // 添加双击支持
        DoubleClickHandler doubleClickHandler = topBar.GetComponent<DoubleClickHandler>();
        if (doubleClickHandler == null)
            doubleClickHandler = topBar.gameObject.AddComponent<DoubleClickHandler>();
        doubleClickHandler.onDoubleClick.AddListener(MaximizeOrRestore);

        closeButton = transform.Find("TopBar/CloseButton").GetComponent<Button>();
        maximizeButton = transform.Find("TopBar/MaximizeButton").GetComponent<Button>();
        minimizeButton = transform.Find("TopBar/MinimizeButton").GetComponent<Button>();

        closeButton.onClick.AddListener(OnCloseButtonClicked);
        maximizeButton.onClick.AddListener(OnMaximizeButtonClicked);
        minimizeButton.onClick.AddListener(OnMinimizeButtonClicked);

        //gameObject.SetActive(false);
    }

    private void OnCloseButtonClicked()
    {
        Focus();
        WindowsManager.Instance.CloseWindow(AppName);
    }

    private void OnMaximizeButtonClicked()
    {
        Focus();
        MaximizeOrRestore();
    }

    private void OnMinimizeButtonClicked()
    {
        Focus();
        WindowsManager.Instance.MinimizeWindow(AppName);
    }

    private void SetState(WindowState state)
    {
        lastState = this.state;
        this.state = state;
    }

    public void Open()
    {
        //gameObject.SetActive(true);

        switch (state)
        {
            case WindowState.Normal:
            case WindowState.Maximized:
                return;

            case WindowState.Minimized:
                Restore();
                break;
            case WindowState.Closed:
                Create();
                break;
        }
    }

    public void Create()
    {
        SetState(WindowState.Normal);
        Show();
    }

    public void Restore()
    {
        if (state == WindowState.Normal) return;

        if (lastState == WindowState.Maximized)
        {
            Maximize();
            return;
        }

        SetState(WindowState.Normal);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        Sequence restoreSequence = DOTween.Sequence();

        restoreSequence.Join(transform.DOMove(lastPosition, .2f));
        restoreSequence.Join(transform.DOScale(lastScale, .2f));
        restoreSequence.Join(GetComponent<RectTransform>().DOSizeDelta(lastSizeDelta, 0.3f));

        restoreSequence.OnComplete(() =>
        {
            canvasGroup.interactable = true;
        });

        restoreSequence.Play();
    }

    public void Close()
    {
        if (state == WindowState.Closed) return;

        SetState(WindowState.Closed);

        if (destroyAfterClosed)
            Hide(onFinished: () => Destroy(gameObject));
        else
            Hide();
    }

    public void Minimize(Transform shortcut)
    {
        if (state == WindowState.Minimized) return;

        // 保存当前状态以便恢复
        // 保存当前状态的代码一定要卸载SetState之前
        RecordLastTransformInfo();

        SetState(WindowState.Minimized);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // 使用DOTween创建动画序列
        Sequence minimizeSequence = DOTween.Sequence();

        // 同时执行缩小和移动动画
        minimizeSequence.Join(transform.DOScale(Vector3.zero, 0.2f));
        minimizeSequence.Join(transform.DOMove(shortcut.position, 0.2f));

        minimizeSequence.OnComplete(() => { canvasGroup.blocksRaycasts = false; });

        // 播放动画
        minimizeSequence.Play();
    }

    public void Maximize()
    {
        if (state == WindowState.Maximized) return;

        // 保存当前状态以便恢复
        RecordLastTransformInfo();

        SetState(WindowState.Maximized);

        // 获取Canvas的RectTransform作为最大化的参考尺寸
        RectTransform targetRect = WindowsManager.Instance.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        //// 获取桌面的RectTransform作为最大化的参考尺寸
        //RectTransform targetRect = WindowsManager.Instance.Desktop.transform as RectTransform;

        // 使用DOTween创建动画序列
        Sequence maximizeSequence = DOTween.Sequence();

        // 同时执行移动和缩放动画
        maximizeSequence.Join(transform.DOMove(targetRect.position, 0.2f));
        maximizeSequence.Join(transform.DOScale(Vector3.one, 0.2f));
        maximizeSequence.Join(GetComponent<RectTransform>().DOSizeDelta(targetRect.sizeDelta, 0.2f));

        // 播放动画
        maximizeSequence.Play();
    }

    private void MaximizeOrRestore()
    {
        if (state == WindowState.Maximized)
            Restore();
        else if (state == WindowState.Normal)
            Maximize();
    }

    private void RecordLastTransformInfo()
    {
        // 只保存Normal状态下窗口的信息
        if (state != WindowState.Normal) return;
        
        lastPosition = transform.position;
        lastScale = transform.localScale;
        lastSizeDelta = (transform as RectTransform).sizeDelta;
    }

    private void Focus()
    {
        WindowsManager.Instance.FocusWindow(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Focus();
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