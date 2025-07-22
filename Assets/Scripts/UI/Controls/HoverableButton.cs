using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening; // 引入DOTween命名空间

public class HoverableButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image normalImage; // 正常状态的图像
    public Image hoveredImage; // 鼠标悬停时显示的图像
    public float fadeDuration = 0.1f; // 淡入淡出持续时间

    public UnityEvent onClick { get; set; } = new UnityEvent();
    public UnityEvent onPointerEnter { get; set; } = new UnityEvent();

    protected virtual void Awake()
    {
        // 初始化时确保hoveredImage是透明的
        if (hoveredImage != null)
        {
            Color color = hoveredImage.color;
            color.a = 0f;
            hoveredImage.color = color;
            hoveredImage.gameObject.SetActive(false);
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        onPointerEnter?.Invoke();

        if (hoveredImage == null) return;

        // 激活图像并开始淡入动画
        hoveredImage.gameObject.SetActive(true);
        hoveredImage.DOKill(); // 停止所有正在进行的动画

        hoveredImage.DOFade(1, fadeDuration)
            .SetEase(Ease.OutQuad);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (hoveredImage == null) return;

        // 开始淡出动画，完成后禁用图像
        hoveredImage.DOKill(); // 停止所有正在进行的动画

        hoveredImage.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => hoveredImage.gameObject.SetActive(false));
    }

    protected virtual void OnDestroy()
    {
        // 清理DOTween动画
        if (hoveredImage != null)
        {
            hoveredImage.DOKill();
        }
    }
}