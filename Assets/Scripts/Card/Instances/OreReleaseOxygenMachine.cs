using System.Collections.Generic;
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
    public int curOreNum; // 白爆矿数量
    public int oreConsumption; // 白爆矿消耗量
    public float electricityConsumption; // 电力消耗量

    private OreReleaseOxygenMachine()
    {
        maxOxygenStorage = 360;
        curOxygenStorage = 0;
        maxTimeProgress = 360;
        curTimeProgress = 0;
        oxygenRelease = 180;
        curOreNum = 0;
        oreConsumption = 1;
        electricityConsumption = 1;
        Events = new()
        {
            new Event("打开", "打开矿石释氧机", Event_Open, Judge_Open),
            new Event("关闭", "关闭矿石释氧机", Event_Close, Judge_Close),
            new Event("获取氧气", "获取氧气", Event_GetOxygen, Judge_GetOxygen)
        };
    }

    #region 开关
    public void Event_Open()
    {
        isWorking = true;
    }

    public bool Judge_Open()
    {
        return !isWorking;
    }

    public void Event_Close()
    {
        isWorking = false;
    }

    public bool Judge_Close()
    {
        return isWorking;
    }
    #endregion

    #region 获取氧气
    public void Event_GetOxygen()
    {
        //if (GameManager.Instance.EquipmentBag.FindCardOfName("氧气罐") == null) return;

        //Dictionary<PlayerStateEnum, PlayerState> playerStateDict = StateManager.Instance.PlayerStateDict;

        

        //if (playerStateDict[PlayerStateEnum.Oxygen].CurValue < playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen])
        //{
        //    //判断氧气罐是否能装下，装不下保留，装下则全装
        //    float canGet = playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] - playerStateDict[PlayerStateEnum.Oxygen].CurValue;
        //    if (canGet > oxygenRelease)
        //    {
        //        playerStateDict[PlayerStateEnum.Oxygen].CurValue += oxygenRelease;
        //        StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] -= oxygenRelease;
        //        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
        //    }
        //    else
        //    {
        //        playerStateDict[PlayerStateEnum.Oxygen].CurValue += canGet;
        //        StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] -= canGet;
        //        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
        //    }
        //}
    }

    public bool Judge_GetOxygen()
    {
        ////TODO:判断是否装备氧气罐，氧气是否已满
        //if (GameManager.Instance.EquipmentBag.FindCardOfName("氧气罐") == null) return false;

        //Dictionary<PlayerStateEnum, PlayerState> playerStateDict = StateManager.Instance.PlayerStateDict;
        //if (playerStateDict[PlayerStateEnum.Oxygen].CurValue < playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen])
        //{
        //    return true;
        //}

        return false;
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
            Debug.Log("氧气储存已满");
            return;
        }

        // 白爆矿不够不制氧
        if (curOreNum < oreConsumption) return;

        // 没连接到电网不制氧
        var env = Slot.Bag as EnvironmentBag;
        if (!env.HasCable) return;

        // 电力不足不制氧
        if (StateManager.Instance.Electricity.CurValue < electricityConsumption) return;

        //归零生产进度
        curTimeProgress = 0;

        //消耗白爆矿和电力
        curOreNum -= oreConsumption;

        // 消耗电力
        StateManager.Instance.ChangeElectricity(electricityConsumption);

        // 氧气存量增加
        curOxygenStorage += oxygenRelease;
    }
}