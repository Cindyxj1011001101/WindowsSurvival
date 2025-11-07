/// <summary>
/// 自热烹饪袋
/// </summary>
public class SelfHeatingCookingBag : Card
{
    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<CookComponent>(out var cook) && cook.leftCookTime > 0)
        {
            tip = "煮熟食物";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        Use();
        card.DestroyThis();

        PlaySound("点火_02", true);

        TimeManager.Instance.AddTime(15);

        card.TryGetComponent<CookComponent>(out var cook);
        cook.HandleCookComplete();
    }
}