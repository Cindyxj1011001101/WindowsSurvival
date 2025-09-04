public abstract class CookableCard : Card
{
    private CookComponent cookComponent;

    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out cookComponent);
    }

    public override bool CanQuickInteract(Card card)
    {
        return cookComponent.leftCookTime > 0 && card.CardId == "自热烹饪袋";
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        base.QuickIneract(slot, count, out tip);
        DestroyThis();
        slot.PeekCard().Use();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("点火_02", true);
        TimeManager.Instance.AddTime(15);
        AddCard(cookComponent.outcomeCardId, true);
    }
}