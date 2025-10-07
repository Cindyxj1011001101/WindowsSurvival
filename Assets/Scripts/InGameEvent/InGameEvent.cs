using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 游戏内事件基类
/// </summary>
public abstract class InGameEvent
{
    public string eventName;         // 事件名称
    public int threatLevel;          // 威胁程度
    public float basicTriggerWeight; // 基础触发权重
    public float triggerInterval;    // 触发间隔(天)

    [JsonIgnore] public int TriggerIntervalMinutes => Mathf.CeilToInt(triggerInterval * 24 * 60); // 触发间隔(分钟)

    public virtual bool CanTriggerThisEvent()
    {
        return true;
    }

    public abstract void TriggerThisEvent();
}