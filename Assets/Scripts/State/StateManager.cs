using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 环境状态
/// </summary>
public enum EnvironmentStateEnum
{
    Electricity,
    Oxygen,
    WaterLevel,
    HasCable,
    PressureLevel,
    RoomTemperature,
    CarbonMonoxideLevel,
    Dirtiness,
}
/// <summary>
/// 当前危险程度
/// </summary>
public enum DangerLevelEnum
{
    High = 0,
    Low = 1,
    None = 2,
}

/// <summary>
/// 环境状态类
/// </summary>
public class EnvironmentState
{
    [JsonProperty]
    private float curValue;
    [JsonProperty]
    public float MaxValue { get; set; }
    [JsonProperty]
    public EnvironmentStateEnum stateEnum;
    [JsonIgnore]
    public float RemainingCapacity => MaxValue - CurValue;

    [JsonIgnore]
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
    public EnvironmentState Electricity { get; private set; }

    /// <summary>
    /// 飞船水平面高度
    /// </summary>
    public EnvironmentState WaterLevel { get; private set; }

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

        // 初始化
        var stateData = GameDataManager.Instance.StateData;
        if (!stateData.init)
        {
            InitPlayerStates();
            InitElectricity();
            InitWaterLevel();
        }
        else
        {
            Electricity = stateData.electricity;
            WaterLevel = stateData.waterLevel;
            PlayerStateDict = stateData.playerState;
        }

        SetupPlayerStateEvents();

        // 监听回合结算
        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
        // 当环境改变时尝试获取氧气
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, TryGainOxygenFromEnvironment);
    }

    private void Start()
    {
        // 评估危险状态，播放音乐
        EvaluateDangerLevel();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, TryGainOxygenFromEnvironment);
    }

    #region 初始化玩家状态
    private void InitPlayerStates()
    {
        // 初始化玩家状态
        PlayerStateDict.Add(PlayerStateEnum.Health, InitHealthState());
        PlayerStateDict.Add(PlayerStateEnum.Fullness, InitFullnessState());
        PlayerStateDict.Add(PlayerStateEnum.Thirst, InitThirstState());
        PlayerStateDict.Add(PlayerStateEnum.San, InitSanityState());
        PlayerStateDict.Add(PlayerStateEnum.Oxygen, InitOxygenState());
        PlayerStateDict.Add(PlayerStateEnum.Sobriety, InitSorbriety());
        PlayerStateDict.Add(PlayerStateEnum.Load, InitLoadState());
        PlayerStateDict.Add(PlayerStateEnum.BodyTemperature, InitBodyTemperatureState());
        PlayerStateDict.Add(PlayerStateEnum.CarbonMonoxidePoisoning, InitCarbonMonoxideState());
        PlayerStateDict.Add(PlayerStateEnum.Itchiness, InitItchinessState());
        PlayerStateDict.Add(PlayerStateEnum.PainLevel, InitPainState());
    }

    /// <summary>
    /// 设置玩家状态等级变化的事件
    /// </summary>
    private void SetupPlayerStateEvents()
    {
        PlayerStateDict[PlayerStateEnum.Health].SetUpEvent(
            onEnterLevel: level =>
            {
                if (level == 0)
                {
                    Die();
                }
            }, onExitLevel: null);
        PlayerStateDict[PlayerStateEnum.BodyTemperature].SetUpEvent(
            onEnterLevel: level =>
            {
                // 极度寒冷
                if (level == 0)
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, +50);
                // 极度炎热
                else if (level == 4)
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, +50);
            }, onExitLevel: level =>
            {
                if (level == 0)
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, -50);
                else if (level == 4)
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, -50);
            });
        PlayerStateDict[PlayerStateEnum.CarbonMonoxidePoisoning].SetUpEvent(
            onEnterLevel: level =>
            {
                // 轻度
                if (level == 1)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, -10);
                // 中度
                else if (level == 2)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, -30);
                // 重度
                else if (level == 3)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, -50);
            }, onExitLevel: level =>
            {
                // 轻度
                if (level == 1)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, +10);
                // 中度
                else if (level == 2)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, +30);
                // 重度
                else if (level == 3)
                    ChangePlayerMaxState(PlayerStateEnum.Oxygen, +50);
            });
        PlayerStateDict[PlayerStateEnum.Itchiness].SetUpEvent(
            onEnterLevel: level =>
            {
                // 很痒
                if (level == 1)
                {
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, +20);
                }
                // 极度瘙痒
                else if (level == 2)
                {
                    ChangePlayerMaxState(PlayerStateEnum.PainLevel, +75);
                }
            }, onExitLevel: level =>
            {
                // 很痒
                if (level == 1)
                {
                    ChangePlayerConstState(PlayerStateEnum.PainLevel, -20);
                }
                // 极度瘙痒
                else if (level == 2)
                {
                    ChangePlayerMaxState(PlayerStateEnum.PainLevel, -75);
                }
            });

        PlayerStateDict[PlayerStateEnum.Fullness].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.Thirst].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.San].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.Sobriety].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.Load].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.PainLevel].SetUpEvent();
        PlayerStateDict[PlayerStateEnum.Oxygen].SetUpEvent();
    }

    private PlayerState InitHealthState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "死亡"),
            new (0, 10, "濒死"),
            new (10, 30, "重伤"),
            new (30, int.MaxValue, "还算健康")
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };

        return new PlayerState(100, 100, PlayerStateEnum.Health, +0.4f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitFullnessState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "饥荒"),
            new (0, 10, "极度饥饿"),
            new (10, 30, "饥饿"),
            new (30, int.MaxValue, "还不饿")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityEffect = -1, healthEffect = -8 },
            new () { sanityEffect = -0.7f },
            new () { sanityEffect = -0.3f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new PlayerState(100, 100, PlayerStateEnum.Fullness, -1.2f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitThirstState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "脱水"),
            new (0, 10, "极度口渴"),
            new (10, 30, "口渴"),
            new (30, int.MaxValue, "还不渴")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityEffect = -1, healthEffect = -8 },
            new () { sanityEffect = -0.7f },
            new () { sanityEffect = -0.3f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new PlayerState(100, 100, PlayerStateEnum.Thirst, -1.5f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitSanityState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "梦魇"),
            new (0, 10, "精神崩溃"),
            new (10, 30, "精神紧张"),
            new (30, int.MaxValue, "精神正常")
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new PlayerState(100, 100, PlayerStateEnum.San, +0.1f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitOxygenState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "窒息"),
            new (0, 25, "缺氧"),
            new (25, 50, "呼吸不畅"),
            new (50, int.MaxValue, "氧气充足")
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        var effects = new List<StateEffect>()
        {
            new () { healthEffect = -7 },
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect,
        };
        return new PlayerState(60, 60, PlayerStateEnum.Oxygen, -6f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitSorbriety()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "困得要死"),
            new (0, 10, "极度疲劳"),
            new (10, 30, "疲劳"),
            new (30, int.MaxValue, "还不困")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityEffect = -4, healthEffect = -3 },
            new () { sanityEffect = -2, healthEffect = -1 },
            new () { sanityEffect = -0.5f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new PlayerState(100, 100, PlayerStateEnum.Sobriety, -1.1f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitLoadState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 15, "正常重量"),
            new (15, 18, "轻微超重"),
            new (18, 22.5f, "严重超重"),
            new (22.5f, int.MaxValue, "压得喘不过气"),
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 1, 2 };
        var highDangerLevels = new List<int>() { 3 };
        return new PlayerState(0, 30, PlayerStateEnum.Load, 0f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitBodyTemperatureState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 25, "极度寒冷"),
            new (25, 50, "寒冷"),
            new (50, 150, "体温舒适"),
            new (150, 175, "炎热"),
            new (175, int.MaxValue, "极度炎热")
        };
        var effects = new List<StateEffect>()
        {
            new () { fulnessEffect = -1.2f, healthEffect = -1 },
            new () { fulnessEffect = -0.4f },
            new () { sanityEffect = +0.2f },
            new () { thirstEffect = -0.5f },
            new () { thirstEffect = -1.5f, healthEffect = -1 },
        };
        var lowDangerLevels = new List<int>() { 1, 3 };
        var highDangerLevels = new List<int>() { 0, 4 };

        return new PlayerState(100, 200, PlayerStateEnum.BodyTemperature, 0f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitCarbonMonoxideState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 30, "正常"),
            new (30, 50, "轻度一氧化碳中毒"),
            new (50, 80, "中度一氧化碳中毒"),
            new (80, int.MaxValue, "重度一氧化碳中毒"),
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            new () { healthEffect = -0.1f },
            new () { healthEffect = -0.4f },
            new () { healthEffect = -1.2f },
        };
        var lowDangerLevels = new List<int>() { 1, 2 };
        var highDangerLevels = new List<int>() { 3 };

        return new PlayerState(0, 100, PlayerStateEnum.CarbonMonoxidePoisoning, -0.8f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitItchinessState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 50, "有点痒"),
            new (50, 75, "很痒"),
            new (75, int.MaxValue, "极度瘙痒"),
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            new () { sanityEffect = -0.1f },
            new () { sanityEffect = -0.3f },
        };
        var lowDangerLevels = new List<int>() { 1 };
        var highDangerLevels = new List<int>() { 2 };
        return new PlayerState(0, 100, PlayerStateEnum.Itchiness, -3f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private PlayerState InitPainState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 100, "不疼"),
            new (100, 200, "有点疼"),
            new (200, 300, "很疼"),
            new (300, int.MaxValue, "极度疼痛"),
        };
        var effects = new List<StateEffect>()
        {
            StateEffect.NoEffect,
            new () { sanityEffect = -0.2f },
            new () { sanityEffect = -0.6f, sorbrietyEffect = +0.5f, healthEffect = -0.5f },
            new () { sanityEffect = -2f, sorbrietyEffect = +1f, healthEffect = -1f },
        };
        var lowDangerLevels = new List<int>() { 1, 2 };
        var highDangerLevels = new List<int>() { 3 };
        return new PlayerState(0, 400, PlayerStateEnum.PainLevel, -8f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }
    #endregion

    private void InitElectricity()
    {
        Electricity = new EnvironmentState(Random.Range(30, 45), 50, EnvironmentStateEnum.Electricity);
    }

    private void InitWaterLevel()
    {
        WaterLevel = new EnvironmentState(0, 100, EnvironmentStateEnum.WaterLevel);
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
        //记录改值前的危险等级
        var lastStateLevel = PlayerStateDict[stateEnum].StateLevelName;
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        // 氧气特殊处理
        if (stateEnum == PlayerStateEnum.Oxygen)
            HandlePlayerOxygenChange(delta);
        else
            PlayerStateDict[stateEnum].AddValue(delta);
        //记录改值前的危险等级
        var curStateLevel = PlayerStateDict[stateEnum].StateLevelName;
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Player"+stateEnum, PlayerStateDict[stateEnum].CurValue.ToString()));
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Player"+stateEnum, lastStateLevel+"-"+curStateLevel));
        // 刷新前端显示
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);

        // 判断危险等级，播放音乐
        EvaluateDangerLevel();
    }

    public void ApplyPlayerEffects(Dictionary<PlayerStateEnum, float> playerEffects)
    {
        foreach (var (state, delta) in playerEffects)
        {
            ChangePlayerState(state, delta);
        }
    }

    /// <summary>
    /// 尝试从环境中获取氧气
    /// </summary>
    private void TryGainOxygenFromEnvironment(EnvironmentBag env)
    {
        // 室外环境里没有氧气
        if (!env.PlaceData.isIndoor) return;
        if (!env.StateDict.TryGetValue(EnvironmentStateEnum.Oxygen, out var oxygen)) return;
        var gain = Mathf.Min(PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity, env.StateDict[EnvironmentStateEnum.Oxygen].CurValue);
        if (gain > 0)
        {
            PlayerStateDict[PlayerStateEnum.Oxygen].AddValue(gain);
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -gain);
        }

        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
    }

    private void HandlePlayerOxygenChange(float delta)
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        // 室外环境直接改变玩家氧气，多余的就浪费
        if (!env.PlaceData.isIndoor)
        {
            PlayerStateDict[PlayerStateEnum.Oxygen].AddValue(delta);
            return;
        }

        // 每次玩家的氧气变化之前，都先尝试从环境中获取氧气
        TryGainOxygenFromEnvironment(env);

        // 玩家氧气
        var playerOxygen = PlayerStateDict[PlayerStateEnum.Oxygen];

        // 室内环境
        // 如果是消耗氧气，优先消耗环境
        if (delta < 0)
        {
            delta = -delta;
            // 环境氧气剩余量
            var envOxygen = env.StateDict[EnvironmentStateEnum.Oxygen].CurValue;
            // 要消耗的环境氧气量
            var envConsume = Mathf.Min(envOxygen, delta);
            if (envConsume > 0)
            {
                // 消耗环境氧气
                env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -envConsume);
            }
            var playerConsume = delta - envConsume;
            if (playerConsume > 0)
            {
                // 消耗玩家氧气
                playerOxygen.AddValue(-playerConsume);
            }
        }
        // 如果是补充氧气，优先补充到玩家
        else if (delta > 0)
        {
            // 计算玩家能补充多少
            var playerGain = Mathf.Min(playerOxygen.RemainingCapacity, delta);
            if (playerGain > 0)
                // 补充玩家氧气
                playerOxygen.AddValue(playerGain);
            // 计算环境能补充多少
            var envGain = delta - playerGain;
            if (envGain > 0)
                // 补充环境氧气
                env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, envGain);
        }
    }

    /// <summary>
    /// 改变玩家的额外状态
    /// </summary>
    /// <param name="stateEnum"></param>
    /// <param name="delta"></param>
    public void ChangePlayerExtraState(PlayerStateEnum stateEnum, float delta)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        if (stateEnum == PlayerStateEnum.Oxygen)
            HandleExtraOxygenChange(delta);
        else
            PlayerStateDict[stateEnum].AddExtraValue(delta);

        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }

    /// <summary>
    /// 处理额外氧气变化
    /// </summary>
    /// <param name="delta"></param>
    private void HandleExtraOxygenChange(float delta)
    {
        var env = GameManager.Instance.CurEnvironmentBag;

        var playerOxygen = PlayerStateDict[PlayerStateEnum.Oxygen];
        // 增加额外氧气
        if (delta > 0)
        {
            // 氧气上限增加
            playerOxygen.AddExtraValue(delta);
            // 尝试从当前环境中补满氧气
            TryGainOxygenFromEnvironment(env);
        }
        // 减少额外氧气
        else
        {
            // 记录原始氧气值
            var value1 = playerOxygen.CurValue;

            // 氧气上限减少
            PlayerStateDict[PlayerStateEnum.Oxygen].AddExtraValue(delta);

            // 记录当前氧气值
            var value2 = playerOxygen.CurValue;

            // 计算额外储存的氧气
            var extraOxygen = value1 - value2;

            // 额外储存氧气大于0
            if (extraOxygen > 0)
            {
                // 释放到环境里
                env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, extraOxygen);
            }
        }
    }

    public void ChangePlayerConstState(PlayerStateEnum stateEnum, float delta)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        PlayerStateDict[stateEnum].AddConstValue(delta);
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }

    public void ChangePlayerMaxState(PlayerStateEnum stateEnum, float delta)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        PlayerStateDict[stateEnum].AddMaxValue(delta);
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }
    #endregion

    #region 电力和水平面相关
    /// <summary>
    /// 改变全局电力
    /// </summary>
    /// <param name="delta"></param>
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

    /// <summary>
    /// 改变水平面
    /// </summary>
    /// <param name="delta"></param>
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

        // 睡眠时每回合+3.5清醒
        if (isSleeping)
        {
            ChangePlayerState(PlayerStateEnum.Sobriety, 3.5f);
        }
    }

    public void EnvironmentIntervalSettle()
    {
        // 每回合减少0.2电力
        ChangeElectricity(-0.2f);
    }

    public void PlayerIntervalSettle()
    {
        foreach (var (type, state) in PlayerStateDict)
        {
            if (state.BasicChangeRate != 0)
            {
                ChangePlayerState(type, state.BasicChangeRate);
            }
        }
    }

    /// <summary>
    /// 定时结算状态异常导致的额外变化
    /// </summary>
    public void ExtraPlayerIntervalSettle()
    {
        // 统计最终状态影响效果
        StateEffect finalEffect = StateEffect.NoEffect;
        foreach (var state in PlayerStateDict.Values)
        {
            finalEffect += state.GetStateEffect();
        }
        ChangePlayerState(PlayerStateEnum.Health, finalEffect.healthEffect);
        ChangePlayerState(PlayerStateEnum.San, finalEffect.sanityEffect);
        ChangePlayerState(PlayerStateEnum.Fullness, finalEffect.fulnessEffect);
        ChangePlayerState(PlayerStateEnum.Thirst, finalEffect.thirstEffect);
        ChangePlayerState(PlayerStateEnum.Sobriety, finalEffect.sorbrietyEffect);
    }
    #endregion

    #region 睡觉
    private bool isSleeping;

    public void Sleep(int time)
    {
        isSleeping = true;
        TimeManager.Instance.AddTime(time);
        isSleeping = false;
    }
    #endregion

    #region 危险状态

    /// <summary>
    /// 危险等级
    /// </summary>
    public DangerLevelEnum DangerLevel => _lastDangerLevel;

    // 缓存上次的危险状态
    private DangerLevelEnum _lastDangerLevel = DangerLevelEnum.None;

    private void EvaluateDangerLevel()
    {
        int curLevel = int.MaxValue;
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.Health].DangerLevel);
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.Fullness].DangerLevel);
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.Thirst].DangerLevel);
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.Sobriety].DangerLevel);
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.San].DangerLevel);
        curLevel = Mathf.Min(curLevel, (int)PlayerStateDict[PlayerStateEnum.Oxygen].DangerLevel);

        DangerLevelEnum danger = (DangerLevelEnum)curLevel;

        //如果上次的状态和这次一致，就不切音乐
        if (danger != _lastDangerLevel)
            PlayDangerLevelMusic(danger);

        _lastDangerLevel = danger;
    }

    //处于危险状态时，就播放心跳声，离开就播放环境音乐
    private void PlayDangerLevelMusic(DangerLevelEnum currentLevel)
    {
        // 应用低通滤波等音效变化
        SoundManager.Instance.ApplyDangerEffects(currentLevel);

        // 根据新状态处理音乐
        switch (currentLevel)
        {
            case DangerLevelEnum.None:
                SoundManager.Instance.PlayCurEnvironmentMusic();
                break;

            case DangerLevelEnum.Low:
                SoundManager.Instance.PlayBGM("心跳_01", true, 2f, 1f);
                break;

            case DangerLevelEnum.High:
                SoundManager.Instance.PlayBGM("心跳_01", true, 2f, 1.5f);
                break;
        }
    }

    #endregion

    #region 死亡逻辑

    private void Die()
    {
        ChatManager.Instance.chatWindow = WindowsManager.Instance.OpenWindow("Chat",true) as ChatWindow;
    }
    #endregion
}