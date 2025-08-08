public class InnerBag : BagBase
{
    private InnerContentsComponent component;

    public void InitFromInnerContentComponent(InnerContentsComponent component)
    {
        // 清除原来的信息
        Clear();

        this.component = component;

        // 初始化新的信息
        AddSlot(component.slotCount);
        for (int i = 0; i < component.slotCount; i++)
        {
            var cardList = component.innerContents[i];
            slots[i].Init(cardList);
            (slots[i] as InnerCardSlot).SetInnerContentsComponent(component);
        }
    }

    public override bool CanAddCard(Card card, out string tip)
    {
        // 不能嵌套放置
        if (card.CardId == component.BelongedCard.CardId)
        {
            tip = "不能嵌套放置同类卡牌";
            return false;
        }

        // 不能放置这种卡牌，直接返回空列表
        if (component.contentFilter != null && !component.contentFilter(card, out tip)) return false;

        return base.CanAddCard(card, out tip);
    }
    public override void Init()
    {
        
    }

    public override void Clear()
    {
        base.Clear();
        component = null;
    }
}