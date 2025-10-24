using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameEventTip : HoverableButton
{
    public Text eventNameText;
    public Image frameImage;
    public RectTransform background;

    private float showHideTransition = .2f;
    private Sequence seq;

    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.RemoveAllListeners();
        // background 归位

    }

    public void SetGameEvent(Sprite icon, Color color, string eventName)
    {
        normalImage.sprite = icon;
        eventNameText.text = eventName;
        normalImage.color = eventNameText.color = frameImage.color = color;
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);


    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);


    }

    public void Show()
    {

    }

    public void Hide()
    {
        ObjectBufferPool.Instance.Restore(gameObject);
    }
}