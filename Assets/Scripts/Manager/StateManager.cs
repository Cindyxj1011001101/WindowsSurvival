using System;
using System.Collections.Generic;
using UnityEngine;

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
    Oxygen,
    Electricity,
    Temperature,
    Height,
    hasCable,
    isIndoor,
    isInWater,
    isInSpacecraft,
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

    [Header("当前地点环境状态")]
    public Dictionary<EnvironmentStateEnum, EnvironmentState> CurEnvironmentStateDict = new Dictionary<EnvironmentStateEnum, EnvironmentState>();

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
        EventManager.Instance.AddListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener<ChangeStateArgs>(EventType.ChangeState, OnChangeState);
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
    }
    public void Init()
    {
        PlayerStateDict.Add(PlayerStateEnum.Health, new PlayerState(InitPlayerStateData.Instance.Health, 100, PlayerStateEnum.Health));
        PlayerStateDict.Add(PlayerStateEnum.Fullness, new PlayerState(InitPlayerStateData.Instance.Fullness, 100, PlayerStateEnum.Fullness));
        PlayerStateDict.Add(PlayerStateEnum.Thirst, new PlayerState(InitPlayerStateData.Instance.Thirst, 100, PlayerStateEnum.Thirst));
        PlayerStateDict.Add(PlayerStateEnum.San, new PlayerState(InitPlayerStateData.Instance.San, 100, PlayerStateEnum.San));
        PlayerStateDict.Add(PlayerStateEnum.Oxygen, new PlayerState(InitPlayerStateData.Instance.Oxygen, 100, PlayerStateEnum.Oxygen));
        PlayerStateDict.Add(PlayerStateEnum.Tired, new PlayerState(InitPlayerStateData.Instance.Tired, 100, PlayerStateEnum.Tired));
    }
    #endregion

    #region 状态变化相关
    /// <summary>
    /// 玩家状态变化
    /// 修改某一玩家状态值，保证在最大最小之间，触发刷新UI事件
    /// </summary>
    public void OnChangeState(ChangeStateArgs args)
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

        EventManager.Instance.TriggerEvent(EventType.RefreshState, args.state);
    }
    #endregion

    #region 定时结算相关
    /// <summary>
    /// 定时结算玩家状态
    /// 玩家状态值基础结算，不考虑环境状态
    /// </summary>
    public void IntervalSettle()
    {
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Fullness, InitPlayerStateData.Instance.BasicFullnessChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Health, InitPlayerStateData.Instance.BasicHealthChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Thirst, InitPlayerStateData.Instance.BasicThirstChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, InitPlayerStateData.Instance.BasicSanChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Oxygen, InitPlayerStateData.Instance.BasicOxygenChange));
        OnChangeState(new ChangeStateArgs(PlayerStateEnum.Tired, InitPlayerStateData.Instance.BasicTiredChange));
        ExtraIntervalSettle();
    }

    /// <summary>
    /// 定时结算状态异常导致的额外变化
    /// </summary>
    public void ExtraIntervalSettle()
    {
        ExtraFullnesssChange();
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
    private void ExtraFullnesssChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 20)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 10)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.7f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].curValue <= 0)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1f));
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8f));
        }
    }

    /// <summary>
    /// 健康导致的额外变化结算
    /// </summary>
    private void ExtraHealthChange()
    {
        throw new NotImplementedException();
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
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].curValue <= 10)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.7f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].curValue <= 0)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1f));
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -8f));
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
        throw new NotImplementedException();
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
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -0.3f));
        }
        else if (PlayerStateDict[PlayerStateEnum.Tired].curValue >= 90)
        {
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.San, -1.5f));
            OnChangeState(new ChangeStateArgs(PlayerStateEnum.Health, -2f));
        }
    }

    #endregion
}