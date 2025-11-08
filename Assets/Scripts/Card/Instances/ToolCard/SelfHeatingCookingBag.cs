/// <summary>
/// 自热烹饪袋
/// </summary>
public class SelfHeatingCookingBag : Card
{
    public override bool CanQuickInteract(Card card, out string tip)
    {
        return CanCook(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        Cook(slot.PeekCard());
    }

    public bool CanCook(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<CookComponent>(out var cook) && cook.leftCookTime > 0)
        {
            tip = "煮熟食物";
            return true;
        }
        return false;
    }

    public void Cook(Card food)
    {
        PlaySound("点火_02", true);
        Use();
        food.DestroyThis();
        TimeManager.Instance.AddTime(15);
        food.TryGetComponent<CookComponent>(out var cook);
        cook.HandleCookComplete();
    }
}