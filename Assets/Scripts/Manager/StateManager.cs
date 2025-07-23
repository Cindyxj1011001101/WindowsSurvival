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
    Sobriety,
}

/// <summary>
/// 环境状态
/// </summary>
public enum EnvironmentStateEnum
{
    Electricity,
    Oxygen,
    WaterLevel,
    HasCable,
    PressureLevel
}

/// <summary>
/// 玩家状态类
/// </summary>
public class PlayerState
{
    private float curValue;
    private float extraValue;
    private float maxValue;
    public PlayerStateEnum stateEnum;

    public float CurValue => Mathf.Clamp(curValue, 0, MaxValue);

    public float ExtraValue => extraValue;

    public float MaxValue => maxValue + extraValue;
    public float RemainingCapacity => MaxValue - CurValue;

    public void AddCurValue(float delta)
    {
        curValue += delta;
    }

    public void AddExtraValue(float delta)
    {
        extraValue += delta;
    }

    public PlayerState(float value, float maxValue, float extraValue, PlayerStateEnum state)
    {
        curValue = value;
        this.maxValue = maxValue;
        this.extraValue = extraValue;
        stateEnum = state;
    }
}

/// <summary>
/// 环境状态类
/// </summary>
public class EnvironmentState
{
    private float curValue;
    public float MaxValue { get; set; }
    public float RemainingCapacity => MaxValue - CurValue;
    public EnvironmentStateEnum stateEnum;

    public float CurValue
    {
        get => curValue;
        set
        {
            curValue = Mathf.Clamp(value, 0, MaxValue);
        }
    }

    public EnvironmentState(float value, float maxValue, EnvironmentStateEnum state)
    {
        curValue = value;
        MaxValue = maxValue;
        stateEnum = state;
    }
}

public class StateManager : MonoBehaviour
{
    /// <summary>
    /// 玩家状态
    /// </summary>
    public Dictionary<PlayerStateEnum, PlayerState> PlayerStateDict { get; private set; } = new();

    /// <summary>
    /// 电力
    /// </summary>
    public EnvironmentState Electricity { get; set; }

    /// <summary>
    /// 飞船水平面高度
    /// </summary>
    public EnvironmentState WaterLevel { get; set; }

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

        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
    }

    public void Start()
    {
        InitPlayerState();
        InitElectricity();
    }

    public void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
    }

    private void InitPlayerState()
    {
        //初始化玩家状态
        PlayerStateDict.Add(PlayerStateEnum.Health, new PlayerState(InitPlayerStateData.Instance.Health, 100, 0, PlayerStateEnum.Health));
        PlayerStateDict.Add(PlayerStateEnum.Fullness, new PlayerState(InitPlayerStateData.Instance.Fullness, 100, 0, PlayerStateEnum.Fullness));
        PlayerStateDict.Add(PlayerStateEnum.Thirst, new PlayerState(InitPlayerStateData.Instance.Thirst, 100, 0, PlayerStateEnum.Thirst));
        PlayerStateDict.Add(PlayerStateEnum.San, new PlayerState(InitPlayerStateData.Instance.San, 100, 0, PlayerStateEnum.San));
        PlayerStateDict.Add(PlayerStateEnum.Oxygen, new PlayerState(InitPlayerStateData.Instance.Oxygen, 30, 0, PlayerStateEnum.Oxygen));
        PlayerStateDict.Add(PlayerStateEnum.Sobriety, new PlayerState(InitPlayerStateData.Instance.Sobriety, 100, 0, PlayerStateEnum.Sobriety));
    }

    private void InitElectricity()
    {
        Electricity = new EnvironmentState(Random.Range(30, 45), 50, EnvironmentStateEnum.Electricity);
    }
    #endregion


    #region 状态变化相关

    /// <summary>
    /// 改变玩家状态
    /// </summary>
    /// <param name="stateEnum"></param>
    /// <param name="delta"></param>
    public void ChangePlayerState(PlayerStateEnum stateEnum, float delta)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        // 氧气特殊处理
        if (stateEnum == PlayerStateEnum.Oxygen)
        {
            var env = GameManager.Instance.CurEnvironmentBag;
            // 如果获取氧气时在室内环境
            if (env.PlaceData.isIndoor)
            {
                // 优先释放到环境
                // 计算能释放多少
                var toRelease = Mathf.Min(env.StateDict[EnvironmentStateEnum.Oxygen].RemainingCapacity, delta);
                if (toRelease > 0)
                    // 释放到环境
                    env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, toRelease);

                // 计算释放到环境之后还剩多少
                var left = delta - toRelease;
                // 加入玩家的氧气
                if (left > 0)
                    PlayerStateDict[PlayerStateEnum.Oxygen].AddCurValue(left);
                return;
            }
        }

        PlayerStateDict[stateEnum].AddCurValue(delta);

        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }

    public void ChangePlayerExtraState(PlayerStateEnum stateEnum, float delta)
    {
        PlayerStateDict[stateEnum].AddExtraValue(delta);
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }
    #endregion

    #region 电力和水平面相关
    public void ChangeElectricity(float delta)
    {
        Electricity.CurValue += delta;
        // 刷新前端显示
        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.Electricity)
        {
            stateValue = Electricity
        });
    }

    public void ChangeWaterLevel(float delta)
    {
        WaterLevel.CurValue += delta;
        // 触发水平面变化事件
        EventManager.Instance.TriggerEvent(EventType.ChangeWaterLevel, WaterLevel.CurValue);
        // 刷新前端显示
        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.WaterLevel)
        {
            stateValue = WaterLevel
        });
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
    }

    public void EnvironmentIntervalSettle()
    {
        // 每回合减少0.2电力
        ChangeElectricity(InitPlayerStateData.Instance.BasicElectricityChange);
    }

    public void PlayerIntervalSettle()
    {
        ChangePlayerState(PlayerStateEnum.Fullness, InitPlayerStateData.Instance.BasicFullnessChange);
        ChangePlayerState(PlayerStateEnum.Health, InitPlayerStateData.Instance.BasicHealthChange);
        ChangePlayerState(PlayerStateEnum.Thirst, InitPlayerStateData.Instance.BasicThirstChange);
        ChangePlayerState(PlayerStateEnum.San, InitPlayerStateData.Instance.BasicSanChange);
        ChangePlayerState(PlayerStateEnum.Sobriety, InitPlayerStateData.Instance.BasicSobrietyChange);
        ChangePlayerState(PlayerStateEnum.Oxygen, InitPlayerStateData.Instance.BasicOxygenChange);
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
        ExtraSobrietyChange();
    }

    /// <summary>
    /// 饥饿导致的额外变化结算
    /// 饱食低于20，每回合-0.3精神
    /// 饱食低于10，每回合-0.7精神
    /// 饱食为0时，每回合-1精神，-8 健康
    /// </summary>
    private void ExtraFullnessChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Fullness].CurValue <= 20)
        {
            ChangePlayerState(PlayerStateEnum.San, -0.3f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].CurValue <= 10)
        {
            ChangePlayerState(PlayerStateEnum.San, -0.7f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Fullness].CurValue <= 0)
        {
            ChangePlayerState(PlayerStateEnum.San, -1f);
            ChangePlayerState(PlayerStateEnum.Health, -8f);
        }
    }

    /// <summary>
    /// 健康导致的额外变化结算
    /// </summary>
    private void ExtraHealthChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Health].CurValue <= 0)
        {
            EventManager.Instance.TriggerEvent(EventType.GameOver);
        }
    }

    /// <summary>
    /// 口渴导致的额外变化结算
    /// 水分低于20，每回合-0.3精神
    /// 水分低于10，每回合-0.7精神
    /// 口渴为0时，每回合-1精神，-8健康
    /// </summary>
    private void ExtraThirstChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Thirst].CurValue <= 20)
        {
            ChangePlayerState(PlayerStateEnum.San, -0.3f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].CurValue <= 10)
        {
            ChangePlayerState(PlayerStateEnum.San, -0.7f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Thirst].CurValue <= 0)
        {
            ChangePlayerState(PlayerStateEnum.San, -1f);
            ChangePlayerState(PlayerStateEnum.Health, -8f);
        }
    }

    /// <summary>
    /// 精神导致的额外变化结算
    /// </summary>
    private void ExtraSanChange()
    {

    }

    /// <summary>
    /// 氧气导致的额外变化结算
    /// </summary>
    private void ExtraOxygenChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Oxygen].CurValue == 0)
        {
            ChangePlayerState(PlayerStateEnum.Health, -10f);
        }
    }

    /// <summary>
    /// 疲劳导致的额外变化结算
    /// 困倦大于70每回合-0.5精神
    /// 困倦大于90后每回合-1.5精神，-2健康
    /// </summary>
    private void ExtraSobrietyChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Sobriety].CurValue <= 30)
        {
            ChangePlayerState(PlayerStateEnum.San, -0.3f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Sobriety].CurValue <= 10)
        {
            ChangePlayerState(PlayerStateEnum.San, -1.5f);
            ChangePlayerState(PlayerStateEnum.Health, -2f);
        }
        else if (PlayerStateDict[PlayerStateEnum.Sobriety].CurValue == 0)
        {
            ChangePlayerState(PlayerStateEnum.San, -6f);
            ChangePlayerState(PlayerStateEnum.Health, -4f);
        }
    }

    #endregion

    #region 载重
    /// <summary>
    /// 最大负重
    /// </summary>
    public float MaxLoad { get; set; } = 15;
    /// <summary>
    /// 当前负重
    /// </summary>
    public float CurLoad { get; set; } = 0;

    public void AddLoad(float weight)
    {
        CurLoad += weight;
        EventManager.Instance.TriggerEvent(EventType.ChangeLoad);
    }
    #endregion
}