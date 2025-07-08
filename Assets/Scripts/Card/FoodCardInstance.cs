public class FoodCardInstance : CardInstance
{
    private int currentFresh;

    public int CurrentFresh => currentFresh;

    public override int CompareTo(CardInstance other)
    {
        // 优先级为新鲜度低的优先级高
        if (other is FoodCardInstance) return currentFresh - (other as FoodCardInstance).currentFresh;
        return 0;
    }

    public override void InitFromCardData(CardData cardData)
    {
        currentFresh = (cardData as FoodCardData).MaxFresh;
    }
}