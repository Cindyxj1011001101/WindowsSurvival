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
/// 玩家状态类
/// </summary>
public class State
{
    [JsonProperty] private float extraValue; // 额外值
    [JsonProperty] private float maxValue; // 最大值
    [JsonProperty] private float constValue; // 固定值
    [JsonProperty] private float variableValue; // 可变值
    [JsonProperty] private List<StateThreshold> thresholds = new();
    [JsonProperty] private List<StateEffect> effects = new();
    [JsonProperty] private int stateLevel = -1;
    [JsonProperty] private float basicChangeRate;
    [JsonProperty] private float extraChangeRate;
    [JsonProperty] private List<int> lowDangerLevels = new();
    [JsonProperty] private List<int> highDangerLevels = new();
    [JsonProperty] private float normParam = 0;

    [JsonIgnore] public string StateLevelName => thresholds[stateLevel].levelName;
    [JsonIgnore] public int StateLevel => stateLevel;
    [JsonIgnore] public float CurValue => Mathf.Clamp(variableValue + constValue, 0, MaxValue);
    [JsonIgnore] public float NormedValue => CurValue + normParam;
    [JsonIgnore] public float ExtraValue => extraValue;
    [JsonIgnore] public float MaxValue => maxValue + extraValue;
    [JsonIgnore] public float RemainingCapacity => MaxValue - CurValue;
    [JsonIgnore] public float BasicChangeRate => basicChangeRate;
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

    public float GetPredictedVariableValue()
    {
        return variableValue + basicChangeRate + extraChangeRate;
    }

    private void ClampVariableValue()
    {
        variableValue = Mathf.Clamp(variableValue, 0, MaxValue);
        variableValue = System.MathF.Round(variableValue, 1); // 四舍五入到一位小数
    }

    public void AddValue(float delta)
    {
        variableValue += delta;
        ClampVariableValue();

        CalcStateLevel();
    }

    public void AddExtraValue(float delta)
    {
        extraValue += delta;
        ClampVariableValue();

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

    public void CalcStateLevel()
    {
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (CurValue > thresholds[i].minValue && CurValue <= thresholds[i].maxValue)
            {
                // 如果状态等级发生了变化
                if (stateLevel != i)
                {
                    var oLevel = stateLevel; // 原来处于哪个等级
                    stateLevel = i;
                    // 离开stateLevel事件
                    if (oLevel != -1)
                        effects[oLevel].Revoke();
                    // 进入i事件
                    effects[stateLevel].Apply();
                }
                break;
            }
        }
    }

    public void AddChangeRate(float delta)
    {
        extraChangeRate += delta;
    }

    public void SetBasicChangeRate(float value)
    {
        basicChangeRate = value;
    }

    public State() { }

    public State(float value, float maxValue, float basicChangeRate,
        List<StateThreshold> thresholds, List<StateEffect> effects,
        List<int> lowDangerLevels, List<int> highDangerLevels, float normParam = 0)
    {
        constValue = 0;
        extraValue = 0;
        variableValue = value;
        this.maxValue = maxValue;
        this.thresholds = thresholds;
        this.effects = effects;
        this.basicChangeRate = basicChangeRate;
        extraChangeRate = 0;
        this.lowDangerLevels = lowDangerLevels;
        this.highDangerLevels = highDangerLevels;
        this.normParam = normParam;
    }

    public State(float value, float maxValue, float basicChangeRate = 0, float normParam = 0) : this(value, maxValue, basicChangeRate, new(), new(), new(), new(), normParam) { }
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
    public float bodyTemperatureRate;         // 体温影响
    public float carbonMonoxidePoisoningRate; // 一氧化碳中毒影响

    // 瞬间变化
    public float oxygenMax;       // 氧气上限
    public float painConst;       // 疼痛固定值

    private void ApplyEffects(bool forward)
    {
        int signal = forward ? 1 : -1;

        // 每回合变化
        if (healthRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Health, healthRate * signal);
        if (sanityRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.San, sanityRate * signal);
        if (fulnessRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Fullness, fulnessRate * signal);
        if (thirstRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Thirst, thirstRate * signal);
        if (sorbrietyRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.Sobriety, sorbrietyRate * signal);
        if (carbonMonoxidePoisoningRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.CarbonMonoxidePoisoning, carbonMonoxidePoisoningRate * signal);
        if (bodyTemperatureRate != 0) StateManager.Instance.ChangePlayerStateChangeRate(PlayerStateEnum.BodyTemperature, bodyTemperatureRate * signal);

        // 瞬时变化
        if (oxygenMax != 0) StateManager.Instance.ChangePlayerMaxState(PlayerStateEnum.Oxygen, oxygenMax * signal);
        if (painConst != 0) StateManager.Instance.ChangePlayerConstState(PlayerStateEnum.PainLevel, painConst * signal);
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