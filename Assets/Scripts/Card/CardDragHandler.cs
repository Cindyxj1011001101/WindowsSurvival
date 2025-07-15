using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform sourceParent;
    private CardSlot sourceSlot;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        sourceSlot = GetComponentInParent<CardSlot>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        sourceParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var currentObject = eventData.pointerCurrentRaycast.gameObject;

        BagBase targetBag = currentObject.GetComponentInParent<BagBase>();
        BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();

        if (targetBag != null)
        {
            // 同背包放置
            if (targetBag == sourceBag)
            {
                CardSlot targetSlot = currentObject.GetComponentInParent<CardSlot>();
                if (targetSlot != null && targetSlot != sourceSlot)
                {
                    PlaceCardInSameBag(targetSlot);
                }
            }
            // 跨背包放置
            else if (sourceSlot.CanDragOverBag)
            {
                PlaceCardInDifferentBag(targetBag);
            }
        }

        Home();
    }

    private void PlaceCardInSameBag(CardSlot targetSlot)
    {
        // 如果目标格子为空
        if (targetSlot.IsEmpty)
        {
            // 直接全部放到目标格子中
            while (sourceSlot.StackCount > 0)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
        // 如果目标格子有相同卡牌
        else if (targetSlot.ContainsSimilarCard(sourceSlot.CardData))
        {
            // 往目标格子里尽可能放更多
            while (sourceSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
    }

    private void PlaceCardInDifferentBag(BagBase targetBag)
    {
        while (sourceSlot.StackCount > 0 && targetBag.CanAddCard(sourceSlot.PeekCard()))
        {
            targetBag.AddCard(sourceSlot.RemoveCard());
        }
    }

    /// <summary>
    /// 将原格子归位
    /// </summary>
    private void Home()
    {
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("放置卡牌",true);
        transform.SetParent(sourceParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}