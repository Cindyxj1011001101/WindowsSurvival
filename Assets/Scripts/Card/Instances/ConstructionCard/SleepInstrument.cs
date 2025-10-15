using System;
using System.Collections.Generic;

/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : ConstructionCard
{
    private StateMachineComponent stateMachine;

    private const float ELECTRICITY_CONSUMPTION = 0.6f; // 每回合耗电量
    private const float EXTRA_SOBRIETY_INCREASE = 1.2f; // 额外清醒度增加
    private const float EXTRA_HEALTH_INCREASE = 1.2f;   // 额外清醒度增加

    private SleepInstrument()
    {
        Events = new()
        {
            new CardEvent("开启", $"使其接入电路。接入电路后当麦麦在安装了睡眠脉冲仪的地点休息，休息时每15分钟额外+{EXTRA_SOBRIETY_INCREASE}清醒度和{EXTRA_HEALTH_INCREASE}健康，" +
                            $"并消耗{ELECTRICITY_CONSUMPTION}单位电力",Event_TurnOn, Judge_TurnOn),
            new CardEvent("关闭", "", Event_TurnOff, Judge_TurnOff),
        };
    }

    public override void LateConstrcutor()
    {
        base.LateConstrcutor();

        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("已接电", "11", true, true, true),
                new ("未接电", "12", false, true, false),
            };
            stateMachine = new StateMachineComponent("未接电", states);
            AddComponent(stateMachine);
        }
    }

    public override void Init()
    {
        base.Init();
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, OnStopSleeping);
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectBegin, OnElectromagneticInterferenceBegin);
        EventManager.Instance.AddListener<Type>(EventType.OnGlobalEffectEnd, OnElectromagneticInterferenceEnd);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectBegin, OnElectromagneticInterferenceBegin);
        EventManager.Instance.RemoveListener<Type>(EventType.OnGlobalEffectEnd, OnElectromagneticInterferenceEnd);
    }

    private void OnElectromagneticInterferenceBegin(Type type)
    {
        if (type != typeof(PowerNetworkFailure)) return;

        StopWorking();
        stateMachine.ChangeState("未接电");
        ShowTip($"由于电网故障，{CardName}已停止工作");
    }

    private void OnElectromagneticInterferenceEnd(Type type)
    {
        if (type != typeof(PowerNetworkFailure)) return;

        RefreshSlot();
    }


    private void OnStartSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || stateMachine.currentStateName == "未接电") return;

        StartWorking();
    }

    private void OnStopSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || stateMachine.currentStateName == "未接电") return;

        StopWorking();
    }

    private void StartWorking()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, EXTRA_SOBRIETY_INCREASE);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, EXTRA_HEALTH_INCREASE);
        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_CONSUMPTION);
    }

    private void StopWorking()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -EXTRA_SOBRIETY_INCREASE);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, -EXTRA_HEALTH_INCREASE);
        StateManager.Instance.ChangeElectricityChangeRate(+ELECTRICITY_CONSUMPTION);
    }

    /// <summary>
    /// 接电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TurnOn(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("已接电");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue < ELECTRICITY_CONSUMPTION)
        {
            hint = "电力不足";
            return false;
        }

        return stateMachine.currentStateName == "未接电";
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TurnOff(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("未接电");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (stateMachine.currentStateName == "未接电") return;

        if (StateManager.Instance.Electricity.CurValue < ELECTRICITY_CONSUMPTION)
        {
            StopWorking();
            stateMachine.ChangeState("未接电");
            ShowTip($"电力不足，{CardName}已停止工作");
        }
    }
}