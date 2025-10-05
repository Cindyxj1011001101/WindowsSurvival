using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 当前危险程度
/// </summary>
public enum DangerLevelEnum
{
    High = 0,
    Low = 1,
    None = 2,
}

public class StateManager : MonoBehaviour
{
    /// <summary>
    /// 玩家状态
    /// </summary>
    public Dictionary<PlayerStateEnum, State> PlayerStateDict { get; private set; } = new();

    /// <summary>
    /// 电力
    /// </summary>
    public State Electricity { get; private set; }

    /// <summary>
    /// 飞船水平面高度
    /// </summary>
    public State WaterLevel { get; private set; }

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
        // 当环境改变时尝试获取氧气
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.ChangeEnv, OnChangeEnv);
        // 玩家生命值不高于0时死亡
        EventManager.Instance.AddListener<PlayerStateEnum>(EventType.RefreshPlayerState, CheckPlayerState);
    }

    private void CheckPlayerState(PlayerStateEnum stateEnum)
    {
        if (PlayerStateDict[PlayerStateEnum.Health].CurValue <= 0) Die();
    }

    private void Start()
    {
        // 评估危险状态，播放音乐
        EvaluateDangerLevel();

        // 监听回合结算
        UpdateManager.Instance.PlayerUpdate.AddListener(PlayerUpdate);
        UpdateManager.Instance.EnvironmentUpdate.AddListener(EnvironmentUpdate);
    }

    private void OnDestroy()
    {
        UpdateManager.Instance.PlayerUpdate.RemoveListener(PlayerUpdate);
        UpdateManager.Instance.EnvironmentUpdate.RemoveListener(EnvironmentUpdate);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.ChangeEnv, OnChangeEnv);
        EventManager.Instance.RemoveListener<PlayerStateEnum>(EventType.RefreshPlayerState, CheckPlayerState);
    }

    private void OnChangeEnv(EnvironmentBag env)
    {
        TryGainOxygenFromEnvironment(env);

        CalcBodyTemperatureChangeRate(env);

        CalcCarbonMonoxidePoisoningChangeRate(env);
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
        PlayerStateDict.Add(PlayerStateEnum.Sobriety, InitSobriety());
        PlayerStateDict.Add(PlayerStateEnum.Load, InitLoadState());
        PlayerStateDict.Add(PlayerStateEnum.BodyTemperature, InitBodyTemperatureState());
        PlayerStateDict.Add(PlayerStateEnum.CarbonMonoxidePoisoning, InitCarbonMonoxideState());
        PlayerStateDict.Add(PlayerStateEnum.Itchiness, InitItchinessState());
        PlayerStateDict.Add(PlayerStateEnum.PainLevel, InitPainState());

        foreach (var state in PlayerStateDict.Values)
        {
            state.CalcStateLevel();
        }
    }

    private State InitHealthState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "死亡"),
            new (0, 10, "濒死"),
            new (10, 40, "重伤"),
            new (40, int.MaxValue, "还算健康")
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

        return new State(100, 150, +0.5f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitFullnessState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "饥荒"),
            new (0, 20, "极度饥饿"),
            new (20, 50, "饥饿"),
            new (50, int.MaxValue, "还不饿")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityRate = -1, healthRate = -8 },
            new () { sanityRate = -0.7f ,healthRate = -0.5f},
            new () { sanityRate = -0.1f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new State(100, 250, -1f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitThirstState()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "脱水"),
            new (0, 20, "极度口渴"),
            new (20, 50, "口渴"),
            new (50, int.MaxValue, "还不渴")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityRate = -1, healthRate = -8 },
            new () { sanityRate = -0.7f ,healthRate = -0.5f},
            new () { sanityRate = -0.1f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new State(100, 200, -1.3f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitSanityState()
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
        return new State(100, 100, +0.1f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitOxygenState()
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
            new () { healthRate = -7 },
            StateEffect.NoEffect,
            StateEffect.NoEffect,
            StateEffect.NoEffect,
        };
        return new State(60, 60, -6f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitSobriety()
    {
        var thresholds = new List<StateThreshold>()
        {
            new (-1, 0, "困得要死"),
            new (0, 20, "极度疲劳"),
            new (20, 50, "疲劳"),
            new (50, int.MaxValue, "还不困")
        };
        var effects = new List<StateEffect>()
        {
            new () { sanityRate = -4, healthRate = -3 },
            new () { sanityRate = -1.5f, healthRate = -0.8f },
            new () { sanityRate = -0.3f },
            StateEffect.NoEffect
        };
        var lowDangerLevels = new List<int>() { 2 };
        var highDangerLevels = new List<int>() { 0, 1 };
        return new State(150, 180, -1, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitLoadState()
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
        return new State(0, 30, 0f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitBodyTemperatureState()
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
            new () { fulnessRate = -1.2f, healthRate = -1, bodyTemperatureRate = +2f, painConst = +50 },
            new () { fulnessRate = -0.4f, bodyTemperatureRate = +1f },
            new () { sanityRate = +0.2f },
            new () { thirstRate = -0.5f, bodyTemperatureRate = -1f },
            new () { thirstRate = -1.5f, healthRate = -1, bodyTemperatureRate = -2f, painConst = +50 },
        };
        var lowDangerLevels = new List<int>() { 1, 3 };
        var highDangerLevels = new List<int>() { 0, 4 };

        return new State(100, 200, 0f, thresholds, effects, lowDangerLevels, highDangerLevels, -100);
    }

    private State InitCarbonMonoxideState()
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
            new () { healthRate = -0.1f, oxygenMax = -10 },
            new () { healthRate = -0.4f, oxygenMax = -30 },
            new () { healthRate = -1.2f, oxygenMax = -50 },
        };
        var lowDangerLevels = new List<int>() { 1, 2 };
        var highDangerLevels = new List<int>() { 3 };

        return new State(0, 100, -0.3f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitItchinessState()
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
            new () { sanityRate = -0.1f, painConst = +20 },
            new () { sanityRate = -0.3f, painConst = +75 },
        };
        var lowDangerLevels = new List<int>() { 1 };
        var highDangerLevels = new List<int>() { 2 };
        return new State(0, 100, -1f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }

    private State InitPainState()
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
            new () { sanityRate = -0.2f },
            new () { sanityRate = -0.6f, sorbrietyRate = +0.5f, healthRate = -0.5f },
            new () { sanityRate = -2.5f, sorbrietyRate = +1f, healthRate = -1f },
        };
        var lowDangerLevels = new List<int>() { 1, 2 };
        var highDangerLevels = new List<int>() { 3 };
        return new State(0, 400, -2f, thresholds, effects, lowDangerLevels, highDangerLevels);
    }
    #endregion

    private void InitElectricity()
    {
        Electricity = new(Random.Range(30, 45), 50);
    }

    private void InitWaterLevel()
    {
        WaterLevel = new(0, 100);
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
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Player" + stateEnum, PlayerStateDict[stateEnum].CurValue.ToString()));
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("Player" + stateEnum, lastStateLevel + "-" + curStateLevel));
        // 刷新前端显示
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);

        // 判断危险等级，播放音乐
        EvaluateDangerLevel();
    }

    public void ChangePlayerStateChangeRate(PlayerStateEnum stateEnum, float delta)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;
        PlayerStateDict[stateEnum].AddChangeRate(delta);
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }

    public void SetPlayerStateBasicChangeRate(PlayerStateEnum stateEnum, float value)
    {
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;
        PlayerStateDict[stateEnum].SetBasicChangeRate(value);
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);
    }

    public void ApplyPlayerStateChange(Dictionary<PlayerStateEnum, float> playerEffects)
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
        // 没有氧气则返回
        if (!env.StateDict.TryGetValue(EnvironmentStateEnum.Oxygen, out var envOxygen)) return;

        var playerOxygen = PlayerStateDict[PlayerStateEnum.Oxygen];
        var gain = Mathf.Min(playerOxygen.RemainingCapacity, envOxygen.CurValue);
        if (gain > 0)
        {
            playerOxygen.AddValue(gain);
            env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -gain);
        }

        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, PlayerStateEnum.Oxygen);
    }

    private void HandlePlayerOxygenChange(float delta)
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        // 环境没有氧气属性，则直接改变玩家氧气，多余的就浪费
        if (!env.StateDict.TryGetValue(EnvironmentStateEnum.Oxygen, out var envOxygen))
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
            // 要消耗的环境氧气量
            var envConsume = Mathf.Min(envOxygen.CurValue, delta);
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
        Electricity.AddValue(delta);
        // 刷新前端显示
        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.Electricity)
        {
            stateValue = Electricity
        });
    }

    public void ChangeElectricityChangeRate(float delta)
    {
        Electricity.AddChangeRate(delta);

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
        WaterLevel.AddValue(delta);
        // 触发水平面变化事件
        //EventManager.Instance.TriggerEvent(EventType.ChangeWaterLevel, WaterLevel.CurValue);
        EventManager.Instance.TriggerEvent(EventType.DialogueCondition, new SubscribeActionArgs("WaterLevel", WaterLevel.CurValue.ToString()));

        if (WaterLevel.CurValue >= 100) Die(); // 水平面升高至100，游戏结束

        // 刷新前端显示
        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.WaterLevel)
        {
            stateValue = WaterLevel
        });
    }

    public void ChangeWaterLevelChangeRate(float delta)
    {
        WaterLevel.AddChangeRate(delta);

        var env = GameManager.Instance.CurEnvironmentBag;
        EventManager.Instance.TriggerEvent(EventType.RefreshEnvironmentState, new RefreshEnvironmentStateArgs(env.PlaceData.placeType, EnvironmentStateEnum.WaterLevel)
        {
            stateValue = WaterLevel
        });
    }
    #endregion

    #region 定时结算相关
    /// <summary>
    /// 定时结算环境状态
    /// </summary>
    private void EnvironmentUpdate()
    {
        ChangeElectricity(Electricity.ChangeRate);
        ChangeWaterLevel(WaterLevel.ChangeRate);
    }

    private Dictionary<PlayerStateEnum, float> temp = new(); // 记录玩家状态的当前变化率，防止玩家状态的结算顺序影响结算结果

    /// <summary>
    /// 定时结算玩家状态
    /// </summary>
    private void PlayerUpdate()
    {
        CalcBodyTemperatureChangeRate(GameManager.Instance.CurEnvironmentBag);
        CalcCarbonMonoxidePoisoningChangeRate(GameManager.Instance.CurEnvironmentBag);

        temp.Clear();
        foreach (var (type, state) in PlayerStateDict)
        {
            if (state.ChangeRate != 0)
            {
                //ChangePlayerState(type, state.ChangeRate);
                temp.Add(type, state.ChangeRate);
            }
        }

        ApplyPlayerStateChange(temp);
    }

    /// <summary>
    /// 计算室温导致的体温变化
    /// </summary>
    /// <param name="env"></param>
    private void CalcBodyTemperatureChangeRate(EnvironmentBag env)
    {
        // 温度差 = 室温 - 体温
        var diff = env.StateDict[EnvironmentStateEnum.RoomTemperature].NormedValue - PlayerStateDict[PlayerStateEnum.BodyTemperature].NormedValue;

        float rate;
        if (diff < -50)
        {
            rate = -4f;
        }
        else if (diff < -30)
        {
            rate = -3f;
        }
        else if (diff < -10)
        {
            rate = -2f;
        }
        else if (diff < -5)
        {
            rate = -1f;
        }
        else if (diff <= 5)
        {
            rate = 0f;
        }
        else if (diff <= 10)
        {
            rate = 1f;
        }
        else if (diff <= 30)
        {
            rate = 2f;
        }
        else if (diff <= 50)
        {
            rate = 3f;
        }
        else
        {
            rate = 4f;
        }

        SetPlayerStateBasicChangeRate(PlayerStateEnum.BodyTemperature, rate);
    }

    /// <summary>
    /// 计算室内一氧化碳浓度导致的一氧化碳中毒影响
    /// </summary>
    /// <param name="env"></param>
    private void CalcCarbonMonoxidePoisoningChangeRate(EnvironmentBag env)
    {
        float basicRate = -0.3f;

        if (!env.StateDict.ContainsKey(EnvironmentStateEnum.CarbonMonoxideLevel))
        {
            SetPlayerStateBasicChangeRate(PlayerStateEnum.CarbonMonoxidePoisoning, basicRate);
            return;
        }

        // 温度差 = 室温 - 体温
        var value = env.StateDict[EnvironmentStateEnum.CarbonMonoxideLevel].NormedValue;

        float rate;
        if (value <= 0)
        {
            rate = 0f;
        }
        else if (value <= 25)
        {
            rate = +.5f;
        }
        else if (value <= 50)
        {
            rate = +1f;
        }
        else if (value <= 75)
        {
            rate = +1.7f;
        }
        else
        {
            rate = +3f;
        }
        SetPlayerStateBasicChangeRate(PlayerStateEnum.CarbonMonoxidePoisoning, rate + basicRate);
    }
    #endregion

    #region 休息
    public void Rest(int time, Dictionary<PlayerStateEnum, float> playerStateBasicChangeRates)
    {
        // 记录当前变化率
        var current = new Dictionary<PlayerStateEnum, float>();
        foreach (var state in playerStateBasicChangeRates.Keys)
        {
            if (PlayerStateDict.TryGetValue(state, out var value))
            {
                current.Add(state, value.BasicChangeRate);
            }
        }

        // 应用新的变化率
        foreach (var (state, basicChangeRate) in playerStateBasicChangeRates)
        {
            SetPlayerStateBasicChangeRate(state, basicChangeRate);
        }

        // 触发开始睡觉事件
        EventManager.Instance.TriggerEvent(EventType.StartSleeping);

        // 时间增加
        TimeManager.Instance.AddTime(time);

        // 触发结束睡觉事件
        EventManager.Instance.TriggerEvent(EventType.StopSleeping);

        // 恢复原来的变化率
        foreach (var (state, basicChangeRate) in current)
        {
            SetPlayerStateBasicChangeRate(state, basicChangeRate);
        }
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
    public void PlayDangerLevelMusic(DangerLevelEnum currentLevel)
    {
        SoundManager.Instance.ApplyDangerEffects(currentLevel);
    }

    #endregion

    #region 死亡逻辑

    private void Die()
    {
        ChatManager.Instance.chatWindow = WindowsManager.Instance.OpenWindow("Chat",true) as ChatWindow;
    }
    #endregion

    public static string ParsePlayerState(PlayerStateEnum playerState)
    {
        return playerState switch
        {
            PlayerStateEnum.Health => "健康",
            PlayerStateEnum.Fullness => "饱食度",
            PlayerStateEnum.Thirst => "水分",
            PlayerStateEnum.San => "精神",
            PlayerStateEnum.Oxygen => "氧气",
            PlayerStateEnum.Sobriety => "清醒",
            PlayerStateEnum.Load => "载重",
            PlayerStateEnum.BodyTemperature => "体温",
            PlayerStateEnum.CarbonMonoxidePoisoning => "一氧化碳中毒",
            PlayerStateEnum.Itchiness => "瘙痒",
            PlayerStateEnum.PainLevel => "疼痛",
            _ => playerState.ToString(),
        };
    }
}