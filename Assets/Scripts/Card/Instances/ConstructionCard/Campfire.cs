using System.Collections.Generic;

/// <summary>
/// 野炊营火
/// </summary>
public class Campfire : ConstructionCard
{
    public override bool HasLoopSound => true;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "点燃营火。可以对部分食物进行简单的烧烤。\n点燃状态下会导致室内氧气加速消耗与一氧化碳增加", fuelStorage.Ignite, fuelStorage.CanIgnite);
        AddCardEvent("熄灭", "", fuelStorage.Extinguish, fuelStorage.CanExtinguish);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        // 手动添加燃料存储组件
        fuelStorage = new FuelStorageComponent(96);
        AddComponent(fuelStorage);

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }

        // 添加状态机组件
        var states = new List<CardState>()
        {
            new ("未点燃", "18"),
            new ("已点燃", "18", true),
        };
        stateMachine = new StateMachineComponent("未点燃", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        // 放入内容物时，暂停卡牌每回合更新
        innerContents.onAddCard = (c) =>
        {
            if (fuelStorage.isBurning)
            {
                c.FreezeUpdate();
                c.TryGetComponent<CookComponent>(out var cook);
                if (cook.leftCookTime < 0) return;

                var timer = new TimerComponent(cook.leftCookTime, cook.totalCookTime);
                if (cook.outcomeCardId == "烧焦的食物")
                    timer.tipText = "烧焦";
                else
                    timer.tipText = "烤熟";

                c.AddComponent(timer);
            }
        };

        // 取出时恢复每回合更新
        innerContents.onRemoveCard = (c) =>
        {
            c.UnfreezeUpdate();
            c.RemoveComponent<TimerComponent>();
        };
    }

    /// <summary>
    /// 点燃时触发
    /// </summary>
    private void OnIgnite()
    {
        // 音效
        PlaySound("点火_02");

        // 只有玩家在同一地点且点燃时才播放循环音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.PlayCardLoopSound(CardId, "野炊营火音效", 0.3f);

        // 点燃后暂停所有卡牌每回合更新
        innerContents.FreezeUpdate();

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
            c.RefreshSlot();
        });

        stateMachine.ChangeState("已点燃");
    }

    /// <summary>
    /// 熄灭时触发
    /// </summary>
    private void OnExtinguish()
    {
        // 只有玩家在同一地点时才停止音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);

        // 熄灭后恢复所有卡牌每回合更新
        innerContents.UnfreezeUpdate();

        // 移除计时器组件
        innerContents.ForEachCard(c =>
        {
            c.RemoveComponent<TimerComponent>();
            c.RefreshSlot();
        });

        stateMachine.ChangeState("未点燃");
    }

    /// <summary>
    /// 点燃时每回合触发
    /// </summary>
    private void OnBurning()
    {
        // 内容物增加烹饪进度
        foreach (var card in innerContents.GetAllCards())
        {
            card.TryGetComponent(out CookComponent cook);
            cook.Cook();

            if (card.TryGetComponent<TimerComponent>(out var timer) && cook.leftCookTime >= 0)
            {
                timer.SetValue(cook.leftCookTime);
            }
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

    public override bool CanQuickInteract(Card card, out string tip)
    {
        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            tip = "添加燃料";
			return true;
		}
        // 放入内容物
        if (innerContents.CanQuickInteract(card, out tip)) return true;
        // 拆毁
        return base.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        var card = slot.PeekCard();

        // 添加燃料
        if (fuelStorage.CanQuickInteract(card))
        {
            fuelStorage.QuickIneract(slot, count);
            return;
        }

        // 放入内容物
        if (innerContents.CanQuickInteract(card, out _))
        {
            innerContents.QuickIneract(slot, count);
            return;
        }

        // 拆毁
        base.QuickIneract(slot, count);
    }
    public override void OnEnterEnvironment()
    {
        // 只有点燃状态才播放音效
        if (fuelStorage.isBurning)
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
}