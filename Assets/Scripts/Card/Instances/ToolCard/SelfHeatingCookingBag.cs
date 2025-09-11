/// <summary>
/// ×ÔÈÈÅëâ¿´ü
/// </summary>
public class SelfHeatingCookingBag : Card
{
    private SelfHeatingCookingBag()
    {

    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        return card.TryGetComponent<CookComponent>(out var cook) && cook.leftCookTime > 0;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        Use();
        card.DestroyThis();

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("µã»ğ_02", true);

        TimeManager.Instance.AddTime(15);

        card.TryGetComponent<CookComponent>(out var cook);
        cook.CookingComplete();
    }
}