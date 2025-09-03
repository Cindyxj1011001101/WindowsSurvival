using DG.Tweening;
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
    Food, // 食物
    Tool, // 工具
    Resource, // 资源
    Place, // 地点
    ResourcePoint, // 资源点
    Equipment, // 装备
    Creature, // 生物
    Construction, // 建筑
    Other, // 其他
    Crop, // 作物
    Seed, // 种子
    Liquids​, // 液体
    Medicine​, // 药品
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
    public virtual string ExtraInfo
    {
        get
        {
            if (TryGetComponent<StateMachineComponent>(out var s))
            {
                return s.currentStateName;
            }
            else
            {
                return CardFactory.GetExtraInfo(CardId);
            }
        }
    }

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
                        weight += card.Weight * (1 - component.weightLossRate);
                    }
                }
            }
            return weight;
        }
    }

    [JsonIgnore]
    public List<CardTag> Tags => CardFactory.GetTags(CardId);

    [JsonIgnore]
    public Sprite CardImage
    {
        get
        {
            if (TryGetComponent<StateMachineComponent>(out var stateMachine) && !string.IsNullOrEmpty(stateMachine.CurrentState.imagePath))
            {
                return CardFactory.GetCardImage(CardId, stateMachine.CurrentState.imagePath);
            }
            else
            {
                return CardFactory.GetCardImage(CardId);
            }
        }
    }

    [JsonIgnore]
    public bool IsBigIcon => CardFactory.GetIsBigIcon(CardId);

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

    private void Update()
    {
        if (!isUpdatePaused && !Destroyed)
            OnUpdate();
    }

    /// <summary>
    /// 每回合结算时执行
    /// </summary>
    protected virtual void OnUpdate()
    {
        foreach (var component in components.Values)
        {
            if (component is IUpdate update) update.Update();
        }
    }

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

    /// <summary>
    /// 用于在卡牌实例化后进行额外的初始化操作
    /// </summary>
    public virtual void LateInit()
    {
        if (TryGetComponent<InnerContentsComponent>(out var i))
        {
            i.contentFilter = ReflectionUtility.BindToDelegate<CardFilterDelegate>(this, "ContentFilter", true);
            ReflectionUtility.SetFieldValue(this, "innerContents", i, true);
        }
        if (TryGetComponent<FreshnessComponent>(out var f))
        {
            if (CardId == "磁性触手" || CardId == "熟触手")
                f.onRotton = () => AddCard("废金属", Bag);
            else
                f.onRotton = () => AddCard("腐烂物", Bag);
        }
        if (TryGetComponent<ProgressComponent>(out var p))
        {
            p.onProgressFull = () => AddCard($"有产物的{CardName}", Bag);
        }
        if (TryGetComponent<PlantGrowthComponent>(out var pg))
        {
            pg.onDead = () => AddCard(pg.deadCardId, Bag);
        }
    }

    private bool isUpdating = false; // 是否已启用每回合更新

    [JsonProperty] private bool isUpdatePaused = false; // 是否暂停每回合更新

    /// <summary>
    /// 开始监听每回合的结算
    /// </summary>
    public void StartUpdating()
    {
        if (isUpdating) return;

        isUpdating = true;

        LateInit();

        foreach (var c in components.Values)
        {
            c.SetBelongedCard(this);
        }

        EventManager.Instance.AddListener(EventType.IntervalSettle, Update);

        // 如果有内容物组件，则开始监听内容物的更新
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
        EventManager.Instance.RemoveListener(EventType.IntervalSettle, Update);
    }

    public void PauseUpdating()
    {
        isUpdatePaused = true;
    }

    public void ContinueUpdating()
    {
        isUpdatePaused = false;
    }

    public virtual void Use(int times = 1, UnityAction onBroken = null)
    {
        if (TryGetComponent<DurabilityComponent>(out var component))
        {
            int usedTimes = Mathf.Min(times, component.durability);
            for (int i = 0; i < usedTimes; i++)
            {
                component.Use(() =>
                {
                    DestroyThis();
                    onBroken?.Invoke();
                });
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

    public virtual void DestroyThis()
    {
        if (Destroyed) return;

        Destroyed = true;

        StopUpdating();

        SlotCards.RemoveCard(this);
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

    public void AddComponent(CardComponent newComponent)
    {
        if (components.ContainsKey(newComponent.GetType())) return;

        components.Add(newComponent.GetType(), newComponent);
        newComponent.SetBelongedCard(this);
    }

    public void RemoveComponent<T>() where T : CardComponent
    {
        if (!components.ContainsKey(typeof(T))) return;

        components.Remove(typeof(T));
    }

    /// <summary>
    /// 继承其他卡牌的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="other"></param>
    public bool InheritComponent<T>(Card other, out T newComponent) where T : CardComponent
    {
        // 如果other有该组件，并且当前卡牌也有该组件，则复制一份
        if (other.TryGetComponent<T>(out var component) && TryGetComponent<T>(out _))
        {
            newComponent = JsonManager.DeepCopy(component);
            components[typeof(T)] = newComponent;
            newComponent.SetBelongedCard(this);
            return true;
        }
        newComponent = null;
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
        else if (other.TryGetComponent<PlantGrowthComponent>(out var p))
        {
            // 生长度高的优先
            other.TryGetComponent<PlantGrowthComponent>(out var o);
            return Mathf.CeilToInt(o.growthProgress - p.growthProgress);
        }
        else if (other.TryGetComponent<GrowthComponent>(out var g))
        {
            // 生长度高的优先
            other.TryGetComponent<GrowthComponent>(out var o);
            return Mathf.CeilToInt(o.growth - g.growth);
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
            if (transform != null) return transform;

            if (Slot != null) return Slot.transform;

            return null;
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

    protected Tween AddCards(List<Card> cards, bool toPlayerBag)
    {
        return GameManager.Instance.AddCardsWithTween(cards, Transform.position, toPlayerBag);
    }

    protected void AddCard(string cardId, Bag targetBag)
    {
        AddCard(cardId, targetBag, out _);
    }

    protected void AddCard(string cardId, Bag targetBag, out Card card)
    {
        card = CardFactory.CreateCard(cardId);
        AddCard(card, targetBag);
    }

    protected void AddCard(Card card, Bag targetBag)
    {
        // 尝试放在targetBag里
        if (targetBag.CanAddCard(card, out _))
        {
            // 成功放置
            var transform = Transform;
            if (transform == null && Bag is InnerBag innerBag && innerBag.BelongedCard != null)
                transform = innerBag.BelongedCard.Transform;
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
        tip = string.Empty;
        action?.Invoke(out tip);
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
