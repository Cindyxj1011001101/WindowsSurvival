using System.Collections.Generic;

/// <summary>
/// 野炊营火
/// </summary>
public class Campfire : ConstructionCard
{
    private InnerContentsComponent innerContents; // 内容物组件
    private FuelStorageComponent fuelStorage; // 燃料存储组件

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
        // 手动添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96);
            AddComponent(fuelStorage);
        }

        // 放入内容物时，暂停卡牌每回合更新
        innerContents.onAddCard = (c) =>
        {
            if (fuelStorage.isFiring) c.PauseUpdating();
        };
        // 取出时恢复每回合更新
        innerContents.onRemoveCard = (c) =>
        {
            c.ContinueUpdating();
        };

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
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

    /// <summary>
    /// 点燃
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Light(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, -4); // 点燃后地点氧气每回合-4
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, +2); // 点燃后地点一氧化碳每回合+2

        // 点燃后暂停所有卡牌每回合更新
        innerContents.PauseUpdating();

        fuelStorage.SetIsFiring(true);
    }

    private bool Judge_Light(out string hint)
    {
        hint = string.Empty;
        if (fuelStorage.isFiring)
        {
            return false;
        }

        if (fuelStorage.fuel < 2)
        {
            hint = "燃料不足，无法点燃篝火";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 熄灭
    /// </summary>
    /// <param name="tip"></param>
    private void Event_UnLight(out string tip)
    {
        tip = string.Empty;

        var env = Bag as EnvironmentBag;
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.Oxygen, +4);
        env.ChangeEnvironmentState(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

        // 熄灭后恢复所有卡牌每回合更新
        innerContents.ContinueUpdating();

        fuelStorage.SetIsFiring(false);
    }

    private bool Judge_UnLight(out string hint)
    {
        hint = string.Empty;
        return fuelStorage.isFiring;
    }

    private List<Card> temp = new();
    protected override System.Action OnUpdate => () =>
    {
        // 没有点燃
        if (!fuelStorage.isFiring) return;

        // 这里剩余燃料一定是>=2的，因为燃料<2时会自动熄灭并且无法点燃
        fuelStorage.AddFuel(-2); // 每回合消耗2点燃料

        // 记录所有内容物
        foreach (var slot in innerContents.bag.Slots)
        {
            for (int i = slot.Cards.Count - 1; i >= 0; i--)
            {
                temp.Add(slot.Cards[i]);
            }
        }

        // 内容物增加烹饪进度
        foreach (var card in temp)
        {
            if (card != null && card.TryGetComponent(out CookComponent cookComponent))
            {
                // 使用局部变量捕获当前的值
                Card currentCard = card;

                cookComponent.Update(TimeManager.Instance.SettleInterval, (outcomeId) =>
                {
                    // 处理煮熟的逻辑
                    currentCard.DestroyThis();
                    AddCard(outcomeId, innerContents.bag);
                    ShowTip($"{currentCard.CardName}熟了");
                });
            }
        }

        temp.Clear();

        if (fuelStorage.fuel < 2) // 燃料不足时自动熄灭
        {
            Event_UnLight(out _);
            ShowTip("燃料不足，营火已自动熄灭");
            return;
        }
    };

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (card.TryGetComponent<FlammableComponent>(out _) && fuelStorage.fuel < fuelStorage.maxFuel) return true;
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
        if (card.TryGetComponent<FlammableComponent>(out var burnableComponent) && fuelStorage.fuel < fuelStorage.maxFuel)
        {
            card.DestroyThis();
            fuelStorage.AddFuel(burnableComponent.fuelValue);
            return;
        }

        // 放入内容物
        if (innerContents.CanQuickInteract(card))
        {
            innerContents.QuickIneract(slot, count, out tip);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count, out tip);
    }
}