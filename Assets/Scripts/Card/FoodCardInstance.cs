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
        EventManager.Instance.AddListener(EventType.RefreshCard, UpdateFresh);
    }

    private void UpdateFresh()
    {
        currentFresh -= TimeManager.Instance.SettleInterval;
        if (currentFresh < 0)
        {
            DestroyThisCard();
            EffectResolve.Instance.Resolve((GetCardData() as FoodCardData).onRotton);
        }
    }
}