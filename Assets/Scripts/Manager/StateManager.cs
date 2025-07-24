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

    private void Start()
    {
        InitPlayerState();
        InitElectricity();
        InitWaterLevel();
        _lastDangerLevel = EvaluateDangerLevel();
    }

    private void OnDestroy()
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
        {
            var env = GameManager.Instance.CurEnvironmentBag;
            // 如果获取氧气时在室内环境
            if (env.PlaceData.isIndoor)
            {
                // 如果是消耗氧气，优先消耗环境
                if (delta < 0)
                {

                }
                // 如果是补充氧气，优先补充到玩家
                if (delta > 0)
                {
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
        }

        PlayerStateDict[stateEnum].AddCurValue(delta);

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