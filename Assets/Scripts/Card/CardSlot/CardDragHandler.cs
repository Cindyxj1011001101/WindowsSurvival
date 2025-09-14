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

        // 计算拖动数量
        if (IsLeftButtonPressed(eventData))
        {
            // ctrl + 左键 = 拖动半组
            if (IsCtrlPressed())
                pickedCount = Mathf.CeilToInt((float)sourceSlot.StackNum / 2);
            // 左键拖动一组
            else
                pickedCount = sourceSlot.StackNum;
        }
        else
        {
            // 右键拖拽
            pickedCount = 1;
        }

        var card = sourceSlot.PeekCard();
        cursorSlot.DisplayCard(card, pickedCount);

        // 更新源卡槽显示
        if (sourceSlot.StackNum - pickedCount > 0)
            sourceSlot.DisplayCard(sourceSlot.Cards[pickedCount], sourceSlot.StackNum - pickedCount);
        else
            sourceSlot.Clear();

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
                PlaceCardInDifferentBag(targetWindow.Bag, ref pickedCount, dragEndPosition);
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
        targetSlot.transform.ShowTip(tip);
        var toReturn = sourceSlot.StackNum - left; // toReturn一定>=0
        if (toReturn > 0)
            AnimateCardReturn(toReturn);
        else
            sourceSlot.DontRefresh = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 左键点击
        if (IsLeftButtonPressed(eventData))
        {
            // shift + 左键 = 快速移动一组
            if (IsShiftPressed())
                HandleQuickMove(sourceSlot.StackNum);
            // ctrl + 左键 = 快速移动半组
            else if (IsCtrlPressed())
                HandleQuickMove(Mathf.CeilToInt((float)sourceSlot.StackNum / 2));
            // 打开详情
            else
                (WindowsManager.Instance.OpenWindow("Details") as DetailsWindow).Display(sourceSlot.Cards);

            return;
        }
        // 右键点击
        if (IsRightButtonPressed(eventData))
        {
            // 快速移动一个
            HandleQuickMove(1);
            return;
        }
    }

    /// <summary>
    /// 处理卡牌的快速移动
    /// </summary>
    private void HandleQuickMove(int count)
    {
        // 不可移动的卡牌
        var card = sourceSlot.PeekCard();
        if (!card.Moveable)
        {
            sourceSlot.transform.ShowTip("不能移动该卡牌");
            return;
        }

        // 对于装备卡牌，快速穿上和脱下
        if (card.TryGetComponent<EquipmentComponent>(out var equ))
        {
            if (equ.isEquipped)
                GameManager.Instance.Unequip(card);
            else if (GameManager.Instance.CanEquip(card, out string tip))
                GameManager.Instance.Equip(card);
            else
                sourceSlot.transform.ShowTip(tip);
            return;
        }

        var sourceBag = sourceSlot.GetComponentInParent<BagWindow>().Bag;
        // 从内容物中快速移出
        if (sourceBag is InnerBag)
        {
            // 先尝试移入玩家背包
            if (WindowsManager.Instance.IsWindowOpen("PlayerBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.PlayerBag, ref count, sourceSlot.transform.position, false);
                sourceSlot.RefreshDisplay();
            }

            // 再移入地点
            if (WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.CurEnvironmentBag, ref count, sourceSlot.transform.position, false);
                sourceSlot.RefreshDisplay();
            }
            return;
        }

        // 在背包和地点之间移动
        if (sourceBag is PlayerBag)
        {
            if (WindowsManager.Instance.IsWindowOpen("EnvironmentBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.CurEnvironmentBag, ref count, sourceSlot.transform.position, false);
                sourceSlot.RefreshDisplay();
            }
            return;
        }

        if (sourceBag is EnvironmentBag)
        {
            if (WindowsManager.Instance.IsWindowOpen("PlayerBag"))
            {
                PlaceCardInDifferentBag(GameManager.Instance.PlayerBag, ref count, sourceSlot.transform.position, false);
                sourceSlot.RefreshDisplay();
            }
            return;
        }
    }

    private bool IsShiftPressed()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private bool IsCtrlPressed()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    private bool IsLeftButtonPressed(PointerEventData eventData)
    {
        return eventData.button == PointerEventData.InputButton.Left;
    }

    private bool IsRightButtonPressed(PointerEventData eventData)
    {
        return eventData.button == PointerEventData.InputButton.Right;
    }

    /// <summary>
    /// 放置卡牌动画
    /// </summary>
    /// <param name="placementAction"></param>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="count"></param>
    private void AnimateCardPlacement(Card card, UnityAction placementAction, Vector3 startPos, int count)
    {
        MFXUtility.MoveCard(
            card,
            count,
            startPos,
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
                returnDuration,
                onStart: null,
                onComplete: () =>
                {
                    // 刷新源卡槽显示
                    sourceSlot.DontRefresh = false;
                    // 显示提示
                    sourceSlot.transform.ShowTip(tip);
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
    private void PlaceCardInDifferentBag(Bag targetBag, ref int count, Vector3 startPos, bool needReturnAnim = true)
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
                    group.Count()
                );
            }
        }

        int leftCount = count - movedCard.Count;
        if (leftCount > 0 && needReturnAnim)
            AnimateCardReturn(leftCount, tip);

        count = leftCount;
    }
}