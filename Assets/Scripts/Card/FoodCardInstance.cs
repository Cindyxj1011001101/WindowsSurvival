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
        //EventManager.Instance.AddListener<ChangeTimeArgs>(EventType.ChangeTime, UpdateFresh);
        EventManager.Instance.AddListener(EventType.IntervalSettle, UpdateFresh);
    }

    //private void UpdateFresh(ChangeTimeArgs args)
    //{
    //    if ((CardData as FoodCardData).MaxFresh == -1) return;

    //    currentFresh -= args.timeDelta;
    //    if (currentFresh <= 0)
    //    {
    //        DestroyThisCard();
    //        GameManager.Instance.HandleCardEvent((CardData as FoodCardData).onRotton);
    //    }
    //    EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
    //}

    private void UpdateFresh()
    {
        if ((CardData as FoodCardData).MaxFresh == -1) return;

        currentFresh -= TimeManager.Instance.SettleInterval;
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
        //EventManager.Instance.RemoveListener<ChangeTimeArgs>(EventType.ChangeTime, UpdateFresh);
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, UpdateFresh);
    }
    public override void Use()
    {
        // maxEndurance <= 0表示无限耐久
        if (CardData.maxEndurance <= 0) return;

        currentEndurance--;
        if(SoundManager.Instance != null)
            SoundManager.Instance.PlaySound("吃_01");
        if (currentEndurance <= 0)
        {

            // 销毁这张卡牌
            DestroyThisCard();
            // 触发卡牌耐久归零事件
            GameManager.Instance.HandleCardEvent(CardData.onUsedUp);
        }
        // 刷新前端显示的卡牌数据
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty);
    }
}