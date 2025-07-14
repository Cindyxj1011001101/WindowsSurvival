public class PlayerBag : BagBase
{
    private float maxLoad;

    private float currentLoad;

    public float MaxLoad => maxLoad;
    public float CurrentLoad => currentLoad;

    private void Awake()
    {
        EventManager.Instance.AddListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangePlayerBagCardsArgs>(EventType.ChangePlayerBagCards, OnCardsChanged);
    }

    public void OnCardsChanged(ChangePlayerBagCardsArgs args)
    {
        AddLoad(args.card.CardData.weight * args.add);
    }

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
                AddLoad(slot.PeekCard().CardData.weight * slot.StackCount);
        }
        maxLoad = (runtimeData as PlayerBagRuntimeData).maxLoad;

        TriggerChangeLoadEvent();
    }

    public override bool CanAddCard(CardInstance card)
    {
        // 载重不足，无法添加卡牌
        if (CurrentLoad + card.CardData.weight > maxLoad) return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
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