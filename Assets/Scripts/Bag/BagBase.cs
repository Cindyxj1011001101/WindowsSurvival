using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BagBase : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // 格子预制体
    [SerializeField] private GridLayoutGroup slotLayout; // 格子布局

    [SerializeField] private Button organizeButton; // 整理背包按钮

    protected List<CardSlot> slots = new();

    public List<CardSlot> Slots => slots;

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
    public virtual bool CanAddCard(Card card)
    {
        foreach (CardSlot slot in slots)
        {
            if (slot.CanAddCard(card)) return true;
        }

        return false;
    }

    /// <summary>
    /// 获取放有同类卡牌的slot
    /// </summary>
    /// <param name="ascending">true: 按照堆叠数量升序，false: 按照堆叠数量降序</param>
    /// <returns></returns>
    protected List<CardSlot> GetSlotsContainingSimilarCard(string cardName, bool ascending = true)
    {
        List<CardSlot> result = new();
        foreach (CardSlot slot in slots)
        {
            if (slot.ContainsSimilarCard(cardName))
                result.Add(slot);
        }
        result.Sort((a, b) => ascending ? a.StackCount - b.StackCount : b.StackCount - a.StackCount);
        return result;
    }

    /// <summary>
    /// 添加一张卡牌
    /// </summary>
    /// <param name="card"></param>
    public virtual void AddCard(Card card)
    {
        // 尝试堆叠同类卡牌
        foreach (var slot in GetSlotsContainingSimilarCard(card.cardName))
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
    /// 根据名称查找卡牌
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public Card FindCardOfName(string cardName)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;

            if (slot.ContainsSimilarCard(cardName)) return slot.PeekCard();
        }

        return null;
    }

    /// <summary>
    /// 根据工具类型查找卡牌
    /// </summary>
    /// <param name="toolTypes"></param>
    /// <returns></returns>
    public Card FindCardOfToolTypes(List<ToolType> toolTypes)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;

            var card = slot.PeekCard();
            if (card.TryGetComponent<ToolComponent>(out var component))
            {
                if (toolTypes.Contains(component.toolType)) return card;
            }
        }

        return null;
    }

    public Card FindCardOfToolType(ToolType toolType)
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty) continue;

            var card = slot.PeekCard();
            if (card.TryGetComponent<ToolComponent>(out var component))
            {
                if (toolType == component.toolType) return card;
            }
        }

        return null;
    }

    /// <summary>
    /// 移除指定类型的指定数量的卡牌
    /// </summary>
    /// <param name="cardData">卡牌基础数据</param>
    /// <param name="amount">要移除的数量 (必须是正整数)</param>
    /// <param name="dontRemoveAnyIfNotAdequate">当背包的卡牌不够移除时，true: 什么也不移除，false: 尽可能多地移除卡牌</param>
    /// <returns>成功移除的卡牌的数量</returns>
    /// <exception cref="ArgumentException">amount必须是正整数</exception>
    public int RemoveCards(string cardName, int amount, bool dontRemoveAnyIfNotAdequate = true)
    {
        if (amount <= 0) throw new ArgumentException("要移除的卡牌数量必须是正整数。");

        // 找到所有同类型的卡牌slot
        // 并且将这些slot按照stackCount从大到小排序
        var slots = GetSlotsContainingSimilarCard(cardName, false);
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
    public int GetTotalCountOfSpecificCard(string cardName)
    {
        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (slot.ContainsSimilarCard(cardName))
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

    ///// <summary>
    ///// 使卡牌紧凑排列
    ///// </summary>
    //public void CompactCards()
    //{
    //    if(SoundManager.Instance != null)
    //        SoundManager.Instance.PlaySound("万能泡泡音",true);
    //    // 记录需要移动的卡牌和它们的原始位置
    //    List<(CardSlot slot, int index)> nonEmptySlots = new();

    //    // 第一次遍历：收集所有非空槽位信息
    //    for (int i = 0; i < slots.Count; i++)
    //    {
    //        if (!slots[i].IsEmpty)
    //        {
    //            nonEmptySlots.Add((slots[i], i));
    //        }
    //    }

    //    // 第二次遍历：从前往后填充空位
    //    int currentPosition = 0;
    //    foreach (var (slot, index) in nonEmptySlots)
    //    {
    //        // 如果当前卡牌已经在正确位置，跳过
    //        if (currentPosition == index)
    //        {
    //            currentPosition++;
    //            continue;
    //        }

    //        // 移动卡牌到当前位置
    //        MoveCardToPosition(slot, currentPosition);
    //        currentPosition++;
    //    }
    //}

    ///// <summary>
    ///// 将卡牌从一个槽位移动到另一个槽位
    ///// </summary>
    //private void MoveCardToPosition(CardSlot sourceSlot, int targetIndex)
    //{
    //    // 如果目标位置就是当前位置，不做任何操作
    //    int sourceIndex = slots.IndexOf(sourceSlot);
    //    if (sourceIndex == targetIndex) return;

    //    CardSlot targetSlot = slots[targetIndex];

    //    while (!sourceSlot.IsEmpty && targetSlot.CanAddCard(sourceSlot.PeekCard()))
    //    {
    //        targetSlot.AddCard(sourceSlot.RemoveCard());
    //    }

    //    if (SoundManager.Instance != null)
    //        SoundManager.Instance.PlaySound("整理",true);
    //}

    /// <summary>
    /// 使卡牌紧凑排列并尽可能堆叠
    /// </summary>
    public void CompactCards()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("万能泡泡音", true);

        // 第一步：尝试合并相同类型的卡牌
        TryMergeSimilarCards();

        // 第二步：紧凑排列剩余的卡牌
        CompactRemainingCards();
    }

    /// <summary>
    /// 尝试合并相同类型的卡牌
    /// </summary>
    private void TryMergeSimilarCards()
    {
        // 创建一个字典来记录每种卡牌的槽位
        Dictionary<string, List<CardSlot>> cardSlotsDict = new();

        // 收集所有非空槽位并按卡牌类型分组
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                string cardName = slot.PeekCard().cardName;
                if (!cardSlotsDict.ContainsKey(cardName))
                {
                    cardSlotsDict[cardName] = new List<CardSlot>();
                }
                cardSlotsDict[cardName].Add(slot);
            }
        }

        // 对每种卡牌类型尝试合并
        foreach (var kvp in cardSlotsDict)
        {
            var slotsOfType = kvp.Value;
            if (slotsOfType.Count > 1)
            {
                // 按堆叠数量升序排列，优先合并到堆叠较少的槽位
                slotsOfType.Sort((a, b) => a.StackCount - b.StackCount);

                // 从第二个槽位开始，尝试将卡牌合并到前面的槽位
                for (int i = 1; i < slotsOfType.Count; i++)
                {
                    var sourceSlot = slotsOfType[i];
                    for (int j = 0; j < i; j++)
                    {
                        var targetSlot = slotsOfType[j];

                        // 尝试将卡牌从sourceSlot移动到targetSlot
                        while (!sourceSlot.IsEmpty && targetSlot.CanAddCard(sourceSlot.PeekCard()))
                        {
                            targetSlot.AddCard(sourceSlot.RemoveCard());
                        }

                        // 如果sourceSlot已经空了，可以提前退出
                        if (sourceSlot.IsEmpty) break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 紧凑排列剩余的卡牌
    /// </summary>
    private void CompactRemainingCards()
    {
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

        // 如果目标槽位为空，直接移动整个堆叠
        if (targetSlot.IsEmpty)
        {
            while (!sourceSlot.IsEmpty)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
        else
        {
            // 否则尝试将卡牌添加到目标槽位
            while (!sourceSlot.IsEmpty && targetSlot.CanAddCard(sourceSlot.PeekCard()))
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("整理", true);
    }
}