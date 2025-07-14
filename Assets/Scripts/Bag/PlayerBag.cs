public class PlayerBag : BagBase
{
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
        StateManager.Instance.AddLoad(args.card.CardData.weight * args.add);
    }

    protected override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagData);
    }
    public override bool CanAddCard(CardInstance card)
    {
        // 因为背包和装备共用载重
        // 不是从装备中添加的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not EquipmentBag) &&
            StateManager.Instance.curLoad + card.CardData.weight > StateManager.Instance.maxLoad)
            return false;

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card);
    }
}