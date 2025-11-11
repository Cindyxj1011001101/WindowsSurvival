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

/// <summary>
/// 实体仇恨集。用来处理仇恨的更新与排序
/// </summary>
public class EntityAggroCollection
{
    [JsonIgnore] private Dictionary<string, EntityAggro> uuidLookup = new();    // uuid和EntityAggro的对照关系，方便根据uuid更新EntityAggro
    [JsonProperty] private SortedSet<EntityAggro> sortedSet = new();            // 顺序存储的EntityAggro，方便找到优先级最高的EntityAggro
    [JsonIgnore] private IEntity belongedEntity;                                // 仇恨集合所属的实体
    [JsonProperty] private long updateOrder = 0;                                // 更新顺序。优先级相同时，updateOrder更大的更优先

    public void Init(IEntity belongedEntity)
    {
        this.belongedEntity = belongedEntity;
        foreach (var e in sortedSet)
        {
            if (uuidLookup.ContainsKey(e.TargetUuid)) continue;
            uuidLookup.Add(e.TargetUuid, e);
        }
    }

    public void UpdateRemainingMinutes()
    {
        foreach (var item in sortedSet)
        {
            item.UpdateRemainingMinutes();
        }
    }

    public void AddOrUpdate(IEntity target, int priority, int remainingMinutes, bool isPermanent)
    {
        if (uuidLookup.TryGetValue(target.Uuid, out EntityAggro existingItem))
        {
            // 更新现有项目
            sortedSet.Remove(existingItem);
            existingItem.UpdateThis(priority, remainingMinutes, isPermanent, updateOrder);
            sortedSet.Add(existingItem);
        }
        else
        {
            var newItem = new EntityAggro(target.Uuid, priority, remainingMinutes, isPermanent, updateOrder);
            // 添加新项目
            uuidLookup.Add(newItem.TargetUuid, newItem);
            sortedSet.Add(newItem);
        }

        updateOrder++;
    }

    public void RemoveByUuid(string uuid)
    {
        if (!uuidLookup.TryGetValue(uuid, out var item)) return;

        uuidLookup.Remove(uuid);
        sortedSet.Remove(item);
    }

    public void RemoveUnavailableItems()
    {
        foreach (var item in sortedSet.ToList())
        {
            // 如果仇恨目标已不存在
            if (!GlobalDataManager.Instance.ExistsEntity(item.Target))
            {
                // 将目标移出集合，继续寻找
                RemoveByUuid(item.TargetUuid);
                continue;
            }

            // 如果仇恨持续时间结束
            if (!item.IsRemaining)
            {
                // 将目标移出集合，继续寻找
                RemoveByUuid(item.TargetUuid);
                continue;
            }

            // 仇恨目标不是玩家 且 与目标不处于同一地点
            if (item.Target is not Player && !belongedEntity.IsInSameLocation(item.Target))
            {
                // 将目标移出集合，继续寻找
                RemoveByUuid(item.TargetUuid);
                continue;
            }
        }
    }

    public EntityAggro GetHighestPriority()
    {
        if (sortedSet.IsNullOrEmpty()) return null;

        return sortedSet.Max;
    }

    public void Clear()
    {
        updateOrder = 0;
        belongedEntity = null;
        uuidLookup.Clear();
        sortedSet.Clear();
    }
}