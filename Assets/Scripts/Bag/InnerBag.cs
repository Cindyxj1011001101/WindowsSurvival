using Newtonsoft.Json;

public class InnerBag : Bag
{
    private InnerContentsComponent component;

    [JsonIgnore] public Card BelongedCard => component.BelongedCard;
    [JsonIgnore] public float WeightLossRate => component.weightLossRate;
    [JsonIgnore] public bool AllowAdd => component.allowAdd;
    [JsonIgnore] public bool AllowRemove => component.allowRemove;
    [JsonIgnore] public string NotAllowRemoveReason => component.notAllowRemoveReason;
    [JsonIgnore] public string NotAllowAddReason => component.notAllowAddReason;
    [JsonIgnore] public bool IsCraftMaterialSource => component.isCraftMaterialSource; // 是否作为配方材料来源

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

        // 不能套娃
        if (card.TryGetComponent<InnerContentsComponent>(out _))
        {
            tip = "不能放入带有内容物的卡牌";
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
        // 计算重量
        if (BelongedCard.Bag is PlayerBag || BelongedCard.Bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, card.Weight * (1 - component.weightLossRate));
        }

        component.onAddCard?.Invoke(card);

        component.RefreshSlot();
    }

    public override void OnRemoveCard(Card card)
    {
        if (BelongedCard.Bag is PlayerBag || BelongedCard.Bag is EquipmentBag)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Load, -card.Weight * (1 - component.weightLossRate));
        }

        component.onRemoveCard?.Invoke(card);

        component.RefreshSlot();
    }
}