using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// 游戏内事件基类
/// </summary>
public abstract class GameEvent
{
    #region 读取
    private static Dictionary<string, Type> eventNameTypeDict = new()
    {
        { "入侵", typeof(Invasion) },
        { "行星磁暴", typeof(MagneticStorm) },
        { "裂缝", typeof(CracksAppear) },
        { "鼠患", typeof(RatInfestation) },
        { "灵光乍现", typeof(FlashOfInspiration) },
        { "呕吐", typeof(Vomit) },
        { "太空垃圾", typeof(SpaceJunk) },
        { "泥沙涌动", typeof(SedimentSurge) },
        { "恒星食", typeof(StellarEclipse) },
        { "CO爆炸", typeof(COExplosion) },
        { "制作激励", typeof(CraftIncentive) },
        { "移动激励", typeof(MovementIncentive) },
    };

    public static GameEvent ParseDataRow(DataRow row)
    {
        var eventName = row[0].ToString();
        if (string.IsNullOrEmpty(eventName)) return null; // 如果事件名称为空，跳过读取

        if (!eventNameTypeDict.TryGetValue(eventName, out Type eventType))
        {
            Debug.LogError($"未知的事件名称: {eventName}");
            return null;
        }

        GameEvent instance = (GameEvent)Activator.CreateInstance(eventType);
        instance.eventName = eventName;
        instance.threatLevel = ExcelReader.ParseInt(row[1].ToString());
        instance.basicTriggerWeight = ExcelReader.ParseFloat(row[2].ToString());
        instance.totalDaysCondition = ExcelReader.ParseFloat(row[4].ToString());
        instance.repeatIntervalDays = ExcelReader.ParseFloat(row[5].ToString());

        return instance;
    }
    #endregion

    [JsonProperty] private string eventName;            // 事件名称
    [JsonProperty] private int threatLevel;             // 威胁程度
    [JsonProperty] private float basicTriggerWeight;    // 基础触发权重
    [JsonProperty] private float totalDaysCondition;    // 天数限制，小于天数不能触发
    [JsonProperty] private float repeatIntervalDays;    // 重复触发间隔(天)
    [JsonProperty] private int remainingMinutes;        // 剩余持续时间(分钟)
    [JsonProperty] private int remainingCoolDown;       // 剩余冷却时间(分钟)

    [JsonIgnore] public int ThreatLevel => threatLevel;
    [JsonIgnore] public float BasicTriggerWeight => basicTriggerWeight;
    [JsonIgnore] public string EventName => eventName;

    protected void SetRemainingMinutes(int remainingMinutes)
    {
        this.remainingMinutes = remainingMinutes;
    }

    /// <summary>
    /// 计算威胁事件强度
    /// 公式：(基础值 + Min(生存天数, 生存天数最大影响上限) * 威胁系数) * 随机系数
    /// </summary>
    protected float CalculateThreatIntensity()
    {
        var config = GameEventManager.Instance.InvasionEventConfig;
        // 计算生存天数影响部分（受上限限制）
        float effectiveSurvivalDays = Mathf.Min(TimeManager.Instance.Days, config.maxSurvivalDayEffect);
        float survivalPart = effectiveSurvivalDays * config.threatCoefficient;

        // 计算基础部分
        float basePart = config.basicIntensity + survivalPart;

        // 生成随机系数
        float randomFactor = UnityEngine.Random.value * (config.maxRandomFactor - config.minRandomFactor) + config.minRandomFactor;

        // 计算最终强度
        float finalIntensity = basePart * randomFactor;

        return finalIntensity;
    }

    public bool IsOngoing() => remainingMinutes > 0;

    public bool IsInCoolDown() => remainingCoolDown > 0;

    public bool IsTotalDaysConditionMet() => TimeManager.Instance.TotalDays > totalDaysCondition;

    public bool IsReady() => !IsOngoing() && !IsInCoolDown() && IsTotalDaysConditionMet() && CanTriggerThisEvent();

    public void Trigger()
    {
        // 设置冷却时间
        remainingCoolDown = Mathf.CeilToInt(repeatIntervalDays * 24 * 60);
        OnTrigger();
        EventManager.Instance.TriggerEvent(EventType.OnGameEventTrigger, this);
        Debug.Log($"触发事件：{eventName}，持续时间：{remainingMinutes}分钟");
    }

    public void Update()
    {
        if (IsOngoing())
        {
            remainingMinutes -= TimeManager.SETTLEMENT_INTERVAL;
            if (remainingMinutes <= 0)
            {
                // 持续时间结束
                remainingMinutes = 0;
                // 调用结束处理
                OnEnd();
                EventManager.Instance.TriggerEvent(EventType.OnGameEventEnd, this);
                Debug.Log($"事件结束：{eventName}");
            }
        }

        if (IsInCoolDown())
        {
            remainingCoolDown -= TimeManager.SETTLEMENT_INTERVAL;
            if (remainingCoolDown <= 0)
            {
                remainingCoolDown = 0;
            }
        }

        OnUpdate();
    }

    protected virtual bool CanTriggerThisEvent() => true;

    protected virtual void OnTrigger() { }

    protected virtual void OnUpdate() { }

    protected virtual void OnEnd() { }

    public abstract string GetDetails();
}