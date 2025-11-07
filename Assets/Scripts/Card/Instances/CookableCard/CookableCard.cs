public abstract class CookableCard : Card
{
    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (cook.leftCookTime > 0 && card.CardId == "自热烹饪袋")
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
        PlaySound("点火_02", true);
        TimeManager.Instance.AddTime(15);
        cook.HandleCookComplete();
    }
}