/// <summary>
/// 储物箱
/// </summary>
public class StorageBox : ConstructionCard
{
    public override bool CanQuickInteract(Card card, out string tip)
    {
        // 拆毁
        if (base.CanQuickInteract(card, out tip)) return true;
        // 放入
        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        if (base.CanQuickInteract(slot.PeekCard(), out _))
        {
            base.QuickIneract(slot, count);
            return;
        }

        innerContents.QuickIneract(slot, count);
    }
}