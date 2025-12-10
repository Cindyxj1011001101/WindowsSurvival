/// <summary>
/// 保险柜
/// </summary>
[CardId("被撬开的保险柜")]
public class OpenedInsurance : ConstructionCard
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