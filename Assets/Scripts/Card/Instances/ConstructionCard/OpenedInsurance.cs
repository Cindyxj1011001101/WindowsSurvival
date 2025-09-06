public class OpenedInsurance : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private OpenedInsurance()
    {
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }
}