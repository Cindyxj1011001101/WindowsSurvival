using System.Collections.Generic;
using UnityEngine;

public abstract class BagBase : MonoBehaviour
{
    protected GameObject slotPrefab;
    protected Transform slotContainer;
    [Header("初始格子数量")]
    [SerializeField]
    protected int initSlotCount = 9;

    protected List<CardSlot> slots = new();

    protected int UsedSlotsCount // 已使用的格子的数量
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
    protected int SlotsCount => slots.Count; // 所有格子的数量
    protected bool IsBagFull => UsedSlotsCount == SlotsCount; // 背包是否已满

    protected virtual void Start()
    {
        slotPrefab = Resources.Load<GameObject>("Prefabs/UI/Controls/CardSlot");
        slotContainer = transform.Find("Viewport/Container");
        InitBag();
    }

    protected void InitBag()
    {
        slots = new();
        // 添加格子
        AddSlot(initSlotCount);
    }

    protected void AddSlot(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slots.Add(slot);
        }
    }

    public virtual bool CanAddCard(CardInstance card)
    {
        // 背包有空位，一定能添加卡牌
        if (!IsBagFull) return true;

        // 背包没有空位
        // 能否添加取决于背包中的同类卡牌是否达到堆叠上限
        string cardName = card.CardData.cardName;
        var slots = GetSlotsContainingSimilarCard(cardName);
        foreach (CardSlot slot in slots)
        {
            if (slot.CanStack()) return true;
        }

        return false;
    }

    /// <summary>
    /// 获取所有放有同类卡牌的格子
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
    /// <param name="ascending">true: 按照堆叠数量升序排序，false: 降序排序</param>
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

    public virtual void AddCard(CardInstance card)
    {
        //if (!CanAddCard(card)) return;

        string cardName = card.CardData.cardName;

        // 首先尝试堆叠到已有卡牌
        foreach (var slot in GetSlotsContainingSimilarCard(cardName))
        {
            if (slot.CanStack())
            {
                slot.AddCard(card);
                return;
            }
        }

        // 否则放到新的空位
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                slot.AddCard(card);
                return;
            }
        }
    }

    public virtual void RemoveCard(CardInstance card)
    {
        string cardName = card.CardData.cardName;

        var slots = GetSlotsContainingSimilarCard(cardName, false);

        if (slots.Count == 0) return;

        // 能得到这个slot说明里面至少有一张牌，所以可以直接从里面移除卡牌
        slots[0].RemoveCard();
    }

    public virtual void Clear()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
    }

    /// <summary>
    /// 紧凑排列卡牌，让每个卡牌尽可能向前移动
    /// </summary>
    public void CompactCards()
    {
        // 记录需要移动的卡牌和它们的原始位置
        List<(CardSlot slot, int index)> nonEmptySlots = new List<(CardSlot, int)>();

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
            while (sourceSlot.StackCount > 0)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
        // 如果目标槽位有相同卡牌且可以堆叠
        else if (targetSlot.ContainsSimilarCard(sourceSlot.Card.cardName) && targetSlot.CanStack())
        {
            // 尽可能多地移动卡牌到目标槽位
            while (sourceSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
    }
}