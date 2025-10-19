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
        { "一氧化碳爆炸", typeof(CarbonMonoxideExplosion) },
        { "制作激励", typeof(ProductionIncentive) },
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
        instance.triggerInterval = ExcelReader.ParseFloat(row[4].ToString());

        return instance;
    }
    #endregion

    public string eventName;         // 事件名称
    public int threatLevel;          // 威胁程度
    public float basicTriggerWeight; // 基础触发权重
    public float triggerInterval;    // 触发间隔(天)
    public int remainingTime;        // 剩余持续时间(分钟)

    [JsonIgnore] public int CoolDown => Mathf.CeilToInt(triggerInterval * 24 * 60); // 触发间隔(分钟)

    /// <summary>
    /// 计算威胁事件强度
    /// 公式：(基础值 + Min(生存天数, 生存天数最大影响上限) * 威胁系数) * 随机系数
    /// </summary>
    protected float CalculateThreatIntensity()
    {
        var config = GameEventManager.Instance.InvasionEventConfig;
        // 计算生存天数影响部分（受上限限制）
        float effectiveSurvivalDays = Mathf.Min(TimeManager.Instance.Day, config.maxSurvivalDayEffect);
        float survivalPart = effectiveSurvivalDays * config.threatCoefficient;

        // 计算基础部分
        float basePart = config.basicIntensity + survivalPart;

        // 生成随机系数
        float randomFactor = UnityEngine.Random.value * (config.maxRandomFactor - config.minRandomFactor) + config.minRandomFactor;

        // 计算最终强度
        float finalIntensity = basePart * randomFactor;

        return finalIntensity;
    }

    public virtual bool IsEventEnded() => remainingTime <= 0;

    public virtual bool CanTriggerThisEvent() => true;

    public virtual void OnTrigger() { }

    public virtual void OnUpdate() { remainingTime -= TimeManager.SETTLEMENT_INTERVAL; }

    public virtual void OnEnd() { }
}