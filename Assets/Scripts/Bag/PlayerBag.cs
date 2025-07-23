using System.Collections.Generic;

public class PlayerBag : BagBase
{
    private void Awake()
    {
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    public void OnCardsChanged(ChangePlayerBagCardsArgs args)
    {
        StateManager.Instance.AddLoad(args.card.Weight * args.add);
    }

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagData);
    }

    public override bool CanAddCard(Card card)
    {
        // 因为背包和装备共用载重
        // 不是从装备中添加的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not EquipmentBag) &&
            StateManager.Instance.CurLoad + card.Weight > StateManager.Instance.MaxLoad)
            return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }

    public override List<(CardSlot, int)> GetSlotsCanAddCard(Card card, int count)
    {
        float maxLoad = StateManager.Instance.MaxLoad;
        float curLoad = StateManager.Instance.CurLoad;

        List<(CardSlot, int)> result = new();

        int leftCount = count; // 剩余要添加的数量

        // 优先堆叠，卡牌格按照已堆叠数量降序排序，即优先堆满
        foreach (var slot in GetSlotsByCardId(card.CardId, false))
        {
            if (leftCount <= 0 || curLoad > maxLoad) return result;
            int moveCount = 0;
            for (int i = 0; i < slot.GetRemainingCapacity(card); i++)
            {
                curLoad += card.Weight;
                if (curLoad > maxLoad) break;
                leftCount--;
                moveCount++;
                if (leftCount <= 0) break;
            }

            if (moveCount > 0) result.Add((slot, moveCount));
        }

        // 如果还有要添加的卡牌
        // 这里不能while true，因为上面的循环可能正常结束但是leftCount<=0
        if (leftCount > 0 && curLoad <= maxLoad)
        {
            // 找空位
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    int moveCount = 0;
                    for (int i = 0; i < card.MaxStackNum; i++)
                    {
                        curLoad += card.Weight;
                        if (curLoad > maxLoad) break;
                        leftCount--;
                        moveCount++;
                        if (leftCount <= 0) break;
                    }

                    if (moveCount > 0) result.Add((slot, moveCount));
                }

                if (leftCount <= 0 || curLoad > maxLoad) return result;
            }
        }

        return result;
    }
}