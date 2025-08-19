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
        if (!CanAddCardConsideringWeight(card, out tip))
        {
            return false;
        }

        // 载重足够则按照父类的判断标准进行判断
        return base.CanAddCard(card, out tip);
    }
}