using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    //[JsonProperty]
    //private float curValue; // 当前值

    [JsonProperty]
    private float extraValue; // 额外值

    [JsonProperty]
    private float maxValue; // 最大值

    [JsonProperty]
    private float constValue; // 固定值

    [JsonProperty]
    private float variableValue; // 可变值

    [JsonProperty]
    public PlayerStateEnum stateEnum;

    [JsonProperty]
    private List<StateThreshold> thresholds = new();

    [JsonProperty]
    private List<StateEffect> effects = new();

    [JsonProperty]
    private int stateLevel = -1;

    [JsonProperty]
    public float BasicChangeRate { get; private set; }

    [JsonIgnore]
    public string StateLevelName => thresholds[stateLevel].levelName;

    [JsonIgnore]
    public int StateLevel => stateLevel;

    [JsonIgnore]
    public float CurValue => Mathf.Clamp(variableValue + constValue, 0, MaxValue);

    [JsonIgnore]
    public float ExtraValue => extraValue;

    [JsonIgnore]
    public float MaxValue => maxValue + extraValue;

    [JsonIgnore]
    public float RemainingCapacity => MaxValue - CurValue;

    [JsonIgnore]
    private UnityAction<int> onEnterLevel;

    [JsonIgnore]
    private UnityAction<int> onExitLevel;

    public void AddValue(float delta)
    {
        variableValue += delta;
        variableValue = Mathf.Clamp(variableValue, 0, MaxValue);
        //curValue = Mathf.Clamp(variableValue + constValue, 0, MaxValue);

        CalcStateLevel();
    }

    public void AddExtraValue(float delta)
    {
        extraValue += delta;
        variableValue = Mathf.Clamp(variableValue, 0, MaxValue);
        //curValue = Mathf.Clamp(variableValue + constValue, 0, MaxValue);

        CalcStateLevel();
    }

    public void AddConstValue(float delta)
    {
        constValue += delta;
        //curValue = Mathf.Clamp(variableValue + constValue, 0, MaxValue);

        CalcStateLevel();
    }

    public void AddMaxValue(float delta)
    {
        maxValue += delta;
        //curValue = Mathf.Clamp(variableValue + constValue, 0, MaxValue);

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
                    onExitLevel?.Invoke(stateLevel);
                    // 进入i事件
                    onEnterLevel?.Invoke(i);
                }
                stateLevel = i;
                break;
            }
        }
    }

    public StateEffect GetStateEffect()
    {
        return effects[stateLevel];
    }

    public PlayerState(float value, float maxValue, PlayerStateEnum state, float basicChangeRate,
        List<StateThreshold> thresholds, List<StateEffect> effects)
    {
        constValue = 0;
        extraValue = 0;
        variableValue = value;
        this.maxValue = maxValue;
        stateEnum = state;
        this.thresholds = thresholds;
        this.effects = effects;
        BasicChangeRate = basicChangeRate;
    }

    public void SetUpEvent(UnityAction<int> onEnterLevel = null, UnityAction<int> onExitLevel = null)
    {
        this.onEnterLevel = onEnterLevel;
        this.onExitLevel = onExitLevel;
        CalcStateLevel();
    }
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

    public float healthEffect;      // 健康影响
    public float sanityEffect;      // 精神影响
    public float fulnessEffect;     // 饱食影响
    public float thirstEffect;      // 水分影响
    public float sorbrietyEffect;   // 清醒度影响

    public static StateEffect operator +(StateEffect a, StateEffect b)
    {
        return new StateEffect()
        {
            healthEffect = a.healthEffect + b.healthEffect,
            sanityEffect = a.sanityEffect + b.sanityEffect,
            fulnessEffect = a.fulnessEffect + b.fulnessEffect,
            thirstEffect = a.thirstEffect + b.thirstEffect,
            sorbrietyEffect = a.sorbrietyEffect + b.sorbrietyEffect,
        };
    }
}