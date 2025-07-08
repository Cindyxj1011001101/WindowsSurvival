using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private CardSlot originalSlot;
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalSlot = GetComponentInParent<CardSlot>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!originalSlot.CanDrag) return;

        originalParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!originalSlot.CanDrag) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!originalSlot.CanDrag) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var currentObject = eventData.pointerCurrentRaycast.gameObject;

        BagBase targetBag = currentObject.GetComponentInParent<BagBase>();
        BagBase originalBag = originalSlot.GetComponentInParent<BagBase>();

        bool cardMoved = false;

        if (targetBag != null)
        {
            // 同背包放置
            if (targetBag == originalBag)
            {
                CardSlot targetSlot = currentObject.GetComponentInParent<CardSlot>();
                if (targetSlot != null && targetSlot != originalSlot)
                {
                    cardMoved = TryPlaceCardInSameBag(targetSlot);
                }
            }
            // 跨背包放置
            else
            {
                cardMoved = TryPlaceCardInDifferentBag(originalBag, targetBag);
            }
        }

        // 如果卡牌移动了，执行紧凑排列
        //if (cardMoved)
        //{
        //    originalBag.CompactCards();
        //    if (targetBag != originalBag)
        //    {
        //        targetBag.CompactCards();
        //    }
        //}

        Home();
    }

    private bool TryPlaceCardInSameBag(CardSlot targetSlot)
    {
        // 如果目标格子为空
        if (targetSlot.IsEmpty)
        {
            // 直接全部放到目标格子中
            while (originalSlot.StackCount > 0)
            {
                targetSlot.AddCard(originalSlot.RemoveCard());
            }
            return true;
        }
        // 如果目标格子有相同卡牌
        else if (targetSlot.ContainsSimilarCard(originalSlot.Card.cardName))
        {
            // 往目标格子里尽可能放更多
            bool movedAny = false;
            while (originalSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(originalSlot.RemoveCard());
                movedAny = true;
            }
            return movedAny;
        }
        return false;
    }

    private bool TryPlaceCardInDifferentBag(BagBase originalBag, BagBase targetBag)
    {
        bool movedAny = false;
        while (originalSlot.StackCount > 0 && targetBag.CanAddCard(originalSlot.PeekCard()))
        {
            targetBag.AddCard(originalBag.RemoveCard(originalSlot));
            movedAny = true;
        }
        return movedAny;
    }

    /// <summary>
    /// 将原格子归位
    /// </summary>
    private void Home()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}