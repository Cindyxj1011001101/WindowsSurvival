using Newtonsoft.Json;
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

    private const int MAX_PROCESS_ROUNDS = 16; // 总加工轮数

    [JsonProperty] private int leftProcessRounds = MAX_PROCESS_ROUNDS;                  // 剩余加工轮数
    [JsonProperty] private bool isProcessing = false;                                   // 是否正在加工
    [JsonProperty] private bool processComplished = false;                              // 加工是否完成
    [JsonProperty] private Dictionary<TemperatureType, int> temperatureRecord = new();  // 温度数据，用来处理产物获取

    public override bool HasLoopSound => true;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("点燃", "点燃燃料炉。点燃后可以使燃料炉快速升温。\n点燃状态下会导致室内氧气加速消耗与一氧化碳增加", Ignite, fuelStorage.CanIgnite);
        AddCardEvent("熄灭", "", Extinguish, fuelStorage.CanExtinguish);
        AddCardEvent("开始加工", "", Event_Process, Judge_Process);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        // 添加燃料存储组件
        fuelStorage = new FuelStorageComponent(96);
        AddComponent(fuelStorage);

        // 添加温度组件
        temperature = new TemperatureComponent(0, 300);
        AddComponent(temperature);

        // 每个卡牌槽的最大堆叠数都为1
        foreach (var slot in innerContents.bag.Slots)
        {
            slot.SetMaxStackNum(1);
        }

        var states = new List<CardState>()
        {
            new ("未加工", "5"),
            new ("加工中", "6"),
        };
        stateMachine = new StateMachineComponent("未加工", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        fuelStorage.whileBurning = () =>
        {
            // 点燃时，每回合温度增加
            temperature.AddValue(17); // 温度+17
        };

        fuelStorage.whileNotBurning = () =>
        {
            // 熄灭时，每回合温度减少
            var waterLevel = StateManager.Instance.WaterLevel.CurValue;
            temperature.AddValue(-4); // 温度-4
            if (waterLevel >= 30) // 水平面>=30时，温度额外-8
            {
                temperature.AddValue(-8);
            }
            else if (waterLevel > 0) // 水平面>=0时，温度额外-4
            {
                temperature.AddValue(-4);
            }
        };

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

    private void Ignite(out string s, CardEvent e)
    {
        PlaySound("点火_02");
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.PlayCardLoopSound(CardId, "燃料炉音效", 1f);

        fuelStorage.Ignite(out s);
    }

    private void Extinguish(out string s, CardEvent e)
    {
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);

        fuelStorage.Extinguish(out s);
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
    private void Event_Process(out string tip, CardEvent e)
    {
        tip = string.Empty;

        isProcessing = true;
        leftProcessRounds = MAX_PROCESS_ROUNDS;

        // 暂停内容物的更新
        innerContents.PauseUpdating();

        // 不可以添加或移除内容物
        innerContents.allowAdd = innerContents.allowRemove = false;
        innerContents.notAllowRemoveReason = "加工中，不能移除待加工物";
        innerContents.notAllowAddReason = "加工中，不能添加待加工物";

        // 记录温度状态
        temperatureRecord = new()
        {
            { TemperatureType.Normal, 0 },
            { TemperatureType.Low,    0 },
            { TemperatureType.Medium, 0 },
            { TemperatureType.High,   0 },
        };

        // 添加计时器组件
        AddComponent(new TimerComponent(MAX_PROCESS_ROUNDS * TimeManager.SETTLEMENT_INTERVAL) { tipText = "加工完成" });

        stateMachine.ChangeState("加工中");
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
        if (!isProcessing || leftProcessRounds <= 0) return;

        // 记录温度数据
        if (temperature.value <= 50)
            temperatureRecord[TemperatureType.Normal]++;    // 正常
        else if (temperature.value <= 100)
            temperatureRecord[TemperatureType.Low]++;       // 低温
        else if (temperature.value <= 200)
            temperatureRecord[TemperatureType.Medium]++;    // 中温
        else
            temperatureRecord[TemperatureType.High]++;      // 高温

        // 剩余回合数-1
        leftProcessRounds--;

        // 刷新计时器
        if (TryGetComponent<TimerComponent>(out var timer))
        {
            timer.SetValue(leftProcessRounds * TimeManager.SETTLEMENT_INTERVAL);
        }

        // 加工完成
        if (leftProcessRounds <= 0)
        {
            // 得到产物
            var processedCards = new List<Card>()
            {
                innerContents.bag.Slots[0].PeekCard(),
                innerContents.bag.Slots[1].PeekCard(),
                innerContents.bag.Slots[2].PeekCard(),
            };
            var outcomeCardId = ProcessManager.Instance.GetProcessOutcomeID(processedCards, temperatureRecord);
            
            leftProcessRounds = 0;
            isProcessing = false;
            temperatureRecord.Clear();
            innerContents.Clear(); // 销毁内容物

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

            stateMachine.ChangeState("未加工");
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