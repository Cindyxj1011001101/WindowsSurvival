public class InnerBag : Bag
{
    private InnerContentsComponent component;

    public override void Init()
    {
        base.Init();
        foreach (var slot in Slots)
        {
            foreach (var c in slot.Cards)
            {
                c.SetParentCard(component.BelongedCard);
            }
        }
    }

    public void SetComponent(InnerContentsComponent component)
    {
        this.component = component;
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

    public override void OnAddCard(Card card)
    {
        card.SetParentCard(component.BelongedCard);
        component.BelongedCard.RefreshSlot();
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, component.BelongedCard);
    }

    public override void OnRemoveCard(Card card)
    {
        card.SetParentCard(null);
        component.BelongedCard.RefreshSlot();
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, component.BelongedCard);
    }
}