﻿using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
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
    [SerializeField] private bool destroyAfterClosed = false;

    [SerializeField] private float animDuration = 0.2f;

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
    private Vector3 lastScale;
    private Vector3 lastSizeDelta;

    private bool focused = false;

    private Sequence anim;

    public bool IsPlayingAnim => anim != null && anim.IsActive();

    protected override void Awake()
    {
        base.Awake();

        // 添加拖拽支持
        Transform topBar = transform.Find("TopBar");
        if (topBar.TryGetComponent(out dragMoveHandler))
        {
            dragMoveHandler.targetToMove = transform as RectTransform;
            dragMoveHandler.onPointerDown.AddListener(Focus);
        }

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
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("AwakeWindow", AppName));
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

        // 启用窗口拖拽
        if (dragMoveHandler != null)
            dragMoveHandler.enabled = true;
        // 最大化按钮图标改变
        if (maximizeButton.gameObject.activeSelf)
        {
            maximizeButton.normalImage.sprite = maximize_default;
            (maximizeButton.hoveredGraphics[0] as Image).sprite = maximize_hovered;
        }

        SetState(WindowState.Normal);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        if (IsPlayingAnim)
            anim.Kill();

        anim = DOTween.Sequence();

        anim.Join(transform.DOMove(lastPosition, animDuration));
        anim.Join(transform.DOScale(lastScale, animDuration));
        anim.Join(GetComponent<RectTransform>().DOSizeDelta(lastSizeDelta, animDuration));

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
        if (IsPlayingAnim)
            anim.Kill();

        anim = DOTween.Sequence();

        // 同时执行缩小和移动动画
        anim.Join(transform.DOScale(Vector3.one * .01f, animDuration));
        anim.Join(transform.DOMove(shortcut.position, animDuration));

        anim.OnComplete(() => { canvasGroup.blocksRaycasts = false; });

        // 播放动画
        anim.Play();
        
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
            maximizeButton.normalImage.sprite = restore_default;
            (maximizeButton.hoveredGraphics[0] as Image).sprite = restore_hovered;
        }

        // 保存当前状态以便恢复
        RecordLastTransformInfo();

        SetState(WindowState.Maximized);


        //// 获取桌面的RectTransform作为最大化的参考尺寸
        RectTransform targetRect = GameObject.Find("Desktop").transform as RectTransform;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // 使用DOTween创建动画序列
        if (IsPlayingAnim)
            anim.Kill();

        anim = DOTween.Sequence();

        // 同时执行移动和缩放动画
        anim.Join(transform.DOMove(targetRect.position, animDuration));
        anim.Join(transform.DOScale(Vector3.one, animDuration));
        anim.Join(GetComponent<RectTransform>().DOSizeDelta(targetRect.rect.size, animDuration));
        anim.OnComplete(() =>
        {
            canvasGroup.interactable = true;
        });

        // 播放动画
        anim.Play();
        SoundManager.Instance.PlaySound("低沉泡泡音", true);
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

        this.focused = focused;
        focusFrameImage.gameObject.SetActive(focused);
    }
}