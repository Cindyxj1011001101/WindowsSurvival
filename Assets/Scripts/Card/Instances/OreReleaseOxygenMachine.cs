using UnityEngine;

/// <summary>
/// 矿石释氧机
/// </summary>
public class OreReleaseOxygenMachine : Card
{
    public bool isWorking; // 是否已打开
    public float maxOxygenStorage; // 最大氧气存储
    public float curOxygenStorage; // 当前氧气存储数量
    public int maxTimeProgress; // 最大时间进度
    public int curTimeProgress; // 当前时间进度
    public float oxygenRelease; // 氧气释放量
    //public int curOreNum; // 白爆矿数量
    public int oreConsumption; // 白爆矿消耗量
    public float electricityConsumption; // 电力消耗量

    private OreReleaseOxygenMachine()
    {
        isWorking = false;
        maxOxygenStorage = 360;
        curOxygenStorage = 0;
        maxTimeProgress = 360;
        curTimeProgress = 0;
        oxygenRelease = 180;
        //curOreNum = 0;
        oreConsumption = 1;
        electricityConsumption = 1;
        Events = new()
        {
            new Event("打开", "打开矿石释氧机", Event_Open, Judge_Open),
            new Event("关闭", "关闭矿石释氧机", Event_Close, Judge_Close),
            new Event("获取氧气", "消耗矿石释氧机的氧气储存，充满自身氧气", Event_GetOxygen, Judee_GetOxygen)
        };
    }   

    protected override void LateInit()
    {
        base.LateInit();
        if (TryGetComponent<InnerContentsComponent>(out var component))
        {
            component.contentFilter = ContentFilter;
        }
    }

    private bool ContentFilter(Card c, out string s)
    {
        s = string.Empty;
        if (c.CardId != "白爆矿")
        {
            s = "只能放入白爆矿";
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

    #region 获取氧气
    private bool Judee_GetOxygen()
    {
        // 玩家氧气剩余容量大于0，并且氧气储量大于0时可获取
        var remainingCapacity = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity;
        var toRelease = Mathf.Min(curOxygenStorage, remainingCapacity);
        return toRelease > 0;
    }

    public void Event_GetOxygen(out string tip)
    {
        tip = string.Empty;
        // 玩家氧气剩余容量
        var remainingCapacity = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(curOxygenStorage, remainingCapacity);
        if (toRelease > 0)
            // 释放氧气
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, toRelease);

        // 氧气存量减少
        curOxygenStorage -= toRelease;
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        // 先制氧
        GenerateOxygen();
        // 给室内环境充气
        ReleaseOxygen();
    };

    // 释放氧气
    private void ReleaseOxygen()
    {
        var env = Slot.Bag as EnvironmentBag;
        // 不是室内环境不释放氧气
        if (!env.PlaceData.isIndoor) return;

        // 室内氧气剩余容量
        var remainingCapacity = env.StateDict[EnvironmentStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(curOxygenStorage, remainingCapacity);
        if (toRelease > 0)
            // 释放氧气
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, toRelease);

        // 氧气存量减少
        curOxygenStorage -= toRelease;
    }

    // 制氧
    private void GenerateOxygen()
    {
        // 不在工作状态不制氧
        if (!isWorking) return;

        // 制氧进度增加
        curTimeProgress += TimeManager.Instance.SettleInterval;

        // 进度不满不制氧
        if (curTimeProgress < maxTimeProgress) return;

        // 时间进度达到最大时，开始释放氧气

        // 氧气存储要超了不制氧
        if (curOxygenStorage + oxygenRelease > maxOxygenStorage)
        {
            Debug.Log("氧气储存剩余空间不足");
            return;
        }

        // 没连接到电网不制氧
        var env = Slot.Bag as EnvironmentBag;
        if (!env.HasCable) return;

        // 电力不足不制氧
        if (StateManager.Instance.Electricity.CurValue < electricityConsumption) return;

        // 白爆矿不够不制氧
        if (!TryConsumeOre(oreConsumption)) return;
        
        //归零生产进度
        curTimeProgress = 0;

        // 消耗电力
        StateManager.Instance.ChangeElectricity(-electricityConsumption);

        // 氧气存量增加
        curOxygenStorage += oxygenRelease;
    }

    private bool TryConsumeOre(int amount)
    {
        TryGetComponent<InnerContentsComponent>(out var component);
        int oreCount = component.GetTotalCountByCardId("白爆矿");
        // 白爆矿的数量多余消耗量
        if (oreCount > amount)
        {
            component.RemoveContentsByCardId("白爆矿", amount);
            return true;
        }
        return false;
    }
}