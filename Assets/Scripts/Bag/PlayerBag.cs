public class PlayerBag : BagBase
{
    private float maxLoad = 15;

    private float currentLoad;
     public PlayerBagData playerBagData;//初始背包数据
     public int curStack;//当前卡牌堆数量
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
    // public void AddCard(CardData cardData)
    //  {
//         //记录当前是否有相同卡牌
//             bool hasSame = false;
//             //在当前卡牌库中寻找该卡牌
//             foreach (var card in cardList)
//             {
//                 //卡牌数据相同且未到达堆叠上限时找到相同可堆叠卡牌
//                 if (card.cardData == cardData&&card.cardNum<cardData.maxStackNum)
//                 {
//                     //堆叠该卡牌
//                     card.cardNum++;
//                     curHeavy+=cardData.weight;
//                     hasSame = true;
//                     return;
//                 }
//             }
//             //没有相同卡牌则按照类型创建
//             if (hasSame== false)
//             {
//                 curStack++;
//                 //卡牌数据不同或者已到达堆叠上限
//                 //根据卡牌类型创建该卡牌
//                 if (cardData.GetType() == typeof(ResourcePointCardData))
//                 {
//                     ResourcePointCardData resourcePointCard = (ResourcePointCardData)cardData;
//                     cardList.Add(new ResourcePointCard(cardData,1,resourcePointCard.maxEndurance));
//                 }
//                 else if (cardData.GetType() == typeof(ToolCardData))
//                 {
//                     ToolCardData resourcePointCard = (ToolCardData)cardData;
//                     cardList.Add(new ToolCard(cardData,1,resourcePointCard.maxEndurance));
//                 }
//                 else
//                 {
//                     cardList.Add(new Card(cardData,1));
//                 }
//                 
//             }
     // }
}