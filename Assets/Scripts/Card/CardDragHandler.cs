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
        //if (!sourceSlot.CanDrag) return;

        sourceParent = transform.parent;
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (!sourceSlot.CanDrag) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if (!sourceSlot.CanDrag) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        var currentObject = eventData.pointerCurrentRaycast.gameObject;

        BagBase targetBag = currentObject.GetComponentInParent<BagBase>();
        BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();
        //BagWindo targetBag = currentObject.GetComponentInParent<BagWindow>();
        //BagWindow sourceBag = sourceSlot.GetComponentInParent<BagWindow>();

        bool cardMoved = false;

        if (targetBag != null)
        {
            // 同背包放置
            if (targetBag == sourceBag)
            {
                CardSlot targetSlot = currentObject.GetComponentInParent<CardSlot>();
                if (targetSlot != null && targetSlot != sourceSlot)
                {
                    cardMoved = TryPlaceCardInSameBag(targetSlot);
                }
            }
            // 跨背包放置
            else if (sourceSlot.CanDragOverBag)
            {
                cardMoved = TryPlaceCardInDifferentBag(sourceBag, targetBag);
            }
        }

        // 如果卡牌移动了，执行紧凑排列
        //if (cardMoved)
        //{
        //    sourceBag.CompactCards();
        //    if (targetBag != sourceBag)
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
            while (sourceSlot.StackCount > 0)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
            return true;
        }
        // 如果目标格子有相同卡牌
        else if (targetSlot.ContainsSimilarCard(sourceSlot.CardData.cardName))
        {
            // 往目标格子里尽可能放更多
            bool movedAny = false;
            while (sourceSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
                movedAny = true;
            }
            return movedAny;
        }
        return false;
    }

    //private bool TryPlaceCardInDifferentBag(BagBase sourceBag, BagBase targetBag)
    private bool TryPlaceCardInDifferentBag(BagBase sourceBag, BagBase targetBag)
    {
        bool movedAny = false;
        while (sourceSlot.StackCount > 0 && targetBag.CanAddCard(sourceSlot.PeekCard()))
        {
            targetBag.AddCard(sourceBag.RemoveCard(sourceSlot));
            movedAny = true;
        }
        return movedAny;
    }

    /// <summary>
    /// 将原格子归位
    /// </summary>
    private void Home()
    {
        transform.SetParent(sourceParent);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}