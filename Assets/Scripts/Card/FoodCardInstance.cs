public class FoodCardInstance : CardInstance
{
    public int currentFresh;

    public override int CompareTo(CardInstance other)
    {
        // 优先级为新鲜度低的优先级高
        if (other is FoodCardInstance) return currentFresh - (other as FoodCardInstance).currentFresh;
        return 0;
    }

    public override void InitFromCardData(CardData cardData)
    {
        base.InitFromCardData(cardData);
        currentFresh = (cardData as FoodCardData).MaxFresh;
        EventManager.Instance.AddListener(EventType.IntervalSettle, UpdateFresh);
    }

    private void UpdateFresh()
    {
        if ((GetCardData() as FoodCardData).MaxFresh == -1) return;

        currentFresh -= TimeManager.Instance.SettleInterval;
        if (currentFresh < 0)
        {
            DestroyThisCard();
            EffectResolve.Instance.Resolve((GetCardData() as FoodCardData).onRotton);
        }
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
    }

    protected override void DestroyThisCard()
    {
        base.DestroyThisCard();
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, UpdateFresh);
    }
}