using System.Collections.Generic;
using UnityEngine;

public class InnerBag : BagBase
{
    private InnerContentComponent component;

    public void InitFromInnerContentComponent(InnerContentComponent component)
    {
        this.component = component;
        // 清除原来的信息
        Clear();

        // 初始化新的信息
        AddSlot(component.slotCount);
        for (int i = 0; i < component.slotCount; i++)
        {
            var cardList = component.innerContents[i];
            slots[i].InitFromCardList(cardList);
        }
    }

    public override List<(CardSlot, int)> GetSlotsCanAddCard(Card card, int count)
    {
        var result = new List<(CardSlot, int)>();

        // 不能放置这种卡牌，直接返回空列表
        if (component.canAddContent != null && !component.canAddContent(card)) return result;

        int leftCount = count; // 剩余要添加的数量

        // 优先堆叠，卡牌格按照已堆叠数量降序排序，即优先堆满
        foreach (var slot in GetSlotsByCardId(card.CardId, false))
        {
            if (leftCount <= 0) return result;

            int remainingCapacity = slot.GetRemainingCapacity(card);
            if (remainingCapacity > 0)
            {
                result.Add((slot, Mathf.Min(remainingCapacity, leftCount)));
                leftCount -= remainingCapacity;
            }
        }

        // 如果还有要添加的卡牌
        if (leftCount > 0)
        {
            // 找空位
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    result.Add((slot, Mathf.Min(card.MaxStackNum, leftCount)));
                    leftCount -= card.MaxStackNum;
                }

                if (leftCount <= 0) return result;
            }
        }

        // 剩余卡牌无法继续添加
        return result;
    }

    protected override void Init()
    {
        
    }
}