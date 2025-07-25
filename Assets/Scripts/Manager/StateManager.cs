using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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
/// 当前危险程度
/// </summary>
public enum DangerLevelEnum
{
    None,
    Low,
    High
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
        InitPlayerState();
        InitElectricity();
        InitWaterLevel();

        EventManager.Instance.AddListener(EventType.IntervalSettle, IntervalSettle);
        // 当环境改变时尝试获取氧气
        EventManager.Instance.AddListener<EnvironmentBag>(EventType.Move, TryGainOxygenFromEnvironment);
    }

    private void Start()
    {
        // 评估危险状态，播放音乐
        _lastDangerLevel = EvaluateDangerLevel();
        PlayDangerLevelMusic(_lastDangerLevel);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, IntervalSettle);
        EventManager.Instance.RemoveListener<EnvironmentBag>(EventType.Move, TryGainOxygenFromEnvironment);
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
        if (!PlayerStateDict.ContainsKey(stateEnum)) return;

        // 氧气特殊处理
        if (stateEnum == PlayerStateEnum.Oxygen)
            HandlePlayerOxygenChange(delta);
        else
            PlayerStateDict[stateEnum].AddCurValue(delta);

        // 刷新前端显示
        EventManager.Instance.TriggerEvent(EventType.RefreshPlayerState, stateEnum);

        // 判断危险等级，播放音乐
        DangerLevelEnum dangerLevel = EvaluateDangerLevel();

        // 如果状态没有变化，直接返回
        if (dangerLevel != _lastDangerLevel)
        {
            // 更新缓存的状态
            _lastDangerLevel = dangerLevel;
            PlayDangerLevelMusic(dangerLevel);
        }
    }

    /// <summary>
    /// 尝试从环境中获取氧气
    /// </summary>
    private void TryGainOxygenFromEnvironment(EnvironmentBag env)
    {
        // 室外环境里没有氧气
        if (!env.PlaceData.isIndoor) return;

        var gain = Mathf.Min(PlayerStateDict[PlayerStateEnum.Oxygen].RemainingCapacity, env.StateDict[EnvironmentStateEnum.Oxygen].CurValue);
        if (gain > 0)
        {
            PlayerStateDict[PlayerStateEnum.Oxygen].AddCurValue(gain);
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
            PlayerStateDict[PlayerStateEnum.Oxygen].AddCurValue(delta);
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
                playerOxygen.AddCurValue(-playerConsume);
            }
        }
        // 如果是补充氧气，优先补充到玩家
        else if (delta > 0)
        {
            // 计算玩家能补充多少
            var playerGain = Mathf.Min(playerOxygen.RemainingCapacity, delta);
            if (playerGain > 0)
                // 补充玩家氧气
                playerOxygen.AddCurValue(playerGain);
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
    /// 饱食低于30，每回合-0.3精神
    /// 饱食低于10，每回合-0.7精神
    /// 饱食为0时，每回合-1精神，-8 健康
    /// </summary>
    private void ExtraFullnessChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Fullness].CurValue <= 30)
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
    /// 水分低于30，每回合-0.3精神
    /// 水分低于10，每回合-0.7精神
    /// 口渴为0时，每回合-1精神，-8健康
    /// </summary>
    private void ExtraThirstChange()
    {
        if (PlayerStateDict[PlayerStateEnum.Thirst].CurValue <= 30)
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
    /// 清醒度导致的额外变化结算
    /// 清醒度小于等于30每回合-0.5精神
    /// 清醒度小于等于10后每回合-1.5精神，-2健康
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

    #region 危险状态

    /// <summary>
    /// 危险等级
    /// </summary>
    public DangerLevelEnum DangerLevel => _lastDangerLevel;

    // 缓存上次的危险状态
    private DangerLevelEnum _lastDangerLevel = DangerLevelEnum.None;

    // 危险阈值配置
    private readonly Dictionary<PlayerStateEnum, (float high, float low)> _thresholds = new()
    {
        { PlayerStateEnum.Health, (10f, 30f) },//健康
        { PlayerStateEnum.Fullness, (10f, 30f) },//饱食
        { PlayerStateEnum.Thirst, (10f, 30f) },//水分
        { PlayerStateEnum.Sobriety, (10f, 30f) },//清醒度
        { PlayerStateEnum.San, (10f, 30f) },//精神
        { PlayerStateEnum.Oxygen, (10f, 25f) },//氧气
    };

    //一个用于判断危险状态的静态方法，会根据之前的危险阈值配置和当前状态值，来评估当前的危险等级
    private DangerLevelEnum EvaluateDangerLevel()
    {
        bool /*hasHigh = false, */hasLow = false;

        foreach (var (state, (high, low)) in _thresholds)
        {
            if (!PlayerStateDict.TryGetValue(state, out var s)) continue;

            float value = s.CurValue;
            if (value <= high) return DangerLevelEnum.High; // 发现高危立即返回
            if (value <= low) hasLow = true;
        }

        return hasLow ? DangerLevelEnum.Low : DangerLevelEnum.None;
    }

    //处于危险状态时，就播放心跳声，离开就播放环境音乐
    //如果上次的状态和这次一致，就不切音乐
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