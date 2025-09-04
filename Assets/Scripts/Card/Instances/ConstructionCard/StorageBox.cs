public class StorageBox : ConstructionCard
{
    private InnerContentsComponent innerContents;

    private StorageBox()
    {
    }

    public override bool CanQuickInteract(Card card)
    {
        if (base.CanQuickInteract(card)) return true;

        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        if (base.CanQuickInteract(slot.PeekCard()))
        {
            base.QuickIneract(slot, count, out tip);
            return;
        }

        innerContents.QuickIneract(slot, count, out tip);
    }
}