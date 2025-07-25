using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class BottomBarShortcut : HoverableButton
{
    public CanvasGroup tipCanvas;

    private float showTipDelay = 0.4f; // 显示提示的延迟时间

    private float tipTimer = 0f; // 用于计时的变量

    private bool pointerEntered = false; // 标记鼠标是否进入

    protected override void Awake()
    {
        base.Awake();
        tipCanvas.alpha = 0f;
        tipCanvas.gameObject.SetActive(false); // 确保初始状态下提示不可见
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        pointerEntered = true;
        tipTimer = 0f; // 重置计时器
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        pointerEntered = false;
        HideTipTween();
        base.OnPointerExit(eventData);
    }

    private void Update()
    {
        if (pointerEntered)
        {
            tipTimer += Time.deltaTime; // 增加计时器
            if (tipTimer >= showTipDelay && !tipCanvas.gameObject.activeSelf)
            {
                ShowTipTween(); // 显示提示
            }
        }
    }

    private void ShowTipTween()
    {
        tipCanvas.DOKill(); // 停止所有正在进行的动画
        tipCanvas.gameObject.SetActive(true);
        tipCanvas.DOFade(1, fadeDuration).SetEase(Ease.OutQuad);
    }

    private void HideTipTween()
    {
        tipCanvas.DOKill(); // 停止所有正在进行的动画
        tipCanvas.DOFade(0, fadeDuration).SetEase(Ease.InQuad).OnComplete(() => tipCanvas.gameObject.SetActive(false));
    }
}