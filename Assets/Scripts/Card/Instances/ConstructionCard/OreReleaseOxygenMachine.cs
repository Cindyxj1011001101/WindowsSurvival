using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矿石释氧机
/// </summary>
public class OreReleaseOxygenMachine : ConstructionCard
{
    private const int MAX_PRODUCTION_PROCESS = 120;  // 最大氧气产生进度
    private const int OXYGEN_PRODUCTION = 180;       // 氧气产出
    private const int ORE_CONSUMPTION = 1;           // 白爆矿消耗量
    private const float ELECTRICITY_CONSUMPTION = 1; // 电力消耗量

    [JsonProperty] private int leftProductionProgress = MAX_PRODUCTION_PROCESS;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("开启", $"开启后{CardName}每{MAX_PRODUCTION_PROCESS}分钟消耗{ORE_CONSUMPTION}块白爆矿和{ELECTRICITY_CONSUMPTION}单位电力，产生{OXYGEN_PRODUCTION}单位氧气", Event_TurnOn, Judge_TurnOn);
        AddCardEvent("关闭", "", Event_TurnOff, Judge_TurnOff);
        AddCardEvent("获取氧气", $"消耗{CardName}的氧气储存，补充麦麦的氧气", oxygenStorage.Event_GetOxygen, oxygenStorage.Judge_GetOxygen);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        oxygenStorage = new OxygenStorageComponent(360);
        AddComponent(oxygenStorage);

        var states = new List<CardState>()
        {
            new ("已关闭", "0", false, true, false),
            new ("已开启", "1", true, true, true),
        };
        stateMachine = new StateMachineComponent("已关闭", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
    }

    private void OnMagneticStormBegin(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        Event_TurnOff(null);
        ShowTip($"受行星磁暴影响，{CardName}已关闭并停止工作");
    }

    private void OnMagneticStormEnd(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        RefreshSlot();
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (c.CardId != "白爆矿")
        {
            s = "只能放入白爆矿";
            return false;
        }
        return true;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        if (base.CanQuickInteract(card, out tip)) return true;

        return innerContents.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count)
    {
        if (base.CanQuickInteract(slot.PeekCard(), out _))
        {
            base.QuickIneract(slot, count);
            return;
        }

        innerContents.QuickIneract(slot, count);
    }

    #region 开关
    private void Event_TurnOn(CardEvent e)
    {
        // 添加计时器组件
        var timer = new TimerComponent(leftProductionProgress, MAX_PRODUCTION_PROCESS)
        {
            tipText = "下次制氧"
        };
        AddComponent(timer);

		stateMachine.ChangeState("已开启");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            hint = $"受行星磁暴影响，{CardName}缺少电力供应，无法开启";
            return false;
        }
        return stateMachine.currentStateName == "已关闭";
    }

    private void Event_TurnOff(CardEvent e)
    {
        RemoveComponent<TimerComponent>();
        stateMachine.ChangeState("已关闭");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已开启";
    }
    #endregion

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 先制氧
        GenerateOxygen();
        // 给室内环境充气
        ReleaseOxygen();
    }

    // 释放氧气
    private void ReleaseOxygen()
    {
        var env = Bag as EnvironmentBag;
        // 不是室内环境不释放氧气
        if (!env.PlaceData.isIndoor) return;

        // 室内氧气剩余容量
        var remainingCapacity = env.StateDict[EnvironmentStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(oxygenStorage.value, remainingCapacity);
        if (toRelease > 0)
        {
            // 释放氧气
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, toRelease);
            // 氧气存量减少
            oxygenStorage.AddValue(-toRelease);
            ShowTip($"{CardName}向当前地点释放了 " + toRelease + " 单位的氧气");
        }
    }

    // 制氧
    private void GenerateOxygen()
    {
        // 不在工作状态不制氧
        if (stateMachine.currentStateName == "已关闭")
        {
            return;
        }

        // 剩余制氧进度减少
        leftProductionProgress -= TimeManager.SETTLEMENT_INTERVAL;

		if (TryGetComponent<TimerComponent>(out var timer))
        {
            timer.SetValue(leftProductionProgress);
        }

        // 进度不满不制氧
        if (leftProductionProgress > 0)
        {
            return;
        }

        // 氧气存储要超了不制氧
        if (oxygenStorage.value + OXYGEN_PRODUCTION > oxygenStorage.maxValue)
        {
            return;
        }

        // 没连接到电网不制氧
        var env = Bag as EnvironmentBag;
        if (!env.HasCable)
        {
            return;
        }

        // 电力供应不足不制氧
        if (StateManager.Instance.Electricity.GetPredictedVariableValue() < ELECTRICITY_CONSUMPTION)
        {
            return;
        }

        // 白爆矿不够不制氧
        if (!TryConsumeOre(ORE_CONSUMPTION))
        {
            return;
        }

        // 重置生产进度
        leftProductionProgress = MAX_PRODUCTION_PROCESS;
        timer?.SetValue(leftProductionProgress);

        // 消耗电力
        StateManager.Instance.ChangeElectricity(-ELECTRICITY_CONSUMPTION);

        // 氧气存量增加
        oxygenStorage.AddValue(OXYGEN_PRODUCTION);
    }

    private bool TryConsumeOre(int amount)
    {
        int oreCount = innerContents.GetTotalCountByCardId("白爆矿");
        // 白爆矿的数量多于消耗量
        if (oreCount >= amount)
        {
            innerContents.DestroyCardsByCardId("白爆矿", amount);
            return true;
        }
        return false;
    }
}