public abstract class CookableCard : Card
{
    private CookComponent cookComponent;

    public override void Awake()
    {
        base.Awake();
        TryGetComponent(out cookComponent);
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (cookComponent.leftCookTime > 0 && card.CardId == "自热烹饪袋")
        {
            tip = "煮熟食物";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        DestroyThis();
        slot.PeekCard().Use();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("点火_02", true);
        TimeManager.Instance.AddTime(15);
        TurnTo(cookComponent.outcomeCardId, Bag);
    }
}