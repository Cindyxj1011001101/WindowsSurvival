/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : Card
{
    public bool isWorking; // 是否已打开
    private ElectricDrainageMachine()
    {
        isWorking = false;
        Events = new()
        {
            new Event("开启", "开启后每回合消耗0.5电力，降低2水平面高度", Event_Open, Judge_Open),
            new Event("关闭", "关闭电动排水机", Event_Close, Judge_Close)
        };

        // 仅在室内、非水域地点建造
        AddComponent(new ConstructionComponent()
        {
            onlyInDoor = true,
            onlyOutWater = true,
            needCable = true,
        });
    }

    #region 开关
    private void Event_Open(out string tip)
    {
        tip = string.Empty;
        isWorking = true;

        // 每回合消耗0.5电力
        StateManager.Instance.ChangeElectricityChangeRate(-0.5f);
        // 每回合水平面-2
        StateManager.Instance.ChangeWaterLevelChangeRate(-2f);
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

        StateManager.Instance.ChangeElectricityChangeRate(+0.5f);
        StateManager.Instance.ChangeWaterLevelChangeRate(+2f);
    }

    private bool Judge_Close(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        // 电力小于0.5或者水平面小于0时，自动停止工作
        if (StateManager.Instance.Electricity.CurValue < 0.5f || StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            isWorking = false;
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
            return;
        }
    };
}