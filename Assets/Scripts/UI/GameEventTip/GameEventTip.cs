using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameEventTip : HoverableButton
{
    public Text eventNameText;
    public Image frameImage;
    public RectTransform body;

    private float showHideTransition = .5f;
    private float moveTransition = .25f;

    private float upDistance = 54f;
    private Vector3 upPosition;
    private Vector3 spawnPosition;
    private Vector3 defaultPosition;

    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.RemoveAllListeners();
        defaultPosition = Vector3.zero;
        upPosition = Vector3.up * upDistance;
        spawnPosition = -Vector3.up * body.sizeDelta.y / 2;
        body.localPosition = spawnPosition;
        canvasGroup.blocksRaycasts = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        body.DOKill();
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

        // body 上移
        body.DOLocalMove(upPosition, moveTransition);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        // body 下移
        body.DOLocalMove(defaultPosition, moveTransition);
    }

    public void Show()
    {
        body.DOKill();
        body.DOLocalMove(defaultPosition, showHideTransition).SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = true;
            });
    }

    public void Hide()
    {
        body.DOKill();
        body.DOLocalMove(spawnPosition, showHideTransition)
            .OnComplete(() =>
            {
                ObjectBufferPool.Instance.Restore(gameObject);
            });
    }
}