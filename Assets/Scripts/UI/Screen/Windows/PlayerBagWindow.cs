using UnityEngine.UI;

public class PlayerBagWindow : BagWindow
{
    private Text loadText; // 载重显示

    private float maxLoad;

    private float currentLoad;

    public float MaxLoad => maxLoad;
    public float CurrentLoad => currentLoad;

    protected override void Awake()
    {
        base.Awake();
        loadText = transform.Find("TopBar/CurrentLoad").GetComponent<Text>();
    }

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagRuntimeData);
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

        DisplayBagLoad(currentLoad, maxLoad);

        EventManager.Instance.AddListener<ChangeLoadArgs>(EventType.ChangeLoad, OnLoadChanged);
    }

    private void OnLoadChanged(ChangeLoadArgs args)
    {
        DisplayBagLoad(args.currentLoad, args.maxLoad);
    }

    private void DisplayBagLoad(float currentLoad, float maxLoad)
    {
        loadText.text = $"载重: {currentLoad:0.0} / {maxLoad:0.0}";
    }

    public override bool CanAddCard(CardInstance card)
    {
        // 载重不足，无法添加卡牌
        if (CurrentLoad + card.GetCardData().weight > maxLoad) return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }

    public override void AddCard(CardInstance card)
    {
        if (CanAddCard(card))
        {
            base.AddCard(card);
            AddLoad(card.GetCardData().weight);
        }
    }

    public override CardInstance RemoveCard(CardSlot targetSlot)
    {
        var toRemove = base.RemoveCard(targetSlot);
        if (toRemove != null)
            AddLoad(-toRemove.GetCardData().weight);
        return toRemove;
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

    protected override void RecordRuntimeData()
    {
        PlayerBagRuntimeData runtimeData = new();
        runtimeData.maxLoad = maxLoad;
        runtimeData.cardSlotsRuntimeData = new();
        foreach (var slot in slots)
        {
            runtimeData.cardSlotsRuntimeData.Add(new() { cardInstanceList = slot.Cards });
        }

        GameDataManager.Instance.RecordPlayerBagRuntimeData(runtimeData);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventManager.Instance.RemoveListener<ChangeLoadArgs>(EventType.ChangeLoad, OnLoadChanged);
    }
}