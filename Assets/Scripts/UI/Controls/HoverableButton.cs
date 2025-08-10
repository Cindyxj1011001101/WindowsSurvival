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

    public CanvasGroup canvasGroup {  get; private set; }

    public string hoveredAudio = "临时悬浮";
    public bool playHoverSound = true;

    public Color currentColor { get; set; }
    public Color hoveredColor { get; set; } = ColorManager.White; // 鼠标悬停时的颜色，默认为白色

    public UnityEvent onClick { get; set; } = new UnityEvent();
    public UnityEvent onPointerEnter { get; set; } = new UnityEvent();
    public UnityEvent onPointerExit { get; set; } = new UnityEvent();

    public bool Interactable
    {
        get => interactable;
        set
        {
            interactable = value;
            if (!value)
            {
                foreach (var graphic in hoveredGraphics)
                {
                    graphic.DOKill();
                    graphic.gameObject.SetActive(false); // 确保初始状态下图像不可见
                    graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0f); // 设置透明度为0
                }
            }
            else
            {
                ChangeColor(currentColor);
            }
        }
    }

    private bool interactable = true;

    protected virtual void Awake()
    {
        if (normalImage != null)
            currentColor = normalImage.color;
        else
            currentColor = ColorManager.White;

        var hoveredGraphic = transform.Find("Hovered");
        if (hoveredGraphic != null)
            hoveredGraphics.AddRange(hoveredGraphic.GetComponentsInChildren<Graphic>());

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    protected virtual void Start()
    {
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
        OnPointerEnter(eventData); // 点击时触发鼠标悬停事件
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable) return;

        if (playHoverSound)
        SoundManager.Instance.PlaySound(hoveredAudio, true,0.2f);

        onPointerEnter?.Invoke();

        // 激活图像并开始淡入动画
        foreach (var graphic in hoveredGraphics)
        {
            graphic.gameObject.SetActive(true); // 确保图像可见
            graphic.DOKill(); // 停止所有正在进行的动画
            graphic.DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
                .OnStart(() =>
                {
                    ChangeColor(ColorManager.Black); // 反色
                    graphic.color = hoveredColor; // 改变悬浮框的颜色
                }); // 在动画开始时改变颜色
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

    public void SetVisiable(bool visiable)
    {
        if (visiable)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = canvasGroup.interactable = false;
        }
    }

    public void StartBlinking(float interval = .5f)
    {
        currentColor = Color.white;
        ChangeColor(ColorManager.White);

        canvasGroup.DOFade(0f, interval)
            .SetLoops(-1, LoopType.Yoyo) // Yoyo 模式让动画往返播放
            .SetEase(Ease.Linear);

        void StopBlinking()
        {
            onClick.RemoveListener(StopBlinking);
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
        }

        onClick.AddListener(StopBlinking);
    }
}