public class InnerCardSlot : CardSlot
{
    private InnerContentsComponent component;

    public void SetInnerContentsComponent(InnerContentsComponent component)
    {
        this.component = component;
    }

    public override void AddCard(Card card)
    {
        base.AddCard(card);
        card.SetParentCard(component.BelongedCard);
    }

    public override void RemoveCard(Card card)
    {
        base.RemoveCard(card);
        card.SetParentCard(null);
    }
}