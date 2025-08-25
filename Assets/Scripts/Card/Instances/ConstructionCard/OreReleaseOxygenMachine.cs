using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 矿石释氧机
/// </summary>
public class OreReleaseOxygenMachine : ConstructionCard
{
    private StateMachineComponent stateMachine;
    private InnerContentsComponent innerContents;
    private OxygenStorageComponent oxygenStorage;

    public int maxTimeProgress = 120; // 最大时间进度
    public int curTimeProgress = 0; // 当前时间进度
    public float oxygenRelease = 180; // 氧气释放量
    public int oreConsumption = 1; // 白爆矿消耗量
    public float electricityConsumption = 1; // 电力消耗量

    private OreReleaseOxygenMachine()
    {
        Events = new()
        {
            new Event("接电", "接电后矿石释氧机每2小时消耗1块白爆矿,产生180氧气", Event_Open, Judge_Open),
            new Event("断电", "断电后,将不再工作", Event_Close, Judge_Close),
            new Event("获取氧气", "消耗矿石释氧机的氧气储存，补充自身氧气", Event_GetOxygen, Judge_GetOxygen)
        };
    }

    public override void LateInit()
    {
        base.LateInit();

        // 未布置和已布置两种状态
        if (!TryGetComponent(out stateMachine))
        {
            var states = new List<CardState>()
            {
                new ("已关闭", "0", false, true, false),
                new ("已开启", "1", true, true, true),
            };
            stateMachine = new StateMachineComponent("已关闭", states);
            AddComponent(stateMachine);
        }

        // 添加氧气存储组件
        if (!TryGetComponent(out oxygenStorage))
        {
            oxygenStorage = new OxygenStorageComponent(360);
            AddComponent(oxygenStorage);
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

    public override bool CanQuickInteract(Card card)
    {
        return innerContents.CanQuickInteract(card);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        innerContents.QuickIneract(slot, count, out tip);
    }

    #region 开关
    private void Event_Open(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("已开启");
    }

    private bool Judge_Open(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已关闭";
    }

    private void Event_Close(out string tip)
    {
        tip = string.Empty;
        stateMachine.ChangeState("已关闭");
    }

    private bool Judge_Close(out string hint)
    {
        hint = string.Empty;
        return stateMachine.currentStateName == "已开启";
    }
    #endregion

    #region 获取氧气
    private bool Judge_GetOxygen(out string hint)
    {
        hint = string.Empty;
        // 玩家氧气剩余容量大于0，并且氧气储量大于0时可获取
        var remainingCapacity = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity;
        if (remainingCapacity == 0)
        {
            hint = "麦麦的氧气已满";
            return false;
        }
        var toRelease = Mathf.Min(oxygenStorage.oxygen, remainingCapacity);
        if (toRelease == 0)
        {
            hint = "机器的氧气存储不足";
            return false;
        }
        return true;
    }

    private void Event_GetOxygen(out string tip)
    {
        tip = string.Empty;
        // 玩家氧气剩余容量
        var remainingCapacity = StateManager.Instance.PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(oxygenStorage.oxygen, remainingCapacity);
        if (toRelease > 0)
            // 释放氧气
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Oxygen, toRelease);

        // 氧气存量减少
        oxygenStorage.AddOxygen(-toRelease);
    }
    #endregion

    protected override void OnUpdate()
    {
        base.OnUpdate();

        // 先制氧
        GenerateOxygen();
        // 给室内环境充气
        ReleaseOxygen();
    }

    // 释放氧气
    private void ReleaseOxygen()
    {
        var env = Bag as EnvironmentBag;
        // 不是室内环境不释放氧气
        if (!env.PlaceData.isIndoor) return;

        // 室内氧气剩余容量
        var remainingCapacity = env.StateDict[EnvironmentStateEnum.Oxygen].RemainingCapacity;
        // 计算释放量
        var toRelease = Mathf.Min(oxygenStorage.oxygen, remainingCapacity);
        if (toRelease > 0)
            // 释放氧气
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, toRelease);

        // 氧气存量减少
        oxygenStorage.AddOxygen(-toRelease);
    }

    // 制氧
    private void GenerateOxygen()
    {
        // 不在工作状态不制氧
        if (stateMachine.currentStateName == "已关闭")
        {
            return;
        }

        // 制氧进度增加
        curTimeProgress += TimeManager.Instance.SettleInterval;
        
        // 进度不满不制氧
        if (curTimeProgress < maxTimeProgress)
        {
            return;
        }

        // 氧气存储要超了不制氧
        if (oxygenStorage.oxygen + oxygenRelease > oxygenStorage.maxOxygen)
        {
            return;
        }

        // 没连接到电网不制氧
        var env = Bag as EnvironmentBag;
        if (!env.HasCable)
        {
            return;
        }

        // 电力不足不制氧
        if (StateManager.Instance.Electricity.CurValue < electricityConsumption)
        {
            return;
        }

        // 白爆矿不够不制氧
        if (!TryConsumeOre(oreConsumption))
        {
            return;
        }

        //归零生产进度
        curTimeProgress = 0;

        // 消耗电力
        StateManager.Instance.ChangeElectricity(-electricityConsumption);

        // 氧气存量增加
        oxygenStorage.AddOxygen(oxygenRelease);
    }

    private bool TryConsumeOre(int amount)
    {
        int oreCount = innerContents.GetTotalCountByCardId("白爆矿");
        // 白爆矿的数量多于消耗量
        if (oreCount >= amount)
        {
            innerContents.DestroyCardsByCardId("白爆矿", amount);
            return true;
        }
        return false;
    }
}