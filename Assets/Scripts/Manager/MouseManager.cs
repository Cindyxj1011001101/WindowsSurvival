using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public enum MouseState
{
    Default,
    Click,
    ClickDown,
    Drag,
    ResizeMainDiagonal,
    ResizeCounterDiagonal,
    ResizeX,
    ResizeY,
}

public class MouseManager : MonoBehaviour
{
    private static MouseManager instance;
    public static MouseManager Instance => instance;

    public const float BasicWaitTime = 19f / 24;

    public Stack<ChangeMouseType> curChangeMouseType = new Stack<ChangeMouseType>();

    public Sprite DefaultSprite; // 默认
    public Sprite ClickSprite; // 点击
    public Sprite ClickDownSprite;//点下
    public Sprite DragSprite; // 拖拽
    public Sprite ResizeCornerSprite; // 右上角
    public Sprite ResizeSideSprite; //Y轴
    public Sprite InputSprite; // 输入框

    [SerializeField] private bool isDragging;

    public Animator animator;
    public CanvasGroup mouseCanvasGroup;


    public void Awake()
    {
        Cursor.visible = false;
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else{
            instance=this;
        }
        animator.gameObject.SetActive(false);
        ChangeMouseState(MouseState.Default);
    }

    public void Update()
    {
        SetCursor();
    }

    private void SetCursor()
    {
        Cursor.visible = false;
        //设置鼠标位置
        Vector3 curTransform = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        curTransform.z = 0;
        transform.position = curTransform;
    }

    public void StartDragging()
    {
        ChangeMouseState(MouseState.Drag);
        isDragging = true;
    }

    public void EndDragging()
    {
        isDragging = false;
        ChangeMouseState(MouseState.Default);
    }

    public void ChangeMouseState(MouseState mouseState)
    {
        if (isDragging) return;

        ResetRotation(mouseState);
        switch (mouseState)
        {
            case MouseState.Default:
                ResetPivot(DefaultSprite);
                GetComponent<Image>().sprite = DefaultSprite;
                break;
            case MouseState.Click:
                ResetPivot(ClickSprite);
                GetComponent<Image>().sprite = ClickSprite;
                break;
            case MouseState.ClickDown:
                ResetPivot(ClickDownSprite);
                GetComponent<Image>().sprite = ClickDownSprite;
                break;
            case MouseState.Drag:
                ResetPivot(DragSprite);
                GetComponent<Image>().sprite = DragSprite;
                break;
            case MouseState.ResizeMainDiagonal:
                ResetPivot(ResizeCornerSprite);
                GetComponent<Image>().sprite = ResizeCornerSprite;
                break;
            case MouseState.ResizeCounterDiagonal:
                ResetPivot(ResizeCornerSprite);
                GetComponent<Image>().sprite = ResizeCornerSprite;
                break;
            case MouseState.ResizeX:
                ResetPivot(ResizeSideSprite);
                GetComponent<Image>().sprite = ResizeSideSprite;
                break;
            case MouseState.ResizeY:
                ResetPivot(ResizeSideSprite);
                GetComponent<Image>().sprite = ResizeSideSprite;
                break;
            default:
                break;
        }
    }


    //根据图片设置组件中心点位置
    public void ResetPivot(Sprite sprite)
    {
        // 1. 获取pivot（像素单位）
        Vector2 pivotPixel = sprite.pivot;

        // 2. 获取pivot（归一化到0~1）
        Vector2 pivotNormalized = new Vector2(
            sprite.pivot.x / sprite.rect.width,
            sprite.pivot.y / sprite.rect.height
        );

        // 3. 设置RectTransform的pivot
        GetComponent<RectTransform>().pivot = pivotNormalized;

    }

    public void ResetRotation(MouseState mouseState)
    {
        switch (mouseState)
        {
            case MouseState.ResizeCounterDiagonal:
                GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MouseState.ResizeX:
                GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 90);
                break;
            default:
                GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                break;
        }
    }

    public void Wait(float waitTime = BasicWaitTime, UnityAction callBack = null)
    {
        StartCoroutine(PlayWaitingAnim(waitTime, callBack));
    }

    private IEnumerator PlayWaitingAnim(float waitTime, UnityAction callBack)
    {
        // 禁用鼠标
        SetMouseEnabled(false);

        // 隐藏鼠标
        SetMouseVisible(false);

        // 播放动画
        animator.gameObject.SetActive(true);
        animator.Play("Waiting");

        // 等待动画播放完毕
        yield return new WaitForSeconds(waitTime);

        // 执行回调
        callBack?.Invoke();

        // 停止动画
        animator.Play("");
        animator.gameObject.SetActive(false);

        // 显示鼠标
        SetMouseVisible(true);

        // 启用鼠标
        SetMouseEnabled(true);
    }

    private void SetMouseVisible(bool visible)
    {
        mouseCanvasGroup.alpha = visible ? 1 : 0;
    }

    private void SetMouseEnabled(bool enabled)
    {
       mouseCanvasGroup.interactable = !enabled;
    }
}

