using System.Collections.Generic;

/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
{
    private const float WATER_LEVEL_REDUCTION = 2f;     // 每回合水平面降低量
    private const float ELECTRICITY_CONSUMPTION = 0.5f; // 每回合电力消耗

    protected override void RegisterCardEvents()
    {
        AddCardEvent("接电", $"将其接入电网。接电后每15分钟消耗{ELECTRICITY_CONSUMPTION}单位电力，降低{WATER_LEVEL_REDUCTION}单位水平面高度", Event_TurnOn, Judge_TurnOn);
        AddCardEvent("断电", "", Event_TurnOff, Judge_TurnOff);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("已开启", "7", true, true, true),
            new ("已关闭", "8", false, true, false),
        };
        stateMachine = new StateMachineComponent("已关闭", states);
        AddComponent(stateMachine);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.AddListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventTrigger, OnMagneticStormBegin);
        EventManager.Instance.RemoveListener<GameEvent>(EventType.OnGameEventEnd, OnMagneticStormEnd);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChange);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    private void OnMagneticStormBegin(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm) || stateMachine.currentStateName == "已关闭") return;

        TurnOff();
        ShowTip($"受行星磁暴影响，{CardName}已断电并停止工作");
    }

    private void OnMagneticStormEnd(GameEvent gameEvent)
    {
        if (gameEvent.GetType() != typeof(MagneticStorm)) return;

        RefreshSlot();
    }

    private void OnElectricityChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity || stateMachine.currentStateName == "已关闭") return;

        if (args.stateValue.GetPredictedVariableValue() < 0) // 已经接电了这里就要判断 < 0，因为 ELECTRICITY_CONSUMPTION 那部分已经包含在 GetPredictedVariableValue 里面了
        {
            TurnOff();
            ShowTip($"电力供应不足，{CardName}已断电并停止工作");
        }
    }

    private void OnWaterLevelChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.WaterLevel || stateMachine.currentStateName == "已关闭") return;

        if (args.stateValue.CurValue <= 0)
        {
            TurnOff();
            ShowTip($"水平面已降至0，{CardName}自动停止工作");
        }
    }

    #region 开关
    private void Event_TurnOn(out string tip, CardEvent e)
    {
        tip = string.Empty;
        TurnOn();
    }

    private void TurnOn()
    {
        StateManager.Instance.ChangeElectricityChangeRate(-ELECTRICITY_CONSUMPTION);
        StateManager.Instance.ChangeWaterLevelChangeRate(-WATER_LEVEL_REDUCTION);
        stateMachine.ChangeState("已开启");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        if (GameEventManager.Instance.IsEventOngoing<MagneticStorm>())
        {
            hint = $"受行星磁暴影响，无法接电";
            return false;
        }

        if (StateManager.Instance.Electricity.GetPredictedVariableValue() < ELECTRICITY_CONSUMPTION)
        {
            hint = "电力供应不足";
            return false;
        }
        
        return stateMachine.currentStateName == "已关闭";
    }

    private void Event_TurnOff(out string tip, CardEvent e)
    {
        tip = string.Empty;
        TurnOff();
    }

    private void TurnOff()
    {
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