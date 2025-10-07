using System.Collections.Generic;

/// <summary>
/// 燃料炉
/// </summary>
public class FuelFurnace : ConstructionCard
{
    public override string ExtraInfo
    {
        get
        {
            if (processComplished) return "加工完成";
            else if (isProcessing) return "加工中";
            else return base.ExtraInfo;
        }
    }

    private InnerContentsComponent innerContents;
    private FuelStorageComponent fuelStorage;
    private TemperatureComponent temperatureComponent;

    public List<Card> cardsToProcesss = new(); // 待加工卡牌
    public int leftRounds = 0; // 当前加工轮数
    public int maxRounds = 16; // 总加工轮数
    public bool isProcessing = false; // 是否正在加工
    public Dictionary<TemperatureType, int> temperatureRecord = new(); // 温度数据，用来处理产物获取
    public bool processComplished = false;
    public override bool HasLoopSound => true;

    private FuelFurnace()
    {
        Events = new()
        {
            new Event("开始加工" , "", Event_Process, Judge_Process),
        };
    }

    public override void Awake()
    {
        base.Awake();
        // 添加燃料存储组件
        if (!TryGetComponent(out fuelStorage))
        {
            fuelStorage = new FuelStorageComponent(96, 1);
            AddComponent(fuelStorage);
        }
        fuelStorage.actionWhileBurning = () =>
        {
            // 点燃时，每回合温度增加
            temperatureComponent.AddValue(17); // 温度+17
        };

        fuelStorage.actionWhileNotBurning = () =>
        {
            // 熄灭时，每回合温度减少
            var waterLevel = StateManager.Instance.WaterLevel.CurValue;
            temperatureComponent.AddValue(-4); // 温度-4
            if (waterLevel >= 30) // 水平面>=30时，温度额外-8
            {
                temperatureComponent.AddValue(-8);
            }
            else if (waterLevel > 0) // 水平面>=0时，温度额外-4
            {
                temperatureComponent.AddValue(-4);
            }
        };

        fuelStorage.actionOnIgnite = () =>
        {
            SoundManager.Instance.PlaySound("点火_02");
            if (GameManager.Instance.IsCurrentEnvironment(Bag))
                SoundManager.Instance.PlayCardLoopSound(CardId, "燃料炉音效", 1f);
        };

        fuelStorage.actionOnExtinguish = () =>
        {
            if (GameManager.Instance.IsCurrentEnvironment(Bag))
                SoundManager.Instance.StopCardLoopSound(CardId);
        };
        // 添加点燃熄灭事件
        fuelStorage.AddEvents("点燃燃料炉。点燃后可以使燃料炉快速升温");

        // 添加温度组件
        if (!TryGetComponent(out temperatureComponent))
        {
            temperatureComponent = new TemperatureComponent(0, 300);
            AddComponent(temperatureComponent);
        }

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }

        innerContents.onRemoveCard = (c) =>
        {
            if (processComplished && innerContents.bag.IsEmpty)
            {
                processComplished = false;
                // 恢复内容物的可放入
                innerContents.allowAdd = true;
                RefreshSlot();
            }
        };
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (!c.TryGetComponent<FoodPropertyComponent>(out _))
        {
            s = "只能放入可加工的卡牌";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 开始加工
    /// </summary>
    /// <param name="tip"></param>
    private void Event_Process(out string tip)
    {
        tip = string.Empty;

        isProcessing = true;
        leftRounds = maxRounds;

        // 暂停内容物的更新
        innerContents.PauseUpdating();

        // 不可以添加或移除内容物
        innerContents.allowAdd = innerContents.allowRemove = false;
        innerContents.notAllowRemoveReason = "加工中，不能移除待加工物";
        innerContents.notAllowAddReason = "加工中，不能添加待加工物";

        // 记录当前炉内的卡牌，即需要加工的卡牌
        foreach (var slot in innerContents.bag.Slots)
        {
            foreach (var card in slot.Cards)
            {
                cardsToProcesss.Add(card);
            }
        }
        // 记录温度状态
        temperatureRecord = new()
        {
            { TemperatureType.Normal, 0 },
            { TemperatureType.Low, 0 },
            { TemperatureType.Medium, 0 },
            { TemperatureType.High, 0 },
        };

        // 添加计时器组件
        AddComponent(new TimerComponent(maxRounds * TimeManager.Instance.SettleInterval) { tipText = "加工完成" });
        RefreshSlot();
    }

    private bool Judge_Process(out string hint)
    {
        hint = string.Empty;

        if (isProcessing)
        {
            hint = "正在加工中";
            return false;
        }

        if (processComplished)
        {
            hint = "请先取出加工产物";
            return false;
        }

        if (!innerContents.bag.IsFull)
        {
            hint = "燃料炉内必须放满待加工物才能加工";
            return false;
        }

        return true;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        HandleProcessRound();
    }

    /// <summary>
    /// 处理每回合加工进度变化
    /// </summary>
    private void HandleProcessRound()
    {
        if (!isProcessing || leftRounds <= 0) return;

        // 记录温度数据
        if (temperatureComponent.value <= 50) temperatureRecord[TemperatureType.Normal]++;
        else if (temperatureComponent.value <= 100) temperatureRecord[TemperatureType.Low]++;
        else if (temperatureComponent.value <= 200) temperatureRecord[TemperatureType.Medium]++;
        else temperatureRecord[TemperatureType.High]++;

        // 剩余回合数-1
        leftRounds--;

        // 刷新计时器
        if (TryGetComponent<TimerComponent>(out var timer))
        {
            timer.SetValue(leftRounds * TimeManager.Instance.SettleInterval);
        }

        // 加工完成
        if (leftRounds <= 0)
        {
            // 得到产物
            var outcomeCardId = ProcessManager.GetProcessOutcomeID(cardsToProcesss, temperatureRecord);
            
            leftRounds = 0;
            isProcessing = false;
            temperatureRecord.Clear();
            innerContents.Clear(); // 销毁内容物
            cardsToProcesss.Clear(); // 清空加工列表

            processComplished = true; // 加工完成

            // 可拖出卡牌
            innerContents.allowRemove = true;
            innerContents.notAllowAddReason = "请先取出加工产物";
            
            // 添加产物
            var outcomeCard = CardFactory.CreateCard(outcomeCardId);
            GameManager.Instance.AddCard(outcomeCard, innerContents.bag);
            outcomeCard.RefreshSlot();

            ShowTip("燃料炉加工完成");

            // 移除计时器
            RemoveComponent<TimerComponent>();
            RefreshSlot();
        }
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
        if (fuelStorage.isBurning)
            SoundManager.Instance.PlayCardLoopSound(CardId, "燃料炉音效", 0.3f);
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