public class OpenedInsurance : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private OpenedInsurance()
    {
    }

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }
}