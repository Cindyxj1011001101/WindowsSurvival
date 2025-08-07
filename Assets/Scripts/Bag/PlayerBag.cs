public class PlayerBag : BagBase
{
    public override void Init()
    {
        InitBag(GameDataManager.Instance.PlayerBagData);
    }

    protected override void InitBag(BagRuntimeData runtimeData)
    {
        base.InitBag(runtimeData);
        if (!runtimeData.init)
        {
            // 初始携带一个压缩饼干
            var card = CardFactory.CreateCard("压缩饼干");
            AddCard(card);
            card.Slot.RefreshCurrentDisplay();
        }
    }

    public override bool CanAddCard(Card card, out string tip)
    {
        float curLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].CurValue;
        float maxLoad = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Load].MaxValue;
        // 因为背包和装备共用载重
        // 不是从装备中添加的，要看载重够不够
        if ((card.Slot == null || card.Slot.Bag is not EquipmentBag) &&
            curLoad + card.Weight > maxLoad)
        {
            tip = "再带上它就太重了";
            return false;
        }

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card, out tip);
    }
}