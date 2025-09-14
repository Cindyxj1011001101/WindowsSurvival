public class PlayerBag : Bag
{
    protected override void FirstInit()
    {
        AddSlot(9);
    }

    public override void OnAddCard(Card card)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight);

        EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = 1 });
    }

    public override void OnRemoveCard(Card card)
    {
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight);

        EventManager.Instance.TriggerEvent(EventType.ChangePlayerBagCards,
                new ChangePlayerBagCardsArgs { card = card, add = -1 });
    }

    public override bool CanAddCard(Card card, out string tip)
    {
        if (!card.Moveable)
        {
            tip = "不能移动该卡牌";
            return false;
        }

        if (card.TryGetComponent<InnerContentsComponent>(out _))
        {
            tip = "麦麦的兜里放不下整个包";
            return false;
        }

        if (!CanAddCardConsideringWeight(card, out tip))
        {
            return false;
        }

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card, out tip);
    }
}