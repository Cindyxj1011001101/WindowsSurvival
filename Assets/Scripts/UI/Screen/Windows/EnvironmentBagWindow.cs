public class EnvironmentBagWindow : BagWindow
{
    public CardEvent CardEvent;
    public PlaceEnum place;
    public float discoveryDegree;

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.EnvironmentBagRuntimeData);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        base.InitBag(runtimeData);
        discoveryDegree = (runtimeData as EnvironmentBagRuntimeData).discoveryDegree;
    }

    public override void AddCard(CardInstance card)
    {
        // 如果放不下，就新增格子
        if (!CanAddCard(card))
        {
            // 暂定每次新增3个格子
            AddSlot(3);
        }
        base.AddCard(card);
    }

    protected override void RecordRuntimeData()
    {
        EnvironmentBagRuntimeData runtimeData = new();
        runtimeData.discoveryDegree = discoveryDegree;
        runtimeData.cardSlotsRuntimeData = new();
        foreach (var slot in slots)
        {
            runtimeData.cardSlotsRuntimeData.Add(new() { cardInstanceList = slot.Cards });
        }

        GameDataManager.Instance.RecordEnvironmentBagRuntimeData(runtimeData);
    }
}
