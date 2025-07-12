public class PlayerBag : BagBase
{
    private float maxLoad;

    private float currentLoad;

    public float MaxLoad => maxLoad;
    public float CurrentLoad => currentLoad;

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagData);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        base.InitBag(runtimeData);

        // 计算载重
        currentLoad = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
                // 因为同样的卡牌重量都是一样的，所以可以这样算
                AddLoad(slot.PeekCard().GetCardData().weight * slot.StackCount);
        }
        maxLoad = (runtimeData as PlayerBagRuntimeData).maxLoad;

        TriggerChangeLoadEvent();
    }
    public override bool CanAddCard(CardInstance card)
    {
        // 载重不足，无法添加卡牌
        if (CurrentLoad + card.GetCardData().weight > maxLoad) return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }

    public override void OnCardAdded(CardInstance card)
    {
        base.OnCardAdded(card);
        AddLoad(card.GetCardData().weight);
    }

    public override void OnCardRemoved(CardInstance card)
    {
        base.OnCardRemoved(card);
        AddLoad(-card.GetCardData().weight);
    }

    ChangeLoadArgs args = new ChangeLoadArgs();
    private void AddLoad(float weight)
    {
        currentLoad += weight;
        TriggerChangeLoadEvent();
    }

    private void TriggerChangeLoadEvent()
    {
        // 触发载重变化的事件
        args.currentLoad = currentLoad;
        args.maxLoad = maxLoad;
        EventManager.Instance.TriggerEvent(EventType.ChangeLoad, args);
    }
}