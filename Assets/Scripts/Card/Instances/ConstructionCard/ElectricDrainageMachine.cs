/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
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
    }

    public override bool CanPlace(out string hint)
    {
        hint = string.Empty;
        var env = GameManager.Instance.CurEnvironmentBag;

        // 只能放置在室内非水域环境
        if (!env.PlaceData.isIndoor)
        {
            hint = "只能建造在室内地点";
            return false;
        }

        if (env.PlaceData.isInWater)
        {
            hint = "只能建造在非水域地点";
            return false;
        }

        if (!env.HasCable)
        {
            hint = "需要先在该地点铺设电缆";
            return false;
        }

        return true;
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
            return;
        }
        StateManager.Instance.ChangeElectricity(-0.5f);
        StateManager.Instance.ChangeWaterLevel(-2);
    }
}