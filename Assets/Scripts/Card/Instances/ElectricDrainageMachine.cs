public class ElectricDrainageMachine : Card
{
    public bool isWorking; // 是否已打开
    private ElectricDrainageMachine()
    {
        isWorking = false;
        Events = new()
        {
            new Event("开启", "开启电动排水机", Event_Open, Judge_Open),
            new Event("关闭", "关闭电动排水机", Event_Close, Judge_Close)
        };
    }
    #region 开关
    public void Event_Open(out string tip)
    {
        tip = string.Empty;
        isWorking = true;
    }

    public bool Judge_Open()
    {
        return !isWorking;
    }

    public void Event_Close(out string tip)
    {
        tip = string.Empty;
        isWorking = false;
    }

    public bool Judge_Close()
    {
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
            return;
        }
        StateManager.Instance.ChangeElectricity(-0.5f);
        StateManager.Instance.ChangeWaterLevel(-0.8f);
    }

}