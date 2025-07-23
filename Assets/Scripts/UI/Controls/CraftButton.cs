using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public  class CraftButton : HoverableButton
{
    //[SerializeField] Image childHoveredImage;
    [SerializeField] GameObject craftObject;
    [SerializeField] private Text craftText;
    [SerializeField] private Text lockText;

    //private Sequence anim;

    //protected override void Awake()
    //{
    //    base.Awake();
    //    Color color = childHoveredImage.color;
    //    color.a = 0f;
    //    childHoveredImage.color = color;
    //    childHoveredImage.gameObject.SetActive(false);
    //}

    public void DisplayButton(bool isLocked, bool canCraft)
    {
        craftText.color = Color.white;
        if (isLocked)
        {
            enabled = false;
            craftObject.SetActive(false);
            lockText.gameObject.SetActive(true);
        }
        else if (canCraft)
        {
            enabled = true;
            craftObject.SetActive(true);
            lockText.gameObject.SetActive(false);
        }
        else
        {
            enabled = false;
            craftObject.SetActive(true);
            lockText.gameObject.SetActive(false);
        }
    }

    //public override void OnPointerEnter(PointerEventData eventData)
    //{
    //    onPointerEnter?.Invoke();

    //    if (hoveredImage == null) return;

    //    if (anim != null && anim.IsActive())
    //        anim.Kill();

    //    anim = DOTween.Sequence();

    //    // 激活图像并开始淡入动画
    //    hoveredImage.gameObject.SetActive(true);
    //    childHoveredImage.gameObject.SetActive(true);

    //    anim.Join(hoveredImage.DOFade(1, fadeDuration)
    //        .SetEase(Ease.OutQuad));
    //    anim.Join(childHoveredImage.DOFade(1, fadeDuration * 0.5f)
    //        .SetEase(Ease.OutQuad));

    //    craftText.color = Color.black;
    //}

    //public override void OnPointerExit(PointerEventData eventData)
    //{
    //    onPointerEnter?.Invoke();

    //    if (hoveredImage == null) return;

    //    if (anim != null && anim.IsActive())
    //        anim.Kill();

    //    anim = DOTween.Sequence();

    //    anim.Join(hoveredImage.DOFade(0, fadeDuration)
    //        .SetEase(Ease.OutQuad)).OnComplete(() => hoveredImage.gameObject.SetActive(false)); ;
    //    anim.Join(childHoveredImage.DOFade(0, fadeDuration * 0.5f)
    //        .SetEase(Ease.OutQuad)).OnComplete(() => childHoveredImage.gameObject.SetActive(false)); ;

    //    craftText.color = Color.white;
    //}
}