using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BagBase : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab; // 格子预制体
    [SerializeField] private RectTransform slotContainer; // 格子存放位置
    [SerializeField] private GridLayoutGroup gridLayout; // 格子布局

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

    public void AddSlot(int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            CardSlot slot = slotObj.GetComponent<CardSlot>();
            slot.SetBag(this);
            slot.ClearSlot();
            slots.Add(slot);
        }

        // 添加格子后更新容器高度
        UpdateContainerHeight();
    }

    /// <summary>
    /// 更新容器高度
    /// </summary>
    private void UpdateContainerHeight()
    {
        float containerWidth = slotContainer.rect.width;
        // 计算一行可以放几个格子
        int i = 1;
        while (gridLayout.cellSize.x * i + gridLayout.spacing.x * (i - 1) + gridLayout.padding.left + gridLayout.padding.right <= containerWidth)
        {
            i++;
        }
        int columns = Mathf.Max(1, i - 1);
        int totalRows = Mathf.CeilToInt((float)slots.Count / columns);

        // 计算容器高度
        float containerHeight = totalRows * gridLayout.cellSize.y + (totalRows - 1) * gridLayout.spacing.y + gridLayout.padding.top + gridLayout.padding.bottom;

        slotContainer.sizeDelta = new Vector2(slotContainer.sizeDelta.x, containerHeight);
    }

    public virtual bool CanAddCard(CardInstance card)
    {
        // 如果背包未满，则可以添加
        if (!IsBagFull) return true;

        // 背包已满
        // 则能否添加取决于背包中是否有同类卡牌并且没有达到堆叠上限
        string cardName = card.GetCardData().cardName;
        var slots = GetSlotsContainingSimilarCard(cardName);
        foreach (CardSlot slot in slots)
        {
            if (slot.CanStack()) return true;
        }

        return false;
    }

    /// <summary>
    /// 获取放有同类卡牌的slot
    /// </summary>
    /// <param name="cardName">卡牌名称</param>
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

    public virtual void AddCard(CardInstance card)
    {
        //if (!CanAddCard(card)) return;

        string cardName = card.GetCardData().cardName;

        // 尝试堆叠同类卡牌
        foreach (var slot in GetSlotsContainingSimilarCard(cardName))
        {
            if (slot.CanStack())
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
            case ToolTagCondition toolTagCondition:
                foreach (var slot in slots)
                {
                    if (slot.IsEmpty) continue;

                    if (slot.CardData.GetType() == typeof(ToolCardData))
                    {
                        ToolCardData toolCardData = slot.CardData as ToolCardData;
                        if (toolCardData.tag == toolTagCondition.ConditionToolTag)
                        {
                            return slot;
                        }
                    }
                }
                return null;
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
    /// 移除卡牌
    /// </summary>
    /// <param name="sourceSlot">从哪个格子里移除</param>
    public virtual CardInstance RemoveCard(CardSlot sourceSlot)
    {
        if (sourceSlot.StackCount == 0) return null;

        return sourceSlot.RemoveCard();
    }

    public CardInstance RemoveCard(CardInstance card)
    {
        string cardName = card.GetCardData().cardName;

        var slots = GetSlotsContainingSimilarCard(cardName, false);

        if (slots.Count > 0)
            return RemoveCard(slots[0]);

        return null;
    }

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
            while (!sourceSlot.IsEmpty)
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
        // 如果目标槽位有相同卡牌且可以堆叠
        else if (targetSlot.ContainsSimilarCard(sourceSlot.CardData.cardName) && targetSlot.CanStack())
        {
            // 尽可能多地移动卡牌到目标槽位
            while (sourceSlot.StackCount > 0 && targetSlot.CanStack())
            {
                targetSlot.AddCard(sourceSlot.RemoveCard());
            }
        }
    }
}