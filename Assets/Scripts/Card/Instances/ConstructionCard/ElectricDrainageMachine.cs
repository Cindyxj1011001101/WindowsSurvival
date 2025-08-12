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
    public void Event_Open(out string tip)
    {
        tip = string.Empty;
        isWorking = true;
    }

    public bool Judge_Open(out string hint)
    {
        hint = string.Empty;
        return !isWorking;
    }

    public void Event_Close(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
    }

    public bool Judge_Close(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        Work();
    };

    private void Work()
    {
        if (!isWorking) return;
        if (StateManager.Instance.Electricity.CurValue < 0.5f || StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            isWorking = false;
            EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
            return;
        }
        StateManager.Instance.ChangeElectricity(-0.5f);
        StateManager.Instance.ChangeWaterLevel(-2);
    }
}