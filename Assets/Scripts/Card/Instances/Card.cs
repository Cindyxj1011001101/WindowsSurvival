using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CardTag
{
    Rubbish, // 垃圾
    Cut, // 切割类
    Dig, // 挖掘类
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
}


//卡牌基类
public abstract class Card : IComparable<Card>
{
    public string cardName; // 显示名称
    public string cardDesc; // 描述
    //public Sprite cardImage; // 根据名称在文件夹中找图片
    public CardType cardType; // 卡牌类型
    public int maxStackNum; // 最大堆叠数
    public bool moveable; // 能否移动
    public float weight; // 重量
    public List<CardTag> tags; // 标签
    public List<Event> events; // 可交互事件
    public int maxEndurance; // 最大耐久度
    public int curEndurance; // 当前耐久度

    public Dictionary<Type, ICardComponent> components = new();

    [JsonIgnore]
    public CardSlot slot;

    [JsonIgnore]
    public Sprite CardImage => Resources.Load<Sprite>("Sprites/" + cardName);

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

    public virtual void Use()
    {
        // 无限耐久
        if (maxEndurance < 0) return;

        // 耐久--
        curEndurance--;
        if (curEndurance <= 0)
            // 销毁卡牌
            DestroyThis();
        else
            // 刷新前端显示的卡牌数据
            slot.RefreshCurrentDisplay();
    }

    public void DestroyThis()
    {
        slot.RemoveCard(this);
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, OnUpdate);
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
            other.TryGetComponent<FreshnessComponent>(out var b);
            return a.freshness - b.freshness;
        }
        else
        {
            // 耐久度低的优先
            return curEndurance - other.curEndurance;
        }
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
