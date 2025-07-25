using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic; // 引入DOTween命名空间

public class HoverableButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image normalImage; // 正常状态的图像
    public List<Graphic> hoveredGraphics; // 鼠标悬停时显示的图像
    public float fadeDuration = 0.1f; // 淡入淡出持续时间

    public Text[] textsNeedToReverseColor;
    public Image[] imagseNeedToReverseColor;

    public Color currentColor;
    //[SerializeField]
    //protected Color reversedColor = new Color(17, 17, 17, 255);

    public UnityEvent onClick { get; set; } = new UnityEvent();
    public UnityEvent onPointerEnter { get; set; } = new UnityEvent();
    public UnityEvent onPointerExit { get; set; } = new UnityEvent();

    public bool Interactable
    {
        get => interactable;
        set
        {
            if (!value)
                foreach (var graphic in hoveredGraphics)
                {
                    graphic.DOKill();
                    graphic.gameObject.SetActive(false); // 确保初始状态下图像不可见
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f); // 设置透明度为0
                }
            interactable = value;
        }
    }

    private bool interactable = true;

    protected virtual void Awake()
    {
        var hoveredGraphic = transform.Find("Hovered");
        if (hoveredGraphic != null)
            hoveredGraphics.AddRange(hoveredGraphic.GetComponentsInChildren<Graphic>());

        if (normalImage != null)
            currentColor = normalImage.color;
        else
            currentColor = ColorManager.Instance.white;

        // 初始化时确保hoveredImage是透明的
        foreach (var graphic in hoveredGraphics)
        {
            graphic.gameObject.SetActive(false); // 确保初始状态下图像不可见
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f); // 设置透明度为0
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable) return; // 如果不可交互，则不处理点击事件

        onClick?.Invoke();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;

        onPointerEnter?.Invoke();

        //if (normalImage != null)
        //    currentColor = normalImage.color;

        // 激活图像并开始淡入动画
        foreach (var graphic in hoveredGraphics)
        {
            graphic.gameObject.SetActive(true); // 确保图像可见
            graphic.DOKill(); // 停止所有正在进行的动画
            graphic.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnStart(() => ChangeColor(ColorManager.Instance.black)); // 在动画开始时改变颜色
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable) return;

        onPointerExit?.Invoke();

        // 开始淡出动画，完成后禁用图像
        foreach (var graphic in hoveredGraphics)
        {
            graphic.DOKill(); // 停止所有正在进行的动画
            graphic.DOFade(0f, fadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => graphic.gameObject.SetActive(false)) // 动画完成后禁用图像
                .OnStart(() => ChangeColor(currentColor));
        }
    }

    protected virtual void OnDestroy()
    {
        // 清理DOTween动画
        foreach (var graphic in hoveredGraphics)
        {
            graphic.DOKill(); // 停止所有正在进行的动画
        }
    }

    public void ChangeColor(Color color)
    {
        foreach (var text in textsNeedToReverseColor)
        {
            text.color = color;
        }
        foreach (var image in imagseNeedToReverseColor)
        {
            image.color = color;
        }
    }
}