using System;

/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : ConstructionCard
{
    public bool isConnected = false; // 是否已接电
    public float electricityConsume = .6f; // 每回合耗电量
    private SleepInstrument()
    {
        Events = new()
        {
            new Event("接电", "", Event_ConnectElectricity, Judge_ConnectElectricity),
            new Event("断电", "", Event_DisconnectElectricity, Judge_DisconnectElectricity),
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, OnStopSleeping);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
    }


    private void OnStartSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || !isConnected) return;

        StartWorking();
    }

    private void OnStopSleeping()
    {
        if (GameManager.Instance.CurEnvironmentBag != Bag || !isConnected) return;

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
        isConnected = true;
    }

    private bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            hint = "电力不足";
            return false;
        }

        return !isConnected;
    }

    /// <summary>
    /// 断电
    /// </summary>
    /// <param name="tip"></param>
    private void Event_DisconnectElectricity(out string tip)
    {
        tip = string.Empty;
        isConnected = false;
    }

    private bool Judge_DisconnectElectricity(out string hint)
    {
        hint = string.Empty;
        return isConnected;
    }

    protected override Action OnUpdate => () =>
    {
        if (!isConnected) return;

        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            isConnected = false;
            StopWorking();
            RefreshSlot();
            ShowTip("电力不足，睡眠脉冲仪已自动断电");
        }
    };
}