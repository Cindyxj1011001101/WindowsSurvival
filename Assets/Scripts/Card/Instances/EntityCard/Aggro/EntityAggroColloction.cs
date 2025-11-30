using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
            if (item.Target == null)
            {
                // 将目标移出集合，继续寻找
                RemoveByUuid(item.TargetUuid);
                continue;
            }

            // 如果仇恨目标被摧毁或锁定
            if (item.Target is EntityCard c && (c.Destroyed || c.Locked))
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