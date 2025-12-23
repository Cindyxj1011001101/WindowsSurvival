using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;

public enum WindowState
{
    Default = 0,
    Maximized = 1,
    Minimized = 2,
    Closed = 3,
}

public abstract class WindowBase : PanelBase
{
    [SerializeField] private bool ignoreThisWhenSave;

    public bool IgnoreThisWhenSave => ignoreThisWhenSave;

    [SerializeField] private bool destroyAfterClosed = false;

    // 动画管理器引用
    private AnimationManager Anim => AnimationManager.Instance;

    public string AppName => GetType().Name.Replace("Window", "");

    private HoverableButton closeButton;
    private HoverableButton maximizeButton;
    private HoverableButton minimizeButton;

    [SerializeField] private Sprite maximize_default;
    [SerializeField] private Sprite maximize_hovered;
    [SerializeField] private Sprite restore_default;
    [SerializeField] private Sprite restore_hovered;
    
    private Image focusFrameImage;

    private DragMoveHandler dragMoveHandler;


    private WindowState lastState = WindowState.Closed;
    private WindowState state = WindowState.Closed;

    private Vector3 lastPosition;
    private Vector3 lastSizeDelta;

    protected bool focused = false;

    [SerializeField] private bool isModal;

    public bool IsModal => isModal;

    public WindowState LastState => lastState;
    public WindowState State => state;

    public Vector3 LastPosition => lastPosition;
    public Vector3 LastSizeDelta => lastSizeDelta;

    public RectTransform RectTransform => transform as RectTransform;

    private Sequence anim;

    public bool IsPlayingAnim => anim != null && anim.IsActive();

    protected override void Awake()
    {
        base.Awake();

        // 添加拖拽支持
        Transform topBar = transform.Find("TopBar");
        //if (topBar.TryGetComponent(out dragMoveHandler))
        //{
        //    //dragMoveHandler.targetToMove = RectTransform;
        //    dragMoveHandler.onPointerDown.AddListener(Focus);
        //}
        dragMoveHandler = topBar.GetComponent<DragMoveHandler>();

        // 添加双击支持
        if (topBar.TryGetComponent<DoubleClickHandler>(out var doubleClickHandler))
            doubleClickHandler.onDoubleClick.AddListener(MaximizeOrRestore);

        closeButton = transform.Find("TopBar/CloseButton").GetComponent<HoverableButton>();
        maximizeButton = transform.Find("TopBar/MaximizeButton").GetComponent<HoverableButton>();
        minimizeButton = transform.Find("TopBar/MinimizeButton").GetComponent<HoverableButton>();

        closeButton.onClick.AddListener(OnCloseButtonClicked);
        maximizeButton.onClick.AddListener(OnMaximizeButtonClicked);
        minimizeButton.onClick.AddListener(OnMinimizeButtonClicked);

        // 将聚焦框设置为不可见
        focusFrameImage = transform.Find("Frame").GetComponent<Image>();
        focusFrameImage.gameObject.SetActive(false);
    }

    public override void Show(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (showMode != ShowMode.Fade)
        {
            base.Show(showMode, onFinished);
            return;
        }

        if (onFinished != null)
            onShown.AddListener(onFinished);

        canvasGroup.DOKill();
        RectTransform.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        var targetPosition = RectTransform.position;
        anim = Anim.PlayWindowOpen(RectTransform, canvasGroup, targetPosition) as Sequence;
        if (anim == null)
        {
            // fallback：至少保证可见
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            onShown?.Invoke();
            onShown.RemoveAllListeners();
            return;
        }

        anim.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            onShown?.Invoke();
            onShown.RemoveAllListeners();
        });

        anim.Play();
    }

    public override void Hide(ShowMode showMode = ShowMode.Fade, UnityAction onFinished = null)
    {
        if (showMode != ShowMode.Fade)
        {
            base.Hide(showMode, onFinished);
            return;
        }

        if (onFinished != null)
            onHidden.AddListener(onFinished);

        canvasGroup.DOKill();
        RectTransform.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        var targetPosition = RectTransform.position;
        anim = Anim.PlayWindowClose(RectTransform, canvasGroup, targetPosition) as Sequence;
        if (anim == null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            onHidden?.Invoke();
            onHidden.RemoveAllListeners();
            return;
        }

        anim.OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            onHidden?.Invoke();
            onHidden.RemoveAllListeners();
        });

        anim.Play();
    }

    public void InitFromWindowData(WindowData data)
    {
        SetState(data.state);

        SetModal(data.isModal);

        RectTransform.anchoredPosition = data.position;
        RectTransform.sizeDelta = data.sizeDelta;
        RectTransform.localScale = data.scale;

        lastState = data.lastState;
        lastPosition = data.lastPosition;
        lastSizeDelta = data.lastSizeDelta;


        if (state == WindowState.Minimized || state == WindowState.Closed)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Show(ShowMode.None);
        }
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

    public void SetModal(bool isModal)
    {
        this.isModal = isModal;
        if (isModal)
        {
            // 禁用最小化和关闭
            closeButton.Interactable = minimizeButton.Interactable = false;
        }
    }

    private void SetState(WindowState state)
    {
        lastState = this.state;
        this.state = state;

        var isMaximized = state == WindowState.Maximized;

        // 启用窗口拖拽
        if (dragMoveHandler != null)
            dragMoveHandler.enabled = !isMaximized;

        // 最大化按钮图标改变
        if (maximizeButton.gameObject.activeSelf)
        {
            maximizeButton.image.sprite = isMaximized ? restore_default : maximize_default;
            (maximizeButton.hoveredGraphics[0] as Image).sprite = isMaximized ? restore_hovered : maximize_hovered;
        }
    }

    public void ForceSetPositionAndSizeDelta(PositionAndSizeDelta args)
    {
        SetState(WindowState.Default);

        if (anim != null && anim.IsActive())
            anim.Kill();

        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = canvasGroup.interactable = true;

        RectTransform.localScale = Vector3.one;
        SetPositionAndSizeDelta(args);
    }

    public void SetPositionAndSizeDelta(PositionAndSizeDelta args)
    {
        RectTransform.anchoredPosition = args.position;
        RectTransform.sizeDelta = args.sizeDelta;

        foreach (var item in GetComponentsInChildren<IAdaptiveSize>())
        {
            item.UpdateSize();
        }
    }

    public void Open()
    {
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("AwakeWindow", AppName));
        switch (state)
        {
            case WindowState.Default:
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
        SetState(WindowState.Default);

        // 设置默认位置
        lastPosition = RectTransform.anchoredPosition;
        lastSizeDelta = RectTransform.sizeDelta;

        Show();
    }

    public void Restore()
    {
        if (state == WindowState.Default) return;

        if (lastState == WindowState.Maximized)
        {
            Maximize();
            return;
        }

        SetState(WindowState.Default);

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        anim = Anim.PlayWindowRestore(RectTransform, canvasGroup, lastPosition, lastSizeDelta) as Sequence;

        anim.OnComplete(() =>
        {
            canvasGroup.interactable = true;
        });

        anim.Play();
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

    public virtual void Minimize(Transform shortcut)
    {
        if (state == WindowState.Minimized) return;

        // 保存当前状态以便恢复
        // 保存当前状态的代码一定要在SetState之前
        RecordLastTransformInfo();

        SetState(WindowState.Minimized);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        anim = Anim.PlayWindowMinimize(transform, canvasGroup, shortcut) as Sequence;
    }

    public void Maximize()
    {
        if (state == WindowState.Maximized) return;

        // 禁止拖拽窗口
        if (dragMoveHandler != null)
            dragMoveHandler.enabled = false;
        // 最大化按钮图标改变
        if (maximizeButton.gameObject.activeSelf)
        {
            maximizeButton.image.sprite = restore_default;
            (maximizeButton.hoveredGraphics[0] as Image).sprite = restore_hovered;
        }

        // 保存当前状态以便恢复
        RecordLastTransformInfo();

        SetState(WindowState.Maximized);


        // 获取桌面的RectTransform作为最大化的参考尺寸
        RectTransform targetRect = WindowsManager.Instance.Desktop;

        canvasGroup.alpha = 1;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        anim = Anim.PlayWindowMaximize(RectTransform, canvasGroup, targetRect) as Sequence;
        SoundManager.Instance.PlaySound("低沉泡泡音", true);
    }

    private void MaximizeOrRestore()
    {
        if (state == WindowState.Maximized)
            Restore();
        else if (state == WindowState.Default)
            Maximize();
    }

    private void RecordLastTransformInfo()
    {
        // 只保存Normal状态下窗口的信息
        if (state != WindowState.Default) return;
        
        lastPosition = transform.position;
        lastSizeDelta = RectTransform.sizeDelta;
    }

    private void Focus()
    {
        WindowsManager.Instance.FocusWindow(this);
    }

    // 不要由自己调用
    // 不要由自己调用
    // 不要由自己调用
    public void SetFocused(bool focused)
    {
        if (this.focused == focused) return;

        this.focused = focused;

        if (focused) OnFocused();

        focusFrameImage.gameObject.SetActive(focused);
    }

    protected virtual void OnFocused() { }
}