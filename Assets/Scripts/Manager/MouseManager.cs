using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public Sprite DefaultSprite; // 默认
    public Sprite ClickSprite; // 点击
    public Sprite ClickDownSprite;//点下
    public Sprite DragSprite; // 拖拽
    public Sprite ResizeCornerSprite; // 右上角
    public Sprite ResizeSideSprite; //Y轴
    public Sprite InputSprite; // 输入框

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
        ChangeMouseState(MouseState.Default);
    }

    public void Update()
    {
        setCursor();
    }
    void setCursor()
    {
        Cursor.visible = false;
        //设置鼠标位置
        Vector3 curTransform = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        curTransform.z = 0;
        transform.position = curTransform;
    }

    public void ChangeMouseState(MouseState mouseState)
    {
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
}

