using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 动画参数配置
    private float moveDuration = 0.2f;
    private float returnDuration = 0.2f;

    private CardSlot sourceSlot;
    private Canvas canvas;

    private CardSlot cursorSlot;
    private int pickedCount;

    Vector3 dragEndPosition;

    private void Awake()
    {
        sourceSlot = GetComponentInParent<CardSlot>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 在鼠标位置创建图标
        cursorSlot = CardMoveTween.CreateSlot(eventData.position);

        if (eventData.button == PointerEventData.InputButton.Left)
            // 左键拖拽
            pickedCount = sourceSlot.StackCount;
        else
            // 右键拖拽
            pickedCount = 1;

        // 更新源卡槽显示
        sourceSlot.DisplayCard(sourceSlot.PeekCard(), sourceSlot.StackCount - pickedCount);
        cursorSlot.DisplayCard(sourceSlot.PeekCard(), pickedCount);
    }

    public void OnDrag(PointerEventData eventData)
    {
        (cursorSlot.transform as RectTransform).anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragEndPosition = cursorSlot.transform.position;
        Destroy(cursorSlot.gameObject);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("放置卡牌", true);
        }

        var currentObject = eventData.pointerCurrentRaycast.gameObject;
        if (currentObject == null) return;

        BagBase targetBag = currentObject.GetComponentInParent<BagBase>();
        BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();

        // 能够放置
        if (targetBag != null)
        {
            // 同背包放置
            if (targetBag == sourceBag)
            {
                CardSlot targetSlot = currentObject.GetComponentInParent<CardSlot>();
                // 放在同背包的不同格子里
                if (targetSlot != null && targetSlot != sourceSlot)
                {
                    PlaceCardInSameBag(targetSlot, pickedCount);
                }
                // 放在同背包的相同格子里
                else
                {
                    AnimateCardReturn(pickedCount);
                }
            }
            // 跨背包放置
            else if (sourceSlot.PeekCard().moveable)
            {
                PlaceCardInDifferentBag(targetBag, pickedCount, dragEndPosition);
            }
            // 卡牌的moveable为false
            else
            {
                AnimateCardReturn(pickedCount);
            }
        }
        // 不能放置
        else
        {
            AnimateCardReturn(pickedCount);
        }
    }

    /// <summary>
    /// 右键点击在背包间移动一张卡牌
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && sourceSlot.PeekCard().moveable)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("放置卡牌", true);
            }

            BagBase sourceBag = sourceSlot.GetComponentInParent<BagBase>();
            BagBase targetBag = null;

            if (sourceBag is PlayerBag && WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
            {
                targetBag = GameManager.Instance.CurEnvironmentBag;
            }
            else if (sourceBag is EnvironmentBag && WindowsManager.Instance.IsWindowOpen("PlayerBag"))
            {
                targetBag = GameManager.Instance.PlayerBag;
            }

            if (targetBag != null && targetBag.CanAddCard(sourceSlot.PeekCard()))
            {
                sourceSlot.DisplayCard(sourceSlot.PeekCard(), sourceSlot.StackCount - 1);
                PlaceCardInDifferentBag(targetBag, 1, sourceSlot.transform.position);
            }
        }
    }

    /// <summary>
    /// 放置卡牌动画
    /// </summary>
    /// <param name="placementAction"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="count"></param>
    private void AnimateCardPlacement(Card card, UnityAction placementAction, Vector3 startPos, Vector3 endPos, int count)
    {
        CardMoveTween.MoveCard(
            card,
            count,
            startPos,
            endPos,
            moveDuration,
            onStart: null,
            onComplete: () =>
            {
                placementAction.Invoke();
                sourceSlot.RefreshCurrentDisplay();
            }
        );
    }

    /// <summary>
    /// 播放卡牌返回动画
    /// </summary>
    private void AnimateCardReturn(int count)
    {
        CardMoveTween.MoveCard(
                sourceSlot.PeekCard(),
                count,
                dragEndPosition,
                sourceSlot.transform.position,
                returnDuration,
                onStart: null,
                onComplete: () =>
                {
                    // 刷新源卡槽显示
                    sourceSlot.RefreshCurrentDisplay();
                }
            );
    }

    /// <summary>
    /// 同背包放置
    /// </summary>
    /// <param name="targetSlot"></param>
    /// <param name="count"></param>
    private void PlaceCardInSameBag(CardSlot targetSlot, int count)
    {
        var card = sourceSlot.PeekCard();
        // 得到targetSlot的剩余容量
        int remainingCapacity = targetSlot.GetRemainingCapacity(card);

        // 得到真正移动的数量
        int moveCount = Mathf.Min(remainingCapacity, count);
        if (moveCount > 0)
        {
            // 动画结束时将卡牌转移到targetSlot
            AnimateCardPlacement(
                card,
                () =>
                {
                    sourceSlot.TransferCardsTo(targetSlot, moveCount);
                },
                dragEndPosition,
                targetSlot.transform.position,
                moveCount
            );
        }
        
        // 记录剩余数量
        int leftCount = count - moveCount;
        if (leftCount > 0)
        {
            // 剩余卡牌回到原位
            AnimateCardReturn(leftCount);
        }
    }

    /// <summary>
    /// 跨背包放置
    /// </summary>
    /// <param name="targetBag"></param>
    /// <param name="count"></param>
    /// <param name="startPos"></param>
    private void PlaceCardInDifferentBag(BagBase targetBag, int count, Vector3 startPos)
    {
        var card = sourceSlot.PeekCard();
        // 得到targetBag中所有可以放置卡牌的格子以及可以放置的数量
        List<(CardSlot, int)> list = targetBag.GetSlotsCanAddCard(card, count);

        int leftCount = count; // 剩余待移动卡牌的数量

        // 将卡牌放入目标背包的目标格子里
        foreach (var (targetSlot, moveCount) in list)
        {
            // 这里targetBag.GetSlotsCanAddCard方法确保leftCount不会是负数
            leftCount -= moveCount;
            AnimateCardPlacement(
                card,
                () =>
                {
                    sourceSlot.TransferCardsTo(targetSlot, moveCount);
                },
                startPos,
                targetSlot.transform.position,
                moveCount
            );
        }

        if (leftCount > 0)
        {
            // 剩余卡牌回到原位
            AnimateCardReturn(leftCount);
        }
    }
}