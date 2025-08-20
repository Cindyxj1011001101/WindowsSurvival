/// <summary>
/// 野炊营火
/// </summary>
public class Campfire : ConstructionCard
{
    private InnerContentsComponent innerContents;
    private FuelComponent fuelComponent;
    public bool isLightened = false;
    private Campfire()
    {
        Events = new()
        {
            new Event("点燃", "", Event_Light, Judge_Light),
            new Event("熄灭", "", Event_UnLight, Judge_UnLight)
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        if (!TryGetComponent(out fuelComponent))
        {
            fuelComponent = new FuelComponent(96);
            AddComponent(fuelComponent);
        }
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (!c.TryGetComponent<CookComponent>(out _))
        {
            s = "只能放入可烹饪的物品";
            return false;
        }
        return true;
    }

    private void Event_Light(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -4); // 点燃后地点氧气每回合-4
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, +2); // 点燃后地点一氧化碳每回合+2

        isLightened = true;
    }

    private bool Judge_Light(out string hint)
    {
        hint = string.Empty;
        if (isLightened)
        {
            return false;
        }

        if (fuelComponent.fuel < 2)
        {
            hint = "燃料不足，无法点燃篝火";
            return false;
        }

        return true;
    }

    private void Event_UnLight(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +4);
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

        isLightened = false;
    }

    private bool Judge_UnLight(out string hint)
    {
        hint = string.Empty;
        return isLightened;
    }

    protected override System.Action OnUpdate => () =>
    {
        if (!isLightened || fuelComponent.fuel < 2) return;

        fuelComponent.AddFuel(-2); // 每回合消耗2点燃料

        Card card;
        foreach (var slot in innerContents.bag.Slots)
        {
            for (int i = slot.Cards.Count - 1; i >= 0; i--)
            {
                card = slot.Cards[i];
                if (card != null && card.TryGetComponent(out CookComponent cookComponent))
                {
                    // 使用局部变量捕获当前的值
                    Card currentCard = card;

                    cookComponent.Update(TimeManager.Instance.SettleInterval, (outcomeId) =>
                    {
                        // 处理煮熟的逻辑
                        currentCard.DestroyThis();
                        AddCard(outcomeId, innerContents.bag);
                    });
                }
            }
        }

        if (fuelComponent.fuel < 2) // 燃料不足时自动熄灭
        {
            isLightened = false;
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
            ShowTip("燃料不足，营火已自动熄灭");
            return;
        }
    };

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out _) && fuelComponent.fuel < fuelComponent.maxFuel)
        {
            return true;
        }
        // 放入内容物
        if (innerContents.CanQuickInteract(card)) return true;
        // 拆毁
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuelComponent.fuel < fuelComponent.maxFuel)
        {
            card.DestroyThis();
            fuelComponent.AddFuel(burnableComponent.fuelValue);
            return;
        }

        // 放入内容物
        if (innerContents.CanQuickInteract(card))
        {
            innerContents.QuickIneract(slot, count, out tip);
            return;
        }

        base.QuickIneract(slot, count, out tip);
    }
}