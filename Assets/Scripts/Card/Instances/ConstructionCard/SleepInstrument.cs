/// <summary>
/// 睡眠脉冲仪
/// </summary>
[CardId("睡眠脉冲仪")]
public class SleepInstrument : ConstructionCard
{
    private const float POWER_CONSUMPTION_RATE = 0.6f;          // 每回合耗电量
    private const float EXTRA_SOBRIETY_INCREASE_RATE = 1.2f;    // 额外清醒度增加
    private const float EXTRA_HEALTH_INCREASE_RATE = 1.2f;      // 额外精神值增加

    protected override void RegisterCardEvents()
    {
        AddCardEvent("开启", $"开启机器。开启后当麦麦在安装了{CardName}的地点休息时，机器会自动接电，使麦麦每15分钟额外回复{EXTRA_SOBRIETY_INCREASE_RATE}清醒度和{EXTRA_HEALTH_INCREASE_RATE}健康，" +
                            $"并消耗{POWER_CONSUMPTION_RATE}单位电力", Event_TurnOn, Judge_TurnOn);
		AddCardEvent("断电", "", Event_TurnOff, Judge_TurnOff);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
        powerConsumption = new(POWER_CONSUMPTION_RATE);
        AddComponent(powerConsumption);
    }

    protected override void OnInit()
    {
        EventManager.Instance.AddListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.AddListener(EventType.StopSleeping, OnStopSleeping);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.StartSleeping, OnStartSleeping);
        EventManager.Instance.RemoveListener(EventType.StopSleeping, OnStopSleeping);
    }

    private void PowerOn()
    {
        // 额外恢复清醒度和精神值
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, EXTRA_SOBRIETY_INCREASE_RATE);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, EXTRA_HEALTH_INCREASE_RATE);
    }

    private void PowerOff()
    {
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, -EXTRA_SOBRIETY_INCREASE_RATE);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, -EXTRA_HEALTH_INCREASE_RATE);
    }

    private void OnStartSleeping()
    {
        // 未开启机器
        if (stateMachine.currentStateName == "未开启") return;

        // 玩家不在机器所在地点休息
        if (!GameManager.Instance.IsCurrentEnvironment(Bag)) return;

        // 可以接电，则接电
        if (powerConsumption.CanConnectPower(out _))
        {
            powerConsumption.ConnectPower();
        }
    }

    private void OnStopSleeping()
    {
        if (!powerConsumption.Connected) return;

        powerConsumption.DisconnectPower();
        ShowTip($"睡眠结束，{CardName}已自动断电");
    }

    private void Event_TurnOn(CardEvent e)
    {
        stateMachine.ChangeState("已开启");
    }

    private bool Judge_TurnOn(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "未开启";
    }

    private void Event_TurnOff(CardEvent e)
    {
        stateMachine.ChangeState("未开启");
    }

    private bool Judge_TurnOff(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已开启";
    }
}