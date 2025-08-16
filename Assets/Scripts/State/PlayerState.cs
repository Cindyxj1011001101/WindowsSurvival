using Newtonsoft.Json;
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
    Sobriety,
    Load,
    CarbonMonoxidePoisoning,
    Itchiness,
    PainLevel,
    BodyTemperature,
}

/// <summary>
/// 玩家状态类
/// </summary>
public class PlayerState
{
    [JsonProperty] private float extraValue; // 额外值
    [JsonProperty] private float maxValue; // 最大值
    [JsonProperty] private float constValue; // 固定值
    [JsonProperty] private float variableValue; // 可变值
    [JsonProperty] public PlayerStateEnum stateEnum;
    [JsonProperty] private List<StateThreshold> thresholds = new();
    [JsonProperty] private List<StateEffect> effects = new();
    [JsonProperty] private int stateLevel = -1;
    [JsonProperty] public float basicChangeRate;
    [JsonProperty] private float extraChangeRate;
    [JsonProperty] private List<int> lowDangerLevels = new();
    [JsonProperty] private List<int> highDangerLevels = new();

    [JsonIgnore] public string StateLevelName => thresholds[stateLevel].levelName;
    [JsonIgnore] public int StateLevel => stateLevel;
    [JsonIgnore] public float CurValue => Mathf.Clamp(variableValue + constValue, 0, MaxValue);
    [JsonIgnore] public float ExtraValue => extraValue;
    [JsonIgnore] public float MaxValue => maxValue + extraValue;
    [JsonIgnore] public float RemainingCapacity => MaxValue - CurValue;
    [JsonProperty] public float BasicChangeRate => basicChangeRate;
    [JsonIgnore] public float ChangeRate => basicChangeRate + extraChangeRate;

    [JsonIgnore]
    public DangerLevelEnum DangerLevel
    {
        get
        {
            if (highDangerLevels.Contains(stateLevel)) return DangerLevelEnum.High;
            if (lowDangerLevels.Contains(stateLevel)) return DangerLevelEnum.Low;
            return DangerLevelEnum.None;
        }
    }

    //[JsonIgnore]
    //private UnityAction<int> onEnterLevel;

    //[JsonIgnore]
    //private UnityAction<int> onExitLevel;

    public void AddValue(float delta)
    {
        variableValue += delta;
        variableValue = Mathf.Clamp(variableValue, 0, MaxValue);

        CalcStateLevel();
    }

    public void AddExtraValue(float delta)
    {
        extraValue += delta;
        variableValue = Mathf.Clamp(variableValue, 0, MaxValue);

        CalcStateLevel();
    }

    public void AddConstValue(float delta)
    {
        constValue += delta;

        CalcStateLevel();
    }

    public void AddMaxValue(float delta)
    {
        maxValue += delta;

        CalcStateLevel();
    }

    private void CalcStateLevel()
    {
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (CurValue > thresholds[i].minValue && CurValue <= thresholds[i].maxValue)
            {
                // 如果状态等级发生了变化
                if (stateLevel != i)
                {
                    // 离开stateLevel事件
                    //onExitLevel?.Invoke(stateLevel);
                    if (stateLevel != -1)
                        effects[stateLevel].Revoke();
                    // 进入i事件
                    //onEnterLevel?.Invoke(i);
                    effects[i].Apply();
                    stateLevel = i;
                }
                break;
            }
        }
    }

    //public StateEffect GetStateEffect()
    //{
    //    return effects[stateLevel];
    //}

    public void AddChangeRate(float delta)
    {
        extraChangeRate += delta;
    }

    public void SetBasicChangeRate(float value)
    {
        basicChangeRate = value;
    }

    public PlayerState(float value, float maxValue, PlayerStateEnum state, float basicChangeRate,
        List<StateThreshold> thresholds, List<StateEffect> effects,
        List<int> lowDangerLevels, List<int> highDangerLevels)
    {
        constValue = 0;
        extraValue = 0;
        variableValue = value;
        this.maxValue = maxValue;
        stateEnum = state;
        this.thresholds = thresholds;
        this.effects = effects;
        this.basicChangeRate = basicChangeRate;
        extraChangeRate = 0;
        this.lowDangerLevels = lowDangerLevels;
        this.highDangerLevels = highDangerLevels;
        CalcStateLevel();
    }

    //public void SetUpEvent(UnityAction<int> onEnterLevel = null, UnityAction<int> onExitLevel = null)
    //{
    //    this.onEnterLevel = onEnterLevel;
    //    this.onExitLevel = onExitLevel;
    //    CalcStateLevel();
    //}
}

// 状态阈值配置
[System.Serializable]
public class StateThreshold
{
    public float minValue;
    public float maxValue;
    public string levelName;

    public StateThreshold(float minValue, float maxValue, string levelName)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.levelName = levelName;
    }
}

// 状态效果配置
[System.Serializable]
public class StateEffect
{
    public static StateEffect NoEffect = new();

    // 每回合变化
    public float healthRate;      // 健康影响
    public float sanityRate;      // 精神影响
    public float fulnessRate;     // 饱食影响
    public float thirstRate;      // 水分影响
    public float sorbrietyRate;   // 清醒度影响

    // 瞬间变化
    public float oxygenMax;       // 氧气上限
    public float painConst;       // 疼痛固定值

    private void ApplyEffects(bool forward)
    {
        int signal = forward ? 1 : -1;

        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, healthRate * signal);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, sanityRate * signal);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Fullness, fulnessRate * signal);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Thirst, thirstRate * signal);
        StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, sorbrietyRate * signal);

        StateManager.Instance.ChangePlayerMaxState(PlayerStateEnum.Oxygen, oxygenMax * signal);
        StateManager.Instance.ChangePlayerConstState(PlayerStateEnum.PainLevel, painConst * signal);
    }

    public void Apply()
    {
        ApplyEffects(true);
    }

    public void Revoke()
    {
        ApplyEffects(false);
    }
}