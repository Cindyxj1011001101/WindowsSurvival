public class StorageBox : ConstructionCard
{
    private InnerContentsComponent innerContents;

    private StorageBox()
    {
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        if (base.CanQuickInteract(card, out tip)) return true;

        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        if (base.CanQuickInteract(slot.PeekCard(), out _))
        {
            base.QuickIneract(slot, count, out tip);
            return;
        }

        innerContents.QuickIneract(slot, count, out tip);
    }
}