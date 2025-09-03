using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    // 动画参数配置
    private float moveDuration = 0.3f;
    private float returnDuration = 0.3f;

    private CardSlot sourceSlot;
    private Canvas canvas;

    private CardSlot cursorSlot;
    private int pickedCount;

    private Vector3 dragEndPosition;

    private void Awake()
    {
        sourceSlot = GetComponentInParent<CardSlot>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        MouseManager.Instance.StartDragging();

        // 在鼠标位置创建图标
        var screenPosition = MFXUtility.ScreenPointToLocalPointInRectangle(eventData.position);
        cursorSlot = MFXUtility.CreateSlot(screenPosition);

        if (eventData.button == PointerEventData.InputButton.Left)
            // 左键拖拽
            pickedCount = sourceSlot.StackNum;
        else
            // 右键拖拽
            pickedCount = 1;

        var card = sourceSlot.PeekCard();
        // 更新源卡槽显示
        sourceSlot.DisplayCard(card, sourceSlot.StackNum - pickedCount);
        cursorSlot.DisplayCard(card, pickedCount);

        // 让sourceSlot暂时不要刷新显示
        sourceSlot.DontRefresh = true;

        SoundManager.Instance.PlaySound("拿起卡牌", true);

        EventManager.Instance.TriggerEvent(EventType.PickUpCard, card);
    }

    public void OnDrag(PointerEventData eventData)
    {
        (cursorSlot.transform as RectTransform).anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        MouseManager.Instance.EndDragging();

        dragEndPosition = MFXUtility.ScreenPointToLocalPointInRectangle(eventData.position);
        ObjectBufferPool.Instance.Restore(cursorSlot.gameObject);

        var currentObject = eventData.pointerCurrentRaycast.gameObject;
        if (currentObject == null)
        {
            AnimateCardReturn(pickedCount);
            EventManager.Instance.TriggerEvent(EventType.PutDownCard);
            return;
        }

        // 处理快捷交互
        var targetSlot = currentObject.GetComponentInParent<CardSlot>();
        if (targetSlot != null && targetSlot.Interactable)
        {
            HandleQuickInteract(targetSlot);
            EventManager.Instance.TriggerEvent(EventType.PutDownCard);
            return;
        }

        BagWindow targetWindow = currentObject.GetComponentInParent<BagWindow>();
        BagWindow sourceWindow = sourceSlot.GetComponentInParent<BagWindow>();

        // 能够放置
        if (targetWindow != null && targetWindow.Bag != null)
        {
            // 同背包放置
            if (targetWindow == sourceWindow)
            {
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
            else if (targetWindow.Bag is not EnvironmentBag && !sourceSlot.PeekCard().Moveable)
            {
                AnimateCardReturn(pickedCount, "不能移动该卡牌");
            }
            else if (sourceWindow.Bag is InnerBag s && !s.AllowRemove)
            {
                AnimateCardReturn(pickedCount, string.IsNullOrEmpty(s.NotAllowRemoveReason) ? "不能取出卡牌" : s.NotAllowRemoveReason);
            }
            else if (targetWindow.Bag is InnerBag t && !t.AllowAdd)
            {
                AnimateCardReturn(pickedCount, string.IsNullOrEmpty(t.NotAllowAddReason) ? "不能放入卡牌" : t.NotAllowAddReason);
            }
            else
            {
                PlaceCardInDifferentBag(targetWindow.Bag, pickedCount, dragEndPosition);
            }
        }
        // 不能放置
        else
        {
            AnimateCardReturn(pickedCount);
        }

        EventManager.Instance.TriggerEvent(EventType.PutDownCard);
    }

    private void HandleQuickInteract(CardSlot targetSlot)
    {
        var left = sourceSlot.StackNum - pickedCount;
        targetSlot.PeekCard().QuickIneract(sourceSlot.Cards, pickedCount, out var tip);
        targetSlot.ShowTip(tip);
        var toReturn = sourceSlot.StackNum - left; // toReturn一定>=0
        if (toReturn > 0)
            AnimateCardReturn(toReturn);
        else
            sourceSlot.DontRefresh = false;
    }

    /// <summary>
    /// 右键点击在背包间移动一张卡牌
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        int pickedCount;
        // 右键单击
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 移动一张
            pickedCount = 1;
        }
        // 左键单击
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // shift + 左键
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // 移动全部
                pickedCount = sourceSlot.StackNum;

                // 如果是装备卡
                var card = sourceSlot.PeekCard();
                // 穿脱装备
                if (card.TryGetComponent<EquipmentComponent>(out var component))
                {
                    // 穿上装备
                    if (GameManager.Instance.CanEquip(card, out string tip))
                        GameManager.Instance.Equip(card);
                    // 脱下装备
                    else if (component.isEquipped)
                        GameManager.Instance.Unequip(card);
                    else
                        sourceSlot.ShowTip(tip);

                    return;
                }
            }
            // 仅左键
            else
            {
                // 显示详情
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Display(sourceSlot.Cards);
                return;
            }
        }
        else return;

        // 不可移动的卡牌
        if (!sourceSlot.PeekCard().Moveable)
        {
            sourceSlot.ShowTip("不能移动该卡牌");
            return;
        }

        BagWindow sourceBag = sourceSlot.GetComponentInParent<BagWindow>();
        Bag targetBag = null;

        if (sourceBag is PlayerBagWindow && WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
        {
            targetBag = GameManager.Instance.CurEnvironmentBag;
        }
        else if (sourceBag is EnvironmentBagWindow && WindowsManager.Instance.IsWindowOpen("PlayerBag"))
        {
            targetBag = GameManager.Instance.PlayerBag;
        }

        if (targetBag != null)
        {
            PlaceCardInDifferentBag(targetBag, pickedCount, sourceSlot.transform.position, false);
            sourceSlot.RefreshDisplay();
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
        MFXUtility.MoveCard(
            card,
            count,
            startPos,
            endPos,
            moveDuration,
            onStart: null,
            onComplete: () =>
            {
                placementAction.Invoke();
                sourceSlot.DontRefresh = false;
            }
        );
    }

    /// <summary>
    /// 播放卡牌返回动画
    /// </summary>
    private void AnimateCardReturn(int count, string tip = "")
    {
        MFXUtility.MoveCard(
                sourceSlot.PeekCard(),
                count,
                dragEndPosition,
                sourceSlot.transform.position,
                returnDuration,
                onStart: null,
                onComplete: () =>
                {
                    // 刷新源卡槽显示
                    sourceSlot.DontRefresh = false;
                    // 显示提示
                    sourceSlot.ShowTip(tip);
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
        List<Card> movedCard = new();
        for (int i = 0; i < count; i++)
        {
            if (!targetSlot.CanAddCard(sourceSlot.PeekCard())) break;
            var toMove = sourceSlot.RemoveCard();
            targetSlot.AddCard(toMove);
            movedCard.Add(toMove);
        }

        if (movedCard.Count > 0)
        {
            AnimateCardPlacement(
                movedCard[0],
                () =>
                {
                    // 再刷新显示
                    targetSlot.RefreshDisplay();
                },
                dragEndPosition,
                targetSlot.transform.position,
                movedCard.Count
            );
        }

        int leftCount = count - movedCard.Count;
        if (leftCount > 0)
            AnimateCardReturn(leftCount);
    }

    /// <summary>
    /// 跨背包放置
    /// </summary>
    /// <param name="targetBag"></param>
    /// <param name="count"></param>
    /// <param name="startPos"></param>
    private void PlaceCardInDifferentBag(Bag targetBag, int count, Vector3 startPos, bool needReturnAnim = true)
    {
        string tip = string.Empty;
        List<Card> movedCard = new();
        for (int i = 0; i < count; i++)
        {
            if (!targetBag.CanAddCard(sourceSlot.PeekCard(), out tip)) break;
            var toMove = sourceSlot.RemoveCard();
            targetBag.AddCard(toMove);
            movedCard.Add(toMove);
        }

        if (movedCard.Count > 0)
        {
            // 将移动了的卡牌按照slot进行分组
            var groups = movedCard.GroupBy(c => c.Slot);

            foreach (var group in groups)
            {
                AnimateCardPlacement(
                    movedCard[0],
                    () =>
                    {
                        // 再刷新显示
                        group.Key.RefreshDisplay();
                    },
                    startPos,
                    group.Key.transform.position,
                    group.Count()
                );
            }
        }

        int leftCount = count - movedCard.Count;
        if (leftCount > 0 && needReturnAnim)
            AnimateCardReturn(leftCount, tip);
    }
}