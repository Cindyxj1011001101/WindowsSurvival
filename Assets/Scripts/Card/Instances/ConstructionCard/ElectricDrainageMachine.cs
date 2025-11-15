using System.Collections.Generic;

/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
{
    private const float WATER_LEVEL_REDUCTION_RATE = 2f;    // 每回合水平面降低量
    private const float POWER_CONSUMPTION_RATE = 0.5f;      // 每回合电力消耗

    protected override void RegisterCardEvents()
    {
        AddCardEvent("接电", $"将其接入电网。接电后每15分钟降低{WATER_LEVEL_REDUCTION_RATE}单位水平面高度，并消耗{POWER_CONSUMPTION_RATE}单位电力", powerConsumption.ConnectPower, CanConnectPower);
        AddCardEvent("断电", "", powerConsumption.DisconnectPower, powerConsumption.CanDisconnectPower);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        var states = new List<CardState>()
        {
            new ("已接电", "7", true),
            new ("未接电", "8", false),
        };
        stateMachine = new StateMachineComponent("未接电", states);
        AddComponent(stateMachine);

        powerConsumption = new(POWER_CONSUMPTION_RATE);
        AddComponent(powerConsumption);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnWaterLevelChange);
    }

    private bool CanConnectPower(out string s)
    {
        if (StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            s = "水平面已降至0";
            return false;
        }

        return powerConsumption.CanConnectPower(out s);
    }

    /// <summary>
    /// 接电事件
    /// </summary>
    private void PowerOn()
    {
        // 接电后水平面每回合下降
        StateManager.Instance.ChangeWaterLevelChangeRate(-WATER_LEVEL_REDUCTION_RATE);
        stateMachine.ChangeState("已接电");
    }

    /// <summary>
    /// 断电事件
    /// </summary>
    private void PowerOff()
    {
        StateManager.Instance.ChangeWaterLevelChangeRate(+WATER_LEVEL_REDUCTION_RATE);
        stateMachine.ChangeState("未接电");
    }

    private void OnWaterLevelChange(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.WaterLevel || !powerConsumption.Connected) return;

        if (args.stateValue.CurValue <= 0)
        {
            powerConsumption.DisconnectPower();
            ShowTip($"水平面已降至0，{CardName}已自动断电");
        }
    }
}