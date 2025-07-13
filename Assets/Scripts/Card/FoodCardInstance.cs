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
        EventManager.Instance.AddListener<ChangeTimeArgs>(EventType.ChangeTime, UpdateFresh);
    }

    private void UpdateFresh(ChangeTimeArgs args)
    {
        if ((CardData as FoodCardData).MaxFresh == -1) return;

        currentFresh -= args.timeDelta;
        if (currentFresh <= 0)
        {
            DestroyThisCard();
            GameManager.Instance.HandleCardEvent((CardData as FoodCardData).onRotton);
        }
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
    }

    protected override void DestroyThisCard()
    {
        base.DestroyThisCard();
        EventManager.Instance.RemoveListener<ChangeTimeArgs>(EventType.ChangeTime, UpdateFresh);
    }
}