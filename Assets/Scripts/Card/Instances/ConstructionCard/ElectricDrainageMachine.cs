/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
{
    public bool isWorking = false; // 是否已打开
    private ElectricDrainageMachine()
    {
        Events = new()
        {
            new Event("开启", "开启后每15分钟消耗0.5电力，降低2水平面高度", Event_Open, Judge_Open),
            new Event("关闭", "", Event_Close, Judge_Close)
        };
    }

    protected override void LateInit()
    {
        base.LateInit();
        EventManager.Instance.AddListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityOrWaterLevelChanged);
    }

    public override void DestroyThis()
    {
        base.DestroyThis();
        EventManager.Instance.RemoveListener<RefreshEnvironmentStateArgs>(EventType.RefreshEnvironmentState, OnElectricityOrWaterLevelChanged);
    }

    private void OnElectricityOrWaterLevelChanged(RefreshEnvironmentStateArgs args)
    {
        if (args.stateEnum != EnvironmentStateEnum.Electricity && args.stateEnum != EnvironmentStateEnum.WaterLevel) return;

        if (!isWorking) return;

        // 如果电力小于0.5或者水平面小于0时，自动停止工作
        if (StateManager.Instance.Electricity.CurValue < 0.5f)
        {
            isWorking = false;
            ShowTip("电力不足，排水机已自动停止工作");
            StopWorking();
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
        }
        else if (StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            isWorking = false;
            ShowTip("水平面已为0，排水机已自动停止工作");
            StopWorking();
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
        }
    }

    private void StartWorking()
    {
        StateManager.Instance.ChangeElectricityChangeRate(-0.5f);
        StateManager.Instance.ChangeWaterLevelChangeRate(-2f);
    }

    private void StopWorking()
    {
        // 停止工作时，恢复电力和水平面变化率
        StateManager.Instance.ChangeElectricityChangeRate(+0.5f);
        StateManager.Instance.ChangeWaterLevelChangeRate(+2f);
    }

    #region 开关
    private void Event_Open(out string tip)
    {
        tip = string.Empty;
        isWorking = true;

        StartWorking();
    }

    private bool Judge_Open(out string hint)
    {
        hint = string.Empty;
        return !isWorking;
    }

    private void Event_Close(out string tip)
    {
        tip = string.Empty;
        isWorking = false;

        StopWorking();
    }

    private bool Judge_Close(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    #endregion
}