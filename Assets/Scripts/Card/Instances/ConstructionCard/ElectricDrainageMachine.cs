/// <summary>
/// 电动排水机
/// </summary>
[CardId("电动排水机")]
public class ElectricDrainageMachine : ConstructionCard
{
    // 表示该卡牌具有循环音效
    // SoundManager 会根据玩家是否在同一环境、以及详情窗口打开状态来播放/调节该卡牌的循环音效
    public override bool HasLoopSound => true;
    private const float WATER_LEVEL_REDUCTION_RATE = 2f;    // 每回合水平面降低量
    private const float POWER_CONSUMPTION_RATE = 0.5f;      // 每回合电力消耗

    protected override void RegisterCardEvents()
    {
        var waterLevelReductionRateText = ColorManager.ColorizeNumber(WATER_LEVEL_REDUCTION_RATE, ColorManager.Green);
        var powerConsumptionRateText = ColorManager.ColorizeNumber(POWER_CONSUMPTION_RATE, ColorManager.Red);
        AddCardEvent("开启", $"开启{CardName}\n开启后每{ColorManager.ColorizeNumber(15, ColorManager.Cyan, "0")}分钟使水平面降低{waterLevelReductionRateText}，" +
            $"并消耗{powerConsumptionRateText}电力", powerConsumption.ConnectPower, CanConnectPower);
        AddCardEvent("关闭", "", powerConsumption.DisconnectPower, powerConsumption.CanDisconnectPower);
        base.RegisterCardEvents(); // 拆毁
    }

    protected override void OnLateConstructor()
    {
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
        // 开启后水平面每回合下降
        StateManager.Instance.ChangeWaterLevelChangeRate(-WATER_LEVEL_REDUCTION_RATE);
        stateMachine.ChangeState("开启");
        // 播放循环音（仅当玩家在同一地点时）
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.PlayCardLoopSound(CardId, "电动排水机循环音", 0.3f);
    }

    /// <summary>
    /// 断电事件
    /// </summary>
    private void PowerOff()
    {
        StateManager.Instance.ChangeWaterLevelChangeRate(+WATER_LEVEL_REDUCTION_RATE);
        stateMachine.ChangeState("关闭");
        // 停止循环音（仅当玩家在同一地点时）
        if (GameManager.Instance.IsCurrentEnvironment(Bag))
            SoundManager.Instance.StopCardLoopSound(CardId);
    }

    public override void OnEnterEnvironment()
    {
        // 玩家进入卡牌所在地点时调用：若当前接通电源则开始播放循环音
        if (powerConsumption != null && powerConsumption.Connected)
            SoundManager.Instance.PlayCardLoopSound(CardId, "电动排水机循环音", 0.3f);
    }
    public override void OnLeaveEnvironment()
    {
        // 玩家离开卡牌所在地点时调用：停止播放该卡牌的循环音
        SoundManager.Instance.StopCardLoopSound(CardId);
    }
    public override void OnDetailOpen()
    {
        // 打开卡牌详情界面时调用：将循环音音量调高以突出音效
        SoundManager.Instance.SetCardLoopVolume(CardId, 1.0f);
    }
    public override void OnDetailClose()
    {
        // 关闭卡牌详情界面时调用：将循环音恢复到默认的较低音量
        SoundManager.Instance.SetCardLoopVolume(CardId, 0.3f);
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