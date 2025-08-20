/// <summary>
/// 睡眠脉冲仪
/// </summary>
public class SleepInstrument : Card
{
    public bool isConnected = false; // 是否已接电
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
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChanged);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityChanged);
    }

    private void OnElectricityChanged(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity) return;
        if (args.stateValue.CurValue <= 0 && isConnected)
        {
            isConnected = false;
            ShowTip("电力不足，睡眠脉冲仪已自动断电");
            StopWorking();
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
        }
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
        StateManager.Instance.ChangeElectricityChangeRate(-.6f);
    }

    private void StopWorking()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -1.2f);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, -1f);
        StateManager.Instance.ChangeElectricityChangeRate(.6f);
    }

    private void Event_ConnectElectricity(out string tip)
    {
        tip = string.Empty;
        isConnected = true;
    }

    private bool Judge_ConnectElectricity(out string hint)
    {
        hint = string.Empty;

        if (StateManager.Instance.Electricity.CurValue <= 0)
        {
            hint = "电力不足";
            return false;
        }

        return !isConnected;
    }
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
}