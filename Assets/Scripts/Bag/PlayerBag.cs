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
                currentLoad += GetCardWeight(slot.PeekCard()) * slot.StackCount;
        }

        // 注册载重变化的事件

    }

    public override bool CanAddCard(CardInstance card)
    {
        // 载重不足，无法添加卡牌
        if (CurrentLoad + GetCardWeight(card) > maxLoad) return false;

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

    private float GetCardWeight(CardInstance card)
    {
        CardData cardData = card.CardData;
        return cardData switch
        {
            FoodCardData => (cardData as FoodCardData).Weight,
            ToolCardData => (cardData as ToolCardData).Weight,
            ResourceCardData => (cardData as ResourceCardData).Weight,
            _ => 0,
        };
    }
}