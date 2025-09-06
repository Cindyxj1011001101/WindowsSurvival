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

    }

    public override void Awake()
    {
        base.Awake();
        // 手动添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96, 2);
            AddComponent(fuelStorage);
        }

        fuelStorage.actionWhileBurning = WhileBurning;

        fuelStorage.actionOnIgnite = OnIgnite;

        fuelStorage.actionOnExtinguish = OnExtinguish;

        // 添加点燃熄灭事件
        fuelStorage.AddEvents("点燃营火。可以对部分食物进行简单的烧烤。\n点燃状态下会导致室内氧气消耗与一氧化碳增加");

        // 放入内容物时，暂停卡牌每回合更新
        innerContents.onAddCard = (c) =>
        {
            if (fuelStorage.isBurning)
            {
                c.PauseUpdating();
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
            c.ContinueUpdating();
            c.RemoveComponent<TimerComponent>();
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

    /// <summary>
    /// 点燃时触发
    /// </summary>
    private void OnIgnite()
    {
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
            c.RefreshSlot();
        });

        stateMachine.ChangeState("已点燃");

        SoundManager.Instance.PlaySound("点火_02");

        // 只有玩家在同一地点且点燃时才播放循环音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.PlayCardLoopSound(CardId, "野炊营火音效", 0.3f);
    }

    /// <summary>
    /// 熄灭时触发
    /// </summary>
    private void OnExtinguish()
    {
        // 熄灭后恢复所有卡牌每回合更新
        innerContents.ContinueUpdating();

        // 移除计时器组件
        innerContents.ForEachCard(c =>
        {
            c.RemoveComponent<TimerComponent>();
            c.RefreshSlot();
        });

        stateMachine.ChangeState("未点燃");

        // 只有玩家在同一地点时才停止音效
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);
    }

    private List<Card> temp = new();

    /// <summary>
    /// 点燃时每回合触发
    /// </summary>
    private void WhileBurning()
    {
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
            if (card == null || card.Destroyed || !card.TryGetComponent(out CookComponent cook)) continue;

            // 使用局部变量捕获当前的值
            Card currentCard = card;

            cook.Update(TimeManager.Instance.SettleInterval, (outcomeId) =>
            {
                // 处理煮熟的逻辑
                currentCard.DestroyThis();
                var outcomeCard = CardFactory.CreateCard(outcomeId);
                GameManager.Instance.AddCard(outcomeCard, innerContents.bag);
                outcomeCard.RefreshSlot();
                if (outcomeId == "烧焦的食物")
                {
                    ShowTip($"{currentCard.CardName}烧焦了");
                    currentCard.ShowTip($"{currentCard.CardName}烧焦了");
                }
                else
                {
                    ShowTip($"{currentCard.CardName}熟了");
                    currentCard.ShowTip($"{currentCard.CardName}熟了");
                }
            });


            if (currentCard.TryGetComponent<TimerComponent>(out var timer) && cook.leftCookTime >= 0)
            {
                timer.SetValue(cook.leftCookTime);
            }
        }

        temp.Clear();
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
        if (innerContents.CanQuickInteract(card, out _))
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