using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
    #region 属性
    [JsonProperty]
    public string CardId { get; private set; } // 卡牌ID

    [JsonProperty]
    protected Dictionary<Type, CardComponent> components = new();

    [JsonIgnore]
    public List<Event> Events { get; protected set; } = new(); // 可交互事件

    [JsonIgnore]
    public string CardName => CardFactory.GetCardName(CardId);

    [JsonIgnore]
    public string ExtraInfo => CardFactory.GetExtraInfo(CardId);

    [JsonIgnore]
    public string CardDesc => CardFactory.GetCardDesc(CardId);

    [JsonIgnore]
    public CardType CardType => CardFactory.GetCardType(CardId);

    [JsonIgnore]
    public int MaxStackNum => CardFactory.GetMaxStackNum(CardId);

    [JsonIgnore]
    public bool Moveable => CardFactory.GetMoveable(CardId);

    [JsonIgnore]
    public float Weight
    {
        get
        {
            // 卡牌重量 = 自身重量 + 内容物重量 * 减重率
            float weight = CardFactory.GetWeight(CardId);
            if (TryGetComponent<InnerContentsComponent>(out var component))
            {
                foreach (var slot in component.bag.Slots)
                {
                    foreach (var card in slot.Cards)
                    {
                        weight += card.Weight * component.weightLossRate;
                    }
                }
            }
            return weight;
        }
    }

    [JsonIgnore]
    public List<CardTag> Tags => CardFactory.GetTags(CardId);

    [JsonIgnore]
    public Sprite CardImage => CardFactory.GetCardImage(CardId);

    [JsonIgnore]
    public bool IsBigIcon => CardFactory.GetIsBigIcon(CardId);

    [JsonIgnore]
    public Card ParentCard { get; protected set; } = null; // 父卡牌，用于被作为内容物的卡牌

    [JsonIgnore]
    public bool Destroyed { get; private set; } = false;

    [JsonIgnore]
    /// 是否有循环音效，默认无
    public virtual bool HasLoopSound => false;

    [JsonIgnore]
    public SlotCards SlotCards { get; protected set; } = null;

    [JsonIgnore]
    public Bag Bag => SlotCards?.Bag;

    [JsonIgnore]
    public CardSlot Slot => SlotCards?.CardSlot;
    #endregion

    /// <summary>
    /// 每回合结算时执行
    /// </summary>
    protected virtual Action OnUpdate { get; } = null;

    public virtual void OnAdded(Bag bag) { }
    public virtual void OnRemoved(Bag bag) { }

    public void SetCardId(string cardId)
    {
        CardId = cardId;
    }

    public void SetSlotCards(SlotCards slotCards)
    {
        SlotCards = slotCards;
    }

    public void SetParentCard(Card parentCard)
    {
        ParentCard = parentCard;
    }

    /// <summary>
    /// 用于在卡牌实例化后进行额外的初始化操作
    /// </summary>
    protected virtual void LateInit()
    {
        if (TryGetComponent<InnerContentsComponent>(out var i))
        {
            i.contentFilter = ReflectionUtility.BindToDelegate<CardFilterDelegate>(this, "ContentFilter", true);
            ReflectionUtility.SetFieldValue(this, "innerContents", i, true);
        }
    }

    private bool isUpdating = false; // 是否已启用每回合更新

    /// <summary>
    /// 开始监听每回合的结算
    /// </summary>
    public void StartUpdating()
    {
        if (isUpdating) return;

        isUpdating = true;

        foreach (var c in components.Values)
        {
            c.SetBelongedCard(this);
        }

        LateInit();

        if (OnUpdate != null)
            EventManager.Instance.AddListener(EventType.IntervalSettle, OnUpdate);

        // 如果有内部内容组件，则开始监听内部内容的更新
        if (TryGetComponent<InnerContentsComponent>(out var component))
        {
            component.Init();
        }
    }

    /// <summary>
    /// 结束监听每回合的结算
    /// </summary>
    public void StopUpdating()
    {
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, OnUpdate);
    }

    public virtual void Use(int times = 1)
    {
        if (TryGetComponent<DurabilityComponent>(out var component))
        {
            for (int i = 0; i < times; i++)
            {
                component.Use(DestroyThis);
            }
        }
    }

    public void RefreshSlot()
    {
        if (Slot != null) Slot.RefreshDisplay();
        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }

    public void ShowTip(string tip)
    {
        if (Slot != null) Slot.ShowTip(tip);
    }

    public void ShowTip(string tip, Color textColor)
    {
        if (Slot != null) Slot.ShowTip(tip, textColor);
    }

    public virtual void DestroyThis()
    {
        if (Destroyed) return;

        Destroyed = true;

        StopUpdating();

        SlotCards.RemoveCard(this);
        RefreshSlot();

        ParentCard?.RefreshSlot();

        EventManager.Instance.TriggerEvent(EventType.ChangeCardProperty, this);
    }

    #region 拖动交互
    /// <summary>
    /// 能否拖动交互
    /// </summary>
    /// <param name="card">被拿起的卡牌</param>
    /// <returns></returns>
    public virtual bool CanQuickInteract(Card card) { return false; }

    /// <summary>
    /// 拖动交互的具体逻辑
    /// </summary>
    /// <param name="slot">被拿起的卡牌对应的SlotCards</param>
    /// <param name="count">需要快捷交互的卡牌数量</param>
    public virtual void QuickIneract(SlotCards slot, int count, out string tip) { tip = string.Empty; }
    #endregion

    /// <summary>
    /// 进入当前环境时（如玩家进入该卡牌所在地点）（通常用于播放卡牌对应的循环音）
    /// </summary>
    public virtual void OnEnterEnvironment() { }

    /// <summary>
    /// 离开当前环境时（如玩家离开该卡牌所在地点）
    /// </summary>
    public virtual void OnLeaveEnvironment() { }

    /// <summary>
    /// 打开卡牌详情时（通常用于调大卡牌对应的循环音）
    /// </summary>
    public virtual void OnDetailOpen() { }

    /// <summary>
    /// 关闭卡牌详情时（通常用于调小卡牌对应的循环音）
    /// </summary>
    public virtual void OnDetailClose() { }

    /// <summary>
    /// 获取卡牌的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="component"></param>
    /// <returns></returns>
    public bool TryGetComponent<T>(out T component) where T : CardComponent
    {
        if (components.TryGetValue(typeof(T), out var comp))
        {
            component = (T)comp;
            return true;
        }

        component = default;
        return false;
    }

    public void AddComponent(CardComponent component)
    {
        if (components.ContainsKey(component.GetType())) return;

        component.SetBelongedCard(this);
        components.Add(component.GetType(), component);
    }

    /// <summary>
    /// 继承其他卡牌的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="other"></param>
    public void InheritComponent<T>(Card other) where T : CardComponent
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
        sb.AppendLine($"卡牌名称: {CardName}");
        sb.AppendLine($"卡牌描述: {CardDesc}");
        sb.AppendLine($"卡牌类型: {CardType}");
        sb.AppendLine($"最大堆叠数: {MaxStackNum}");
        sb.AppendLine($"可移动: {Moveable}");
        sb.AppendLine($"重量: {Weight}");
        sb.AppendLine($"标签: {string.Join(", ", Tags)}");
        sb.AppendLine($"事件数量: {Events.Count}");
        foreach (var ev in Events)
        {
            sb.AppendLine($"  - 事件名称: {ev.name}");
        }
        sb.AppendLine($"组件数量: {components.Count}");
        foreach (var kvp in components)
        {
            sb.AppendLine($"  - 组件类型: {kvp.Key.Name}, 实例: {kvp.Value}");
        }
        return sb.ToString();
    }

    // 卡牌的临时位置，用来处理从临时位置处发出一张卡牌的动效，例如从详情窗口的slot处
    private Transform transform;

    [JsonIgnore]
    public Transform Transform
    {
        get
        {
            if (transform != null)
                return transform;


            return Slot == null ? null : Slot.transform;
        }
        set
        {
            transform = value;
        }
    }

    protected Tween AddCard(string cardId, bool toPlayerBag, out Card card)
    {
        return GameManager.Instance.AddCardWithTween(cardId, Transform.position, toPlayerBag, out card);
    }

    protected Tween AddCard(string cardId, bool toPlayerBag)
    {
        return GameManager.Instance.AddCardWithTween(cardId, Transform.position, toPlayerBag, out _);
    }

    protected Tween AddCards(string cardId, int count, bool toPlayerBag, out List<Card> cards)
    {
        return GameManager.Instance.AddCardsWithTween(cardId, count, Transform.position, toPlayerBag, out cards);
    }

    protected Tween AddCards(string cardId, int count, bool toPlayerBag)
    {
        return GameManager.Instance.AddCardsWithTween(cardId, count, Transform.position, toPlayerBag, out _);
    }

    protected void AddCard(string cardId, Bag targetBag)
    {
        AddCard(CardFactory.CreateCard(cardId), targetBag);
    }

    protected void AddCard(Card card, Bag targetBag)
    {
        // 尝试放在targetBag里
        if (targetBag.CanAddCard(card, out _))
        {
            // 成功放置
            var transform = Transform;
            if (transform == null && ParentCard != null)
                transform = ParentCard.Transform;
            // 当前卡牌和其父卡牌都没有显示在场景里
            if (transform == null)
                // 没有动效直接添加
                GameManager.Instance.AddCard(card, targetBag);
            else
                // 添加并且播放动效
                GameManager.Instance.AddCardWithTween(card, targetBag, transform.position);

        }
        // 放不下看targetBag是不是内容物背包
        else if (targetBag is InnerBag innerBag)
        {
            // 是的话尝试放在内容物背包的父物体所在的背包里
            AddCard(card, innerBag.BelongedCard.Bag);
        }
        // 否则放在当前环境里
        else
        {
            AddCard(card, GameManager.Instance.CurEnvironmentBag);
        }
    }
}

//事件类
public class Event
{
    public string name;
    public string description;
    public string hint;
    public OutStringAction action;
    public OutStringAction<bool> condition;
    public Func<int> getTimeEffect;
    public Func<Dictionary<PlayerStateEnum, float>> getPlayerEffects;
    public Func<Dictionary<EnvironmentStateEnum, float>> getEnvEffects;

    public string Description => string.IsNullOrEmpty(hint) ? description : hint;

    public Event(string name, string description, OutStringAction action, OutStringAction<bool> condition,
        Func<int> getTimeEffect = null, Func<Dictionary<PlayerStateEnum, float>> getPlayerEffects = null, Func<Dictionary<EnvironmentStateEnum, float>> getEnvEffects = null)
    {
        this.name = name;
        this.description = description;
        this.action = action;
        this.condition = condition;
        this.getTimeEffect = getTimeEffect;
        this.getPlayerEffects = getPlayerEffects;
        this.getEnvEffects = getEnvEffects;
    }

    public void Inovke(out string tip)
    {
        if (action != null)
            action.Invoke(out tip);
        else
            tip = string.Empty;
    }

    public bool Judge()
    {
        hint = string.Empty;
        if (condition == null || condition.Invoke(out hint))
        {
            return true;
        }

        return false;
    }

    public int GetTimeEffect()
    {
        if (getTimeEffect == null) return 0;
        return getTimeEffect.Invoke();
    }

    public Dictionary<PlayerStateEnum, float> GetPlayerEffects()
    {
        if (getPlayerEffects == null) return null;
        return getPlayerEffects.Invoke();
    }

    public Dictionary<EnvironmentStateEnum, float> GetEnvEffects()
    {
        if (getEnvEffects == null) return null;
        return getEnvEffects.Invoke();
    }
}

public delegate T OutStringAction<T>(out string s);
public delegate void OutStringAction(out string s);
