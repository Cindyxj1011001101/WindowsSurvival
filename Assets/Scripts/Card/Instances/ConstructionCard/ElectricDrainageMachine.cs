using System;

/// <summary>
/// 电动排水机
/// </summary>
public class ElectricDrainageMachine : ConstructionCard
{
    public bool isWorking = false; // 是否已打开
    public float electricityConsume = 0.5f; // 每回合电力消耗

    private ElectricDrainageMachine()
    {
        Events = new()
        {
            new Event("开启", "开启后每15分钟消耗0.5电力，降低2水平面高度", Event_Open, Judge_Open),
            new Event("关闭", "", Event_Close, Judge_Close)
        };
    }

    private void StartWorking()
    {
        if (isWorking) return;

        isWorking = true;
        StateManager.Instance.ChangeElectricityChangeRate(-electricityConsume);
        StateManager.Instance.ChangeWaterLevelChangeRate(-2f);
    }

    private void StopWorking()
    {
        if (!isWorking) return;

        isWorking = false;
        // 停止工作时，恢复电力和水平面变化率
        StateManager.Instance.ChangeElectricityChangeRate(+electricityConsume);
        StateManager.Instance.ChangeWaterLevelChangeRate(+2f);
    }

    #region 开关
    private void Event_Open(out string tip)
    {
        tip = string.Empty;

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

        StopWorking();
    }

    private bool Judge_Close(out string hint)
    {
        hint = string.Empty;
        return isWorking;
    }
    #endregion

    protected override Action OnUpdate => () =>
    {
        if (!isWorking) return;

        // 如果电力小于0.5或者水平面小于0时，自动停止工作
        if (StateManager.Instance.Electricity.CurValue < electricityConsume)
        {
            StopWorking();
            RefreshSlot();
            ShowTip("电力不足，排水机已自动停止工作");
        }
        else if (StateManager.Instance.WaterLevel.CurValue <= 0)
        {
            StopWorking();
            RefreshSlot();
            ShowTip("水平面已为0，排水机已自动停止工作");
        }
    };
}