using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 玩家状态
/// </summary>
public enum PlayerStateEnum
{
    Health,
    Fullness,
    Thirst,
    San,
    Oxygen,
    Tired,
}

/// <summary>
/// 环境状态
/// </summary>
public enum EnvironmentStateEnum
{
    Electricity,
    Oxygen,
    Pressure,
    Height,
    hasCable,
}

/// <summary>
/// 玩家状态类
/// </summary>
public class PlayerState
{
    public float curValue;
    public float MaxValue;
    public PlayerStateEnum stateEnum;
    public PlayerState(float value, float maxValue, PlayerStateEnum state)
    {
        curValue = value;
        MaxValue = maxValue;
        stateEnum = state;
    }
}

/// <summary>
/// 环境状态类
/// </summary>
public class EnvironmentState
{
    public float curValue;
    public float MaxValue;
    public EnvironmentStateEnum stateEnum;
    public EnvironmentState(float value, float maxValue, EnvironmentStateEnum state)
    {
        curValue = value;
        MaxValue = maxValue;
        stateEnum = state;
    }
}
public class StateManager : MonoBehaviour
{
    [Header("玩家状态")]
    public Dictionary<PlayerStateEnum, PlayerState> PlayerStateDict = new Dictionary<PlayerStateEnum, PlayerState>();
    [Header("电力")]
    public float Electricity;
    #region 单例
    private static StateManager instance;
    public static StateManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StateManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("StateManager");
                    instance = managerObj.AddComponent<StateManager>();
                    DontDestroyOnLoad(managerObj);
                }
            }
            return instance;
        }
    }
    #endregion

    #region 初始化相关
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    public void Start()
    {
        EventManager.Instance.AddListener<ChangeStateArgs>(EventType.ChangeState, OnPlayerChangeState);
        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeStateArgs>(EventType.ChangeState, OnPlayerChangeState);
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void Init()
    {
        //初始化玩家状态
        PlayerStateDict.Add(PlayerStateEnum.Health, new PlayerState(InitPlayerStateData.Instance.Health, 100, PlayerStateEnum.Health));
        PlayerStateDict.Add(PlayerStateEnum.Fullness, new PlayerState(InitPlayerStateData.Instance.Fullness, 100, PlayerStateEnum.Fullness));
        PlayerStateDict.Add(PlayerStateEnum.Thirst, new PlayerState(InitPlayerStateData.Instance.Thirst, 100, PlayerStateEnum.Thirst));
        PlayerStateDict.Add(PlayerStateEnum.San, new PlayerState(InitPlayerStateData.Instance.San, 100, PlayerStateEnum.San));
        PlayerStateDict.Add(PlayerStateEnum.Oxygen, new PlayerState(InitPlayerStateData.Instance.Oxygen, 100, PlayerStateEnum.Oxygen));
        PlayerStateDict.Add(PlayerStateEnum.Tired, new PlayerState(InitPlayerStateData.Instance.Tired, 100, PlayerStateEnum.Tired));
    }

        /// <summary>
    /// 开局初始化环境状态
    /// </summary>
    public Dictionary<EnvironmentStateEnum, EnvironmentState> InitEnvironmentStateDict()
    {
        Electricity=Random.Range(30, 45);
        Dictionary<EnvironmentStateEnum, EnvironmentState> environmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>();
        if(GameManager.Instance.CurEnvironmentBag.PlaceData.isIndoor)
        {
            environmentStateDict.Add(EnvironmentStateEnum.Oxygen, new EnvironmentState(Random.Range(400, 600), 1000, EnvironmentStateEnum.Oxygen));
        }
        environmentStateDict.Add(EnvironmentStateEnum.Pressure, new EnvironmentState(2, 4, EnvironmentStateEnum.Pressure));
        environmentStateDict.Add(EnvironmentStateEnum.Height, new EnvironmentState(0, 100, EnvironmentStateEnum.Height));
        if(GameManager.Instance.CurEnvironmentBag.PlaceData.isInSpacecraft)
        {
            environmentStateDict.Add(EnvironmentStateEnum.hasCable, new EnvironmentState(1, 1, EnvironmentStateEnum.hasCable));
        }
        else
        {
            environmentStateDict.Add(EnvironmentStateEnum.hasCable, new EnvironmentState(0, 1, EnvironmentStateEnum.hasCable));
        }

        return environmentStateDict;
    }
    #endregion



    #region 状态变化相关
    /// <summary>
    /// 玩家状态变化
    /// 修改某一玩家状态值，保证在最大最小之间，触发刷新UI事件
    /// </summary>
    public void OnPlayerChangeState(ChangeStateArgs args)
    {
        if (PlayerStateDict.ContainsKey(args.state))
        {
            PlayerStateDict[args.state].curValue += args.value;
            if (PlayerStateDict[args.state].curValue >= PlayerStateDict[args.state].MaxValue)
            {
                PlayerStateDict[args.state].curValue = PlayerStateDict[args.state].MaxValue;
            }
            if (PlayerStateDict[args.state].curValue <= 0)
            {
                PlayerStateDict[args.state].curValue = 0;
            }
        }
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, args.state);
    }

    /// <summary>
    /// 环境状态变化
    /// 修改某一环境状态值，保证在最大最小之间，触发刷新UI事件
    /// </summary>
    public void OnEnvironmentChangeState(ChangeEnvironmentStateArgs args)
    {
        //判断一下是否为修改电力，如果是则直接修改电力值
        if(args.state==EnvironmentStateEnum.Electricity)
        {
            Electricity += args.value;
            if(Electricity>=50)
            {
                Electricity=50;
            }
            if(Electricity<=0)
            {
                Electricity=0;
            }
            //前端UI刷新
            EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, EnvironmentStateEnum.Electricity);
        }
        else
        {
            //在当前背包中做数值变化
            EventManager.Instance.TriggerEvent(EventType.CurEnvironmentChangeState, args);
        }
    }
    #endregion

    #region 定时结算相关
    /// <summary>
    /// 定时结算玩家状态
    /// 玩家状态值基础结算，不考虑环境状态
    /// </summary>
    public void IntervalSettle()
    {
        PlayerIntervalSettle();
        ExtraPlayerIntervalSettle();
        EnvironmentIntervalSettle();
        ExtraEnvironmentIntervalSettle();
    }

    public void EnvironmentIntervalSettle()
    {
        OnEnvironmentChangeState(new ChangeEnvironmentStateArgs(EnvironmentStateEnum.Electricity, -0.2f));
    }

    public void ExtraEnvironmentIntervalSettle()
    {

    }

    public void PlayerIntervalSettle()
    {
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, InitPlayerStateData.Instance.BasicFullnessChange));
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, InitPlayerStateData.Instance.BasicHealthChange));
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, InitPlayerStateData.Instance.BasicThirstChange));
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, InitPlayerStateData.Instance.BasicSanChange));
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, InitPlayerStateData.Instance.BasicOxygenChange));
        OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Tired, InitPlayerStateData.Instance.BasicTiredChange));
    }

    /// <summary>
    /// 定时结算状态异常导致的额外变化
    /// </summary>
    public void ExtraPlayerIntervalSettle()
    {
        ExtraFullnessChange();
        ExtraHealthChange();
        ExtraThirstChange();
        ExtraSanChange();
        ExtraOxygenChange();
        ExtraTiredChange();
    }

    /// <summary>
    /// 饥饿导致的额外变化结算
    /// 饱食低于20，每回合-0.3精神
    /// 饱食低于10，每回合-0.7精神
    /// 饱食为0时，每回合-1精神，-8 健康
    /// </summary>
    private void ExtraFullnessChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 20)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 10)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.7f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 0)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1f));
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8f));
        }
    }

    /// <summary>
    /// 健康导致的额外变化结算
    /// </summary>
    private void ExtraHealthChange()
    {

    }

    /// <summary>
    /// 口渴导致的额外变化结算
    /// 水分低于20，每回合-0.3精神
    /// 水分低于10，每回合-0.7精神
    /// 口渴为0时，每回合-1精神，-8健康
    /// </summary>
    private void ExtraThirstChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Thirst].curValue <= 20)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].curValue <= 10)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.7f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].curValue <= 0)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1f));
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8f));
        }
    }

    /// <summary>
    /// 精神导致的额外变化结算
    /// 健康等于0时死亡
    /// </summary>
    private void ExtraSanChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Health].curValue <= 0)
        {
            EventManager.Instance.TriggerEvent(EventType.GameOver);
        }
    }

    /// <summary>
    /// 氧气导致的额外变化结算
    /// </summary>
    private void ExtraOxygenChange()
    {

    }

    /// <summary>
    /// 疲劳导致的额外变化结算
    /// 困倦大于70每回合-0.5精神
    /// 困倦大于90后每回合-1.5精神，-2健康
    /// </summary>
    private void ExtraTiredChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Tired].curValue >=70)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Tired].curValue >= 90)
        {
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1.5f));
            OnPlayerChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -2f));
        }
    }

    #endregion
}