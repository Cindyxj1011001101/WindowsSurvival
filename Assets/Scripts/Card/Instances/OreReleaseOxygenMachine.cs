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
    public float OxygenRelease; // 氧气释放量
    public int curOreNum; // 白爆矿数量
    public int oreConsumption; // 白爆矿消耗量
    public float electricityConsumption; // 电力消耗量

    public OreReleaseOxygenMachine()
    {
        maxOxygenStorage = 360;
        curOxygenStorage = 0;
        maxTimeProgress = 360;
        curTimeProgress = 0;
        OxygenRelease = 180;
        curOreNum = 0;
        oreConsumption = 1;
        electricityConsumption = 1;
        events = new()
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
        if (GameManager.Instance.EquipmentBag.FindCardOfName("氧气罐") != null)
        {
            Dictionary<PlayerStateEnum, PlayerState> playerStateDict = StateManager.Instance.PlayerStateDict;
            if (playerStateDict[PlayerStateEnum.Oxygen].curValue < playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen])
            {
                //判断氧气罐是否能装下，装不下保留，装下则全装
                float canGet = playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] - playerStateDict[PlayerStateEnum.Oxygen].curValue;
                if (canGet > OxygenRelease)
                {
                    playerStateDict[PlayerStateEnum.Oxygen].curValue += OxygenRelease;
                    StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] -= OxygenRelease;
                    EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
                }
                else
                {
                    playerStateDict[PlayerStateEnum.Oxygen].curValue += canGet;
                    StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen] -= canGet;
                    EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
                }
            }
        }
    }

    public bool Judge_GetOxygen()
    {
        //TODO:判断是否装备氧气罐，氧气是否已满
        if (GameManager.Instance.EquipmentBag.FindCardOfName("氧气罐") != null)
        {
            Dictionary<PlayerStateEnum, PlayerState> playerStateDict = StateManager.Instance.PlayerStateDict;
            if (playerStateDict[PlayerStateEnum.Oxygen].curValue < playerStateDict[PlayerStateEnum.Oxygen].MaxValue + StateManager.Instance.PlayerExtraStateDict[PlayerStateEnum.Oxygen])
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    protected override System.Action OnUpdate => () =>
    {
        OxygenProgress();
    };

    //释放氧气逻辑
    public void OxygenProgress()
    {
        if (isWorking)
        {
            curTimeProgress += TimeManager.Instance.SettleInterval;
            EnvironmentBag environmentBag = slot.Bag as EnvironmentBag;
            //时间进度达到最大时，开始释放氧气
            if (curTimeProgress >= maxTimeProgress)
            {
                //判断是否能够制氧
                if (curOxygenStorage + OxygenRelease > maxOxygenStorage)//存不下就不制取
                {
                    Debug.Log("氧气储存已满");
                    return;
                }
                //白爆矿不足或电力不足或当前场景未接电线时不工作
                if (curOreNum < 1 || StateManager.Instance.Electricity < 1 || environmentBag.EnvironmentStateDict[EnvironmentStateEnum.HasCable].curValue == 0)
                {
                    Debug.Log("白爆矿不足或电力不足或当前场景未接电线");
                    return;
                }
                //归零生产进度
                curTimeProgress = 0;

                //消耗白爆矿和电力
                curOreNum -= oreConsumption;
                StateManager.Instance.Electricity -= electricityConsumption;
                curOxygenStorage += OxygenRelease;
                //室内释放氧气
                if (environmentBag.PlaceData.isIndoor)
                {
                    float canRelease = environmentBag.EnvironmentStateDict[EnvironmentStateEnum.Oxygen].MaxValue - environmentBag.EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue;
                    if (canRelease > OxygenRelease)
                    {
                        //室内能存下，全部释放到室内
                        environmentBag.EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue += OxygenRelease;
                        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs((slot.Bag as EnvironmentBag).PlaceData.placeType, EnvironmentStateEnum.Oxygen));
                        curOxygenStorage -= OxygenRelease;
                    }
                    else
                    {
                        //室内存不下，剩余部分留在机器存储中
                        environmentBag.EnvironmentStateDict[EnvironmentStateEnum.Oxygen].curValue += canRelease;
                        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs((slot.Bag as EnvironmentBag).PlaceData.placeType, EnvironmentStateEnum.Oxygen));
                        curOxygenStorage -= canRelease;
                    }
                }
            }
        }
    }
}