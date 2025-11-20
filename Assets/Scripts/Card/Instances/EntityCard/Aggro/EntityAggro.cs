using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 实体仇恨
/// </summary>
public class EntityAggro : IComparable<EntityAggro>
{
    [JsonProperty] private string targetUuid;       // 仇恨实体的uuid
    [JsonProperty] private int priority;            // 仇恨优先级
    [JsonProperty] private int remainingMinutes;    // 剩余时间
    [JsonProperty] private bool isPermanent;        // 是否永久持续
    [JsonProperty] private long updateOrder;        // 加入顺序

    [JsonIgnore] public string TargetUuid => targetUuid;
    [JsonIgnore] public int Priority => priority;
    [JsonIgnore] public bool IsRemaining => isPermanent || remainingMinutes > 0;
    [JsonIgnore] public IEntity Target => GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

    public EntityAggro() { }

    public EntityAggro(string targetUuid, int priority, int remainingMinutes, bool isPermanent, long updateOrder)
    {
        this.targetUuid = targetUuid;
        this.priority = priority;
        this.remainingMinutes = remainingMinutes;
        this.isPermanent = isPermanent;
        this.updateOrder = updateOrder;
    }

    public void UpdateThis(int priority, int remainingMinutes, bool isPermanent, long updateOrder)
    {
        this.priority = Mathf.Max(this.priority, priority);
        this.remainingMinutes = Mathf.Max(this.remainingMinutes, remainingMinutes);
        this.updateOrder = updateOrder;
        this.isPermanent = isPermanent || this.isPermanent;
    }

    public void UpdateRemainingMinutes()
    {
        remainingMinutes = Mathf.Max(remainingMinutes - 1, 0);
    }

    public int CompareTo(EntityAggro other)
    {
        if (this == null && other == null) return 0;
        if (this == null) return 1;
        if (other == null) return -1;

        // 首先比较优先级（降序）
        int priorityComparison = other.priority.CompareTo(priority);
        if (priorityComparison != 0)
            return priorityComparison;

        // 优先级相同，比较更新顺序（降序 - 最近更新的优先）
        int timestampComparison = other.updateOrder.CompareTo(updateOrder);
        if (timestampComparison != 0)
            return timestampComparison;

        // 理论上updateOrder是不可能相同的

        // 如果时间和优先级都相同，使用UUID确保稳定性
        return string.Compare(targetUuid, other.targetUuid, StringComparison.Ordinal);
    }
}
