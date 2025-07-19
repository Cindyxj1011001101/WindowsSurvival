using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

public enum CardTag
{
    Rubbish, // 垃圾
}

public enum CardType
{
    Food,//食物
    Tool,//工具
    Resource,//资源
    Place,//地点
    ResourcePoint,//资源点
    Equipment,//装备
    Creature,//生物
    Construction,//建筑
    Other,//其他
}


//卡牌基类
public abstract class Card : IComparable<Card>
{
    [JsonIgnore]
    public string cardName; // 显示名称
    [JsonIgnore]
    public string cardDesc; // 描述
    [JsonIgnore]
    public CardType cardType; // 卡牌类型
    [JsonIgnore]
    public int maxStackNum; // 最大堆叠数
    [JsonIgnore]
    public bool moveable; // 能否移动
    [JsonIgnore]
    public float weight; // 重量
    [JsonIgnore]
    public List<CardTag> tags = new(); // 标签
    [JsonIgnore]
    public List<Event> events = new(); // 可交互事件
    public Dictionary<Type, ICardComponent> components = new();

    [JsonIgnore]
    public CardSlot slot;

    [JsonIgnore]
    public virtual Sprite CardImage
    {
        get => Resources.Load<Sprite>("Sprites/" + cardName);
    }

    /// <summary>
    /// 每回合结算时执行
    /// </summary>
    protected virtual Action OnUpdate { get; } = null;

    public void SetCardSlot(CardSlot slot)
    {
        this.slot = slot;
    }

    /// <summary>
    /// 开始监听每回合的结算
    /// </summary>
    public void StartUpdating()
    {
        if (OnUpdate != null)
            EventManager.Instance.AddListener(EventType.IntervalSettle, OnUpdate);
    }

    public void StopUpdating()
    {
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, OnUpdate);
    }

    public virtual void TryUse()
    {
        if (TryGetComponent<DurabilityComponent>(out var component))
        {
            if (component.durability <= 0) return;

            component.durability--;
            if (component.durability <= 0)
                DestroyThis();
            else
                slot.RefreshCurrentDisplay();
        }
    }

    public virtual void DestroyThis()
    {
        slot.RemoveCard(this);
        StopUpdating();
    }

    public bool TryGetComponent<T>(out T component) where T : ICardComponent
    {
        if (components.TryGetValue(typeof(T), out var comp))
        {
            component = (T)comp;
            return true;
        }

        component = default;
        return false;
    }

    public int CompareTo(Card other)
    {
        if (other.GetType() != GetType()) return 0;
        if (TryGetComponent<FreshnessComponent>(out var a))
        {
            // 新鲜度低的优先
            other.TryGetComponent<FreshnessComponent>(out var o);
            return a.freshness - o.freshness;
        }
        else if (TryGetComponent<ProgressComponent>(out var b))
        {
            // 产物进度高的优先
            other.TryGetComponent<ProgressComponent>(out var o);
            return o.progress - b.progress;
        }
        else if (TryGetComponent<DurabilityComponent>(out var c))
        {
            // 耐久度低的优先
            other.TryGetComponent<DurabilityComponent>(out var o);
            return c.durability - o.durability;
        }
        else
        {
            return 0;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"卡牌名称: {cardName}");
        sb.AppendLine($"卡牌描述: {cardDesc}");
        sb.AppendLine($"卡牌类型: {cardType}");
        sb.AppendLine($"最大堆叠数: {maxStackNum}");
        sb.AppendLine($"可移动: {moveable}");
        sb.AppendLine($"重量: {weight}");
        sb.AppendLine($"标签: {string.Join(", ", tags)}");
        sb.AppendLine($"事件数量: {events.Count}");
        foreach (var ev in events)
        {
            sb.AppendLine($"  - 事件名称: {ev.name}, 描述: {ev.description}");
        }
        sb.AppendLine($"组件数量: {components.Count}");
        foreach (var kvp in components)
        {
            sb.AppendLine($"  - 组件类型: {kvp.Key.Name}, 实例: {kvp.Value}");
        }
        return sb.ToString();
    }
}

//事件类
public class Event
{
    public string name;
    public string description;
    public UnityAction action;
    public Func<bool> condition;

    public Event(string name, string description, UnityAction action, Func<bool> condition)
    {
        this.name = name;
        this.description = description;
        this.action = action;
        this.condition = condition;
    }

    public void Inovke()
    {
        action?.Invoke();
    }

    public bool Judge()
    {
        if (condition == null) return true;

        return condition.Invoke();
    }
}
