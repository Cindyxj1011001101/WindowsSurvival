using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

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
    public string cardId; // 卡牌ID
    public string cardName; // 显示名称
    public string cardDesc; // 描述
    public string imagePath; // 图片路径
    public CardType cardType; // 卡牌类型
    public int maxStackNum; // 最大堆叠数
    public bool moveable; // 能否移动
    public float weight; // 重量
    public List<CardTag> tags = new(); // 标签
    public Dictionary<Type, ICardComponent> components = new();

    [JsonIgnore]
    public List<Event> events = new(); // 可交互事件

    [JsonIgnore]
    public CardSlot slot;

    [JsonIgnore]
    public Sprite CardImage => Resources.Load<Sprite>("Sprites/" + imagePath);

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

    /// <summary>
    /// 结束监听每回合的结算
    /// </summary>
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

    /// <summary>
    /// 获取卡牌的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 继承其他卡牌的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="other"></param>
    public void InheritComponent<T>(Card other) where T : ICardComponent
    {
        // 如果other有该组件，并且当前卡牌也有该组件，则复制一份
        if (other.TryGetComponent<T>(out var component) && TryGetComponent<T>(out _))
            components[typeof(T)] = JsonManager.DeepCopy(component);
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
