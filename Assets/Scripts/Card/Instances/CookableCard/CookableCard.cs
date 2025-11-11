public abstract class CookableCard : Card
{
    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        return card is SelfHeatingCookingBag s && s.CanCook(this, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        (slot.PeekCard() as SelfHeatingCookingBag).Cook(this);
    }
}