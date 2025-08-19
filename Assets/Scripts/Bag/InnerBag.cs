using Newtonsoft.Json;

public class InnerBag : Bag
{
    private InnerContentsComponent component;

    [JsonIgnore]
    public Card BelongedCard => component.BelongedCard;

    [JsonIgnore]
    public float WeightLossRate => component.weightLossRate;

    public void SetComponent(InnerContentsComponent component)
    {
        this.component = component;
    }

    public override bool CanAddCard(Card card, out string tip)
    {
        // 不能嵌套放置
        if (card.CardId == component.BelongedCard.CardId)
        {
            tip = "不能嵌套放置同类卡牌";
            return false;
        }

        // 卡牌不满足过滤器限制
        if (component.contentFilter != null && !component.contentFilter(card, out tip)) return false;

        // 考虑重量
        if (BelongedCard.Bag is PlayerBag || BelongedCard.Bag is EquipmentBag)
        {
            if (!CanAddCardConsideringWeight(card, out tip))
            {
                return false;
            }
        }

        return base.CanAddCard(card, out tip);
    }

    public override void OnAddCard(Card card)
    {
        component.BelongedCard.RefreshSlot();

        // 计算重量
        if (BelongedCard.Bag is PlayerBag || BelongedCard.Bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight * (1 - component.weightLossRate));
        }
        
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, component.BelongedCard);

        component.onAddCard?.Invoke(card);
    }

    public override void OnRemoveCard(Card card)
    {
        component.BelongedCard.RefreshSlot();

        if (BelongedCard.Bag is PlayerBag || BelongedCard.Bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight * (1 - component.weightLossRate));
        }

        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, component.BelongedCard);

        component.onRemoveCard?.Invoke(card);
    }
}