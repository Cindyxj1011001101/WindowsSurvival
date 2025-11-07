using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : ConstructionCard
{
    private const float ELECTRICITY_CONSUMPTION = 0.6f; // 每回合耗电量
    private const float EXTRA_SOBRIETY_INCREASE = 1.2f; // 额外清醒度增加
    private const float EXTRA_HEALTH_INCREASE = 1.2f;   // 额外清醒度增加

    [JsonProperty] private bool isWorking = false;

    protected override void RegisterCardEvents()
    {
        AddCardEvent("接电", $"将其接入电网。接电后当麦麦在安装了{CardName}的地点休息时，每15分钟额外+{EXTRA_SOBRIETY_INCREASE}清醒度和{EXTRA_HEALTH_INCREASE}健康，" +
                            $"并消耗{ELECTRICITY_CONSUMPTION}单位电力", Event_TurnOn, Judge_TurnOn);
		AddCardEvent("断电", "", Event_TurnOff, Judge_TurnOff);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("已接电", "11", true, true, true),
            new ("未接电", "12", false, true, false),
        };
        stateMachine = new StateMachineComponent("未接电", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, OnStopSleeping);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
    }

    private void OnMagneticStormBegin(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm) || stateMachine.currentStateName == "未接电") return;

        Event_TurnOff(out _);
        ShowTip($"受行星磁暴影响，{CardName}已断电并停止工作");
    }

    private void OnMagneticStormEnd(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        RefreshSlot();
    }

    private void OnElectricityChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity || !isWorking) return;

        if (args.stateValue.GetPredictedVariableValue() < 0) // 已经接电了这里就要判断 < 0，因为 ELECTRICITY_CONSUMPTION 那部分已经包含在 GetPredictedVariableValue 里面了
        {
            Event_TurnOff(out _);
            ShowTip($"电力供应不足，{CardName}已断电并停止工作");
        }
    }

    private void OnStartSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || stateMachine.currentStateName == "未接电" || isWorking) return;

        // 开始睡觉时判断电力是否充足
        if (StateManager.Instance.Electricity.GetPredictedVariableValue() < ELECTRICITY_CONSUMPTION)
        {
            Event_TurnOff(out _);
            ShowTip($"电力供应不足，{CardName}已断电并停止工作");
            return;
        }

        StartWorking();
    }

    private void OnStopSleeping()
    {
        if (!isWorking) return;

        StopWorking();
    }

    private void StartWorking()
    {
        isWorking = true;
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, EXTRA_SOBRIETY_INCREASE);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, EXTRA_HEALTH_INCREASE);
        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_CONSUMPTION);
    }

    private void StopWorking()
    {
        isWorking = false;
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
        return stateMachine.currentStateName == "未接电";
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_TurnOff(out string tip)
    {
        tip = string.Empty;
		if (isWorking)
			StopWorking();
		stateMachine.ChangeState("未接电");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }
}