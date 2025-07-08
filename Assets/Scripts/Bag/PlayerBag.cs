public class PlayerBag : BagBase
{
    private float maxLoad = 15;

    private float currentLoad;

    public float MaxLoad => maxLoad;
    public float CurrentLoad => currentLoad;

    protected override void Start()
    {
        base.Start();
        // 添加基础卡牌

        // 计算载重
        currentLoad = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
                // 因为同样的卡牌重量都是一样的，所以可以这样算
                //currentLoad += GetCardWeight(slot.CardInstanceList[0]) * slot.StackCount;
                currentLoad += slot.PeekCard().CardData.weight * slot.StackCount;
        }

        // 注册载重变化的事件

    }

    public override bool CanAddCard(CardInstance card)
    {
        // 载重不足，无法添加卡牌
        if (CurrentLoad + card.CardData.weight > maxLoad) return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }

    public override void AddCard(CardInstance card)
    {
        if (CanAddCard(card))
        {
            base.AddCard(card);
            // 触发载重变化的事件

        }
    }
}