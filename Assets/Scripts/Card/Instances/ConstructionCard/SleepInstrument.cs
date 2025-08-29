using System.Collections.Generic;

/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : ConstructionCard
{
    private StateMachineComponent stateMachine;

    public float electricityConsume = .6f; // 每回合耗电量
    private SleepInstrument()
    {
        Events = new()
        {
            new Event("接电", "使其接入电路。接入电路后如果玩家在安装了睡眠脉冲仪的地点休息，休息时每15分钟额外+1.2清醒度和1健康，并消耗0.6电力", Event_ConnectElectricity, Judge_ConnectElectricity),
            new Event("断电", "", Event_DisconnectElectricity, Judge_DisconnectElectricity),
        };
    }

    public override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, OnStopSleeping);

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

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
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
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, +1.2f);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, +1f);
        StateManager.Instance.ChangeElectricityChangeRate(-electricityConsume);
    }

    private void StopWorking()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -1.2f);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, -1f);
        StateManager.Instance.ChangeElectricityChangeRate(+electricityConsume);
    }

    /// <summary>
    /// 接电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_ConnectElectricity(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("已接电");
    }

    private bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
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
    private void Event_DisconnectElectricity(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("未接电");
    }

    private bool Judge_DisconnectElectricity(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已接电";
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (stateMachine.currentStateName == "未接电") return;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            stateMachine.ChangeState("未接电");
            StopWorking();
            ShowTip("电力不足，睡眠脉冲仪已自动断电");
        }
    }
}