using System;
using System.Collections.Generic;

/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
{
    private StateMachineComponent stateMachine;

    private const float WATER_LEVEL_REDUCTION = 2f;     // 每回合水平面降低量
    private const float ELECTRICITY_CONSUMPTION = 0.5f; // 每回合电力消耗

    private ElectricDrainageMachine()
    {
        Events = new()
        {
            new CardEvent("接电", $"将其接入电网。接电后每15分钟消耗{ELECTRICITY_CONSUMPTION}单位电力，降低{WATER_LEVEL_REDUCTION}单位水平面高度", Event_TurnOn, Judge_TurnOn),
            new CardEvent("断电", "", Event_TurnOff, Judge_TurnOff)
        };
    }

    public override void LateConstrcutor()
    {
        base.LateConstrcutor();

        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("已开启", "7", true, true, true),
                new ("已关闭", "8", false, true, false),
            };
            stateMachine = new StateMachineComponent("已关闭", states);
            AddComponent(stateMachine);
        }
    }

    public override void Init()
    {
        base.Init();
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectBegin, OnMagneticStormBegin);
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectEnd, OnMagneticStormEnd);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectBegin, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectEnd, OnMagneticStormEnd);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    private void OnMagneticStormBegin(Type type)
    {
        if (type != typeof(MagneticStorm) || stateMachine.currentStateName == "已关闭") return;

        Event_TurnOff(out _);
        ShowTip($"由于行星磁暴，{CardName}已断电并停止工作");
    }

    private void OnMagneticStormEnd(Type type)
    {
        if (type != typeof(MagneticStorm)) return;

        RefreshSlot();
    }

    private void OnElectricityChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity || stateMachine.currentStateName == "已关闭") return;

        if (args.stateValue.GetPredictedVariableValue() < 0) // 已经接电了这里就要判断 < 0，因为 ELECTRICITY_CONSUMPTION 那部分已经包含在 GetPredictedVariableValue 里面了
        {
            Event_TurnOff(out _);
            ShowTip($"电力供应不足，{CardName}已断电并停止工作");
        }
    }

    private void OnWaterLevelChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.WaterLevel || stateMachine.currentStateName == "已关闭") return;

        if (args.stateValue.CurValue <= 0)
        {
            Event_TurnOff(out _);
            ShowTip($"水平面已降至0，{CardName}自动停止工作");
        }
    }

    #region 开关
    private void Event_TurnOn(out string tip)
    {
        tip = string.Empty;

        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_CONSUMPTION);
        StateManager.Instance.ChangeWaterLevelChangeRate(-WATER_LEVEL_REDUCTION);
        stateMachine.ChangeState("已开启");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            hint = $"由于行星磁暴，无法接电";
            return false;
        }

        if (StateManager.Instance.Electricity.GetPredictedVariableValue() < ELECTRICITY_CONSUMPTION)
        {
            hint = "电力供应不足";
            return false;
        }
        
        return stateMachine.currentStateName == "已关闭";
    }

    private void Event_TurnOff(out string tip)
    {
        tip = string.Empty;

        // 停止工作时，恢复电力和水平面变化率
        StateManager.Instance.ChangeElectricityChangeRate(ELECTRICITY_CONSUMPTION);
        StateManager.Instance.ChangeWaterLevelChangeRate(WATER_LEVEL_REDUCTION);
        stateMachine.ChangeState("已关闭");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已开启";
    }
    #endregion
}