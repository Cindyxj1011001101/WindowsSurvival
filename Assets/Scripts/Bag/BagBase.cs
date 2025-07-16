using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BagBase : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // 格子预制体
    [SerializeField] private GridLayoutGroup slotLayout; // 格子布局

    protected List<CardSlot> slots = new();

    public List<CardSlot> Slots => slots;

    [SerializeField] private Button organizeButton; // 整理背包按钮

    protected int UsedSlotsCount // 放有卡牌的格子的数量
    {
        get
        {
            int count = 0;
            foreach (CardSlot slot in slots)
            {
                if (!slot.IsEmpty) count++;
            }
            return count;
        }
    }
    protected int SlotsCount => slots.Count; // 格子总数
    protected bool IsBagFull => UsedSlotsCount == SlotsCount; // 背包是否已满

    private void OnEnable()
    {
        if (organizeButton != null)
            organizeButton.onClick.AddListener(CompactCards);
    }

    private void OnDisable()
    {
        if (organizeButton != null)
            organizeButton.onClick.RemoveListener(CompactCards);
    }

    protected virtual void Start()
    {
        Init();
    }

    protected abstract void Init();

    protected virtual void InitBag(BagRuntimeData runtimeData)
    {
        slots = new();
        AddSlot(runtimeData.cardSlotsRuntimeData.Count);
        CardSlotRuntimeData cardSlotData;
        for (int i = 0; i < runtimeData.cardSlotsRuntimeData.Count; i++)
        {
            cardSlotData = runtimeData.cardSlotsRuntimeData[i];
            slots[i].InitFromRuntimeData(cardSlotData);
        }
    }

    /// <summary>
    /// 添加指定数量的格子
    /// </summary>
    /// <param name="amount"></param>
    public void AddSlot(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotLayout.transform);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slot.SetBag(this);
            slot.ClearSlot();
            slots.Add(slot);
        }

        // 添加格子后更新容器高度
        MonoUtility.UpdateContainerHeight(slotLayout, slots.Count);
    }

    /// <summary>
    /// 能否添加新的卡牌
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public virtual bool CanAddCard(CardInstance card)
    {
        //// 如果背包未满，则可以添加
        //if (!IsBagFull) return true;

        //// 背包已满
        //// 则能否添加取决于背包中是否有同类卡牌并且没有达到堆叠上限
        //var slots = GetSlotsContainingSimilarCard(card.CardData);
        //foreach (CardSlot slot in slots)
        //{
        //    if (slot.CanAddCard()) return true;
        //}

        foreach (CardSlot slot in slots)
        {
            if (slot.CanAddCard(card)) return true;
        }

        return false;
    }

    /// <summary>
    /// 获取放有同类卡牌的slot
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <param name="ascending">true: 按照堆叠数量升序，false: 按照堆叠数量降序</param>
    /// <returns></returns>
    protected List<CardSlot> GetSlotsContainingSimilarCard(CardData cardData, bool ascending = true)
    {
        List<CardSlot> result = new();
        foreach (CardSlot slot in slots)
        {
            if (slot.ContainsSimilarCard(cardData))
                result.Add(slot);
        }
        result.Sort((a, b) => ascending ? a.StackCount - b.StackCount : b.StackCount - a.StackCount);
        return result;
    }

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(CardInstance card)
    {
        //if (!CanAddCard(card)) return;

        // 尝试堆叠同类卡牌
        foreach (var slot in GetSlotsContainingSimilarCard(card.CardData))
        {
            if (slot.CanAddCard(card))
            {
                slot.AddCard(card);
                return;
            }
        }

        // 无法堆叠则放在新的slot中
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.AddCard(card);
                return;
            }
        }
    }


    /// <summary>
    /// 尝试从背包中取出一张牌
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public CardSlot TryGetCardByCondition(ConditionData condition)
    {
        switch (condition)
        {
            case TagCondition tagCondition:
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty) continue;
                    if (slot.CardData.CardTagList.Contains(tagCondition.ConditionTag))
                    {
                        return slot;
                    }
                }
                return null;
            case TypeCondition typeCondition:
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty) continue;
                    if (slot.CardData.cardType == typeCondition.ConditonCardType)
                    {
                        return slot;
                    }
                }
                return null;
            case CardCondition cardCondition:
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty) continue;
                    if (slot.CardData == cardCondition.ConditionCard)
                    {
                        return slot;
                    }
                }
                return null;
            default:
                return null;

        }
    }

    /// <summary>
    /// 移除指定类型的指定数量的卡牌
    /// </summary>
    /// <param name="cardData">卡牌基础数据</param>
    /// <param name="amount">要移除的数量 (必须是正整数)</param>
    /// <param name="dontRemoveAnyIfNotAdequate">当背包的卡牌不够移除时，true: 什么也不移除，false: 尽可能多地移除卡牌</param>
    /// <returns>成功移除的卡牌的数量</returns>
    /// <exception cref="ArgumentException">amount必须是正整数</exception>
    public int RemoveCards(CardData cardData, int amount, bool dontRemoveAnyIfNotAdequate = true)
    {
        if (amount <= 0) throw new ArgumentException("要移除的卡牌数量必须是正整数。");

        // 找到所有同类型的卡牌slot
        // 并且将这些slot按照stackCount从大到小排序
        var slots = GetSlotsContainingSimilarCard(cardData, false);
        // 统计总数
        int totalCount = 0;
        foreach (var slot in slots)
        {
            totalCount += slot.StackCount;
        }

        // 如果总数小于需要移除的数量
        if (totalCount < amount)
        {
            // 如果当总数不足时不要移除
            // 则直接返回，并且移除数量为0
            if (dontRemoveAnyIfNotAdequate) return 0;

            // 否则将剩余卡牌全部移除
            foreach (var slot in slots)
            {
                //slot.RemoveAllCards();
                slot.ClearSlot();
            }
            return totalCount;
        }

        // 如果总数不少于需要移除的数量
        // 则全部移除
        int leftAmount = amount; // 剩余要移除的数量
        foreach (var slot in slots)
        {
            // 剩余要移除的数量大于当前slot的总数
            // 全部从slot中移除
            if (leftAmount > slot.StackCount)
            {
                //slot.RemoveAllCards();
                slot.ClearSlot();
                leftAmount -= slot.StackCount;
            }
            // 剩余要移除的数量小于等于当前slot的总数
            // 则将这些卡牌移除，退出循环
            else
            {
                slot.RemoveCards(leftAmount);
                leftAmount = 0;
                break;
            }
        }

        return amount;
    }

    /// <summary>
    /// 得到背包中指定卡牌的数量
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    public int GetTotalCountOfSpecificCard(CardData cardData)
    {
        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (slot.ContainsSimilarCard(cardData))
                totalCount += slot.StackCount;
        }

        return totalCount;
    }

    /// <summary>
    /// 清空所有卡牌格
    /// </summary>
    public virtual void Clear()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// 使卡牌紧凑排列
    /// </summary>
    public void CompactCards()
    {
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("万能泡泡音",true);
        // 记录需要移动的卡牌和它们的原始位置
        List<(CardSlot slot, int index)> nonEmptySlots = new();

        // 第一次遍历：收集所有非空槽位信息
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty)
            {
                nonEmptySlots.Add((slots[i], i));
            }
        }

        // 第二次遍历：从前往后填充空位
        int currentPosition = 0;
        foreach (var (slot, index) in nonEmptySlots)
        {
            // 如果当前卡牌已经在正确位置，跳过
            if (currentPosition == index)
            {
                currentPosition++;
                continue;
            }

            // 移动卡牌到当前位置
            MoveCardToPosition(slot, currentPosition);
            currentPosition++;
        }
    }

    /// <summary>
    /// 将卡牌从一个槽位移动到另一个槽位
    /// </summary>
    private void MoveCardToPosition(CardSlot sourceSlot, int targetIndex)
    {
        // 如果目标位置就是当前位置，不做任何操作
        int sourceIndex = slots.IndexOf(sourceSlot);
        if (sourceIndex == targetIndex) return;

        CardSlot targetSlot = slots[targetIndex];

        //// 如果目标槽位为空，直接移动整个堆叠
        //if (targetSlot.IsEmpty)
        //{
        //    while (!sourceSlot.IsEmpty)
        //    {
        //        targetSlot.AddCard(sourceSlot.RemoveCard());
        //    }
        //}
        //// 如果目标槽位有相同卡牌且可以堆叠
        //else if (targetSlot.ContainsSimilarCard(sourceSlot.CardData) && targetSlot.CanAddCard())
        //{
        //    // 尽可能多地移动卡牌到目标槽位
        //    while (sourceSlot.StackCount > 0 && targetSlot.CanAddCard())
        //    {
        //        targetSlot.AddCard(sourceSlot.RemoveCard());
        //    }
        //}

        while (!sourceSlot.IsEmpty && targetSlot.CanAddCard(sourceSlot.PeekCard()))
        {
            targetSlot.AddCard(sourceSlot.RemoveCard());
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("整理",true);
    }
}