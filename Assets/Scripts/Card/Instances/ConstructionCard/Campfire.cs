using System.Collections.Generic;

/// <summary>
/// 野炊营火
/// </summary>
public class Campfire : ConstructionCard
{
    private InnerContentsComponent innerContents; // 内容物组件
    private FuelStorageComponent fuelStorage; // 燃料存储组件
    private StateMachineComponent stateMachine;

    public override bool HasLoopSound => true;

    private Campfire()
    {
        Events = new()
        {
            new Event("点燃", "点燃营火。点燃状态下会导致室内氧气消耗与一氧化碳增加", Event_Light, Judge_Light),
            new Event("熄灭", "", Event_UnLight, Judge_UnLight)
        };
    }

    public override void LateInit()
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
            if (fuelStorage.isFiring)
            {
                c.PauseUpdating();
                c.TryGetComponent<CookComponent>(out var cook);
                if (cook.leftCookTime < 0) return;

                var timer = new TimerComponent(cook.leftCookTime, cook.totalCookTime);
                if (cook.outcomeCardId == "烧焦的食物")
                    timer.tipText = "烧焦";
                else
                    timer.tipText = "烤熟";
            }
        };
        // 取出时恢复每回合更新
        innerContents.onRemoveCard = (c) =>
        {
            c.ContinueUpdating();
            c.RemoveComponent<CookComponent>();
        };

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }
        
        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("未点燃", "18"),
                new ("已点燃", "18", true),
            };
            stateMachine = new StateMachineComponent("未点燃", states);
            AddComponent(stateMachine);
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
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, +2); // 点燃后地点一氧化碳每回合+2

        // 点燃后暂停所有卡牌每回合更新
        innerContents.PauseUpdating();

        // 显示烹饪计时器
        innerContents.ForEachCard(c =>
        {
            c.TryGetComponent<CookComponent>(out var cook);
            if (cook.leftCookTime < 0) return;
            
            var timer = new TimerComponent(cook.leftCookTime, cook.totalCookTime);
            if (cook.outcomeCardId == "烧焦的食物")
                timer.tipText = "烧焦";
            else
                timer.tipText = "烤熟";
            c.AddComponent(timer);
        });

        fuelStorage.SetIsFiring(true);
        SoundManager.Instance.PlaySound("点火_02");

        stateMachine.ChangeState("已点燃");

        // 只有玩家在同一地点且点燃时才播放循环音效
        if (env == GameManager.Instance.CurEnvironmentBag && fuelStorage.isFiring)
            SoundManager.Instance.PlayCardLoopSound(CardId, "野炊营火音效", 0.3f);
    }

    private bool Judge_Light(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.WaterLevel.CurValue >= 30)
        {
            hint = "水位过高，无法点燃营火";
            return false;
        }

        if (fuelStorage.fuel < 2)
        {
            hint = "燃料不足，无法点燃营火";
            return false;
        }

        return !fuelStorage.isFiring;
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
        env.ChangeEnvironmentStateChangeRate(EnvironmentStateEnum.CarbonMonoxideLevel, -2);

        // 熄灭后恢复所有卡牌每回合更新
        innerContents.ContinueUpdating();

        // 移除计时器组件
        innerContents.ForEachCard(c => c.RemoveComponent<TimerComponent>());

        fuelStorage.SetIsFiring(false);

        stateMachine.ChangeState("未点燃");

        // 只有玩家在同一地点时才停止音效
        if (env == GameManager.Instance.CurEnvironmentBag)
            SoundManager.Instance.StopCardLoopSound(CardId);
    }

    private bool Judge_UnLight(out string hint)
    {
        hint = string.Empty;
        return fuelStorage.isFiring;
    }

    private List<Card> temp = new();

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 没有点燃
        if (!fuelStorage.isFiring) return;

        var waterLevel = StateManager.Instance.WaterLevel.CurValue;

        // 这里剩余燃料一定是>=2的，因为燃料<2时会自动熄灭并且无法点燃
        fuelStorage.AddFuel(-2); // 每回合消耗2点燃料
        if (waterLevel > 0) // 水平面>0时，燃料额外-4
        {
            fuelStorage.AddFuel(-4);
        }

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
                    var outcomeCard = CardFactory.CreateCard(outcomeId);
                    GameManager.Instance.AddCard(outcomeCard, innerContents.bag);
                    outcomeCard.RefreshSlot();
                    ShowTip($"{currentCard.CardName}熟了");
                    currentCard.ShowTip($"{currentCard.CardName}熟了");
                });
            }
        }

        temp.Clear();

        if (fuelStorage.isFiring && fuelStorage.fuel < 2) // 燃料不足时自动熄灭
        {
            Event_UnLight(out _);
            ShowTip("燃料不足，营火已自动熄灭");
            return;
        }

        // 水平面高于30，自动熄灭
        if (fuelStorage.isFiring && waterLevel >= 30)
        {
            Event_UnLight(out _);
            ShowTip("水位过高，营火已自动熄灭");
            return;
        }
    }

    public override bool CanQuickInteract(Card card)
    {
        // 添加燃料
        if (fuelStorage.CanQuickInteract(card)) return true;
        // 放入内容物
        if (innerContents.CanQuickInteract(card)) return true;
        // 拆毁
        return base.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        var card = slot.PeekCard();

        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            fuelStorage.QuickIneract(slot, count, out tip);
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
    public override void OnEnterEnvironment()
    {
        // 只有点燃状态才播放音效
        if (fuelStorage.isFiring)
            SoundManager.Instance.PlayCardLoopSound(CardId, "野炊营火音效", 0.3f);
    }
    public override void OnLeaveEnvironment()
    {
        SoundManager.Instance.StopCardLoopSound(CardId);
    }
    public override void OnDetailOpen()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 1.0f); // 音量调高
    }
    public override void OnDetailClose()
    {
        SoundManager.Instance.SetCardLoopVolume(CardId, 0.3f); // 恢复正常
    }
    public override void DestroyThis()
    {
        OnLeaveEnvironment();
        base.DestroyThis();
    }
}