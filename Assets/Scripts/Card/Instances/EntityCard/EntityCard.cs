using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Events;

public abstract class EntityCard : Card, IEntity
{
    protected float health => entity.value;
    protected float maxHealth => entity.maxValue;
    protected float atk => entity.atk;
    protected float moveDistPerMin => entity.moveDistPerMin;
    protected string deadDrops => entity.deadDrops;
    protected BehavioralTendency behavioralTendency => entity.behavioralTendency;

    [JsonIgnore] public Coordinate Coordinate => coordinate.coordinate;                 // 坐标
    [JsonProperty] public string Uuid { get; private set; }                             // 实体唯一标识
    [JsonProperty] protected Dictionary<string, EntityIntention> intentions = new();    // 所有可能的意图
    [JsonProperty] private string currentIntention;                                     // 当前意图
    [JsonProperty] private int aiRefreshCooldown = 0;                                   // ai刷新冷却
    [JsonProperty] private EntityAggroCollection aggroCollection = new();               // 仇恨列表

    private EntityIntention CurrentIntention
    {
        get
        {
            if (string.IsNullOrEmpty(currentIntention) || !intentions.ContainsKey(currentIntention)) return null;

            return intentions[currentIntention];
        }
    }

    public virtual void TakeDamage(float damage, IEntity damageDealer) => entity.TakeDamage(damage, damageDealer);

    protected override void OnLateConstructor()
    {
        // 设置uuid
        if (string.IsNullOrEmpty(Uuid))
            Uuid = System.Guid.NewGuid().ToString();

        // 添加坐标组件
        coordinate = new();
        AddComponent(coordinate);

        // 注册意图
        RegisterIntentions();
    }

    protected override void OnInit()
    {
        // 记录到全局数据中
        GlobalDataManager.Instance.AddEntity(this);

        // 初始化仇恨
        aggroCollection.Init(this);

        // 刷新冷却为0，且当前意图为空，说明是第一次生成
        if (aiRefreshCooldown == 0 && string.IsNullOrEmpty(currentIntention))
        {
            // 第一次生成时获取一下意图
            TryGetNewIntention();
        }

        EventManager.Instance.AddListener(EventType.AddOneMinute, EntityUpdate);
        EventManager.Instance.AddListener(EventType.PlayerMove, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        intentions.Clear();
        aggroCollection.Clear();
        GlobalDataManager.Instance.RemoveEntity(Uuid);
        EventManager.Instance.RemoveListener(EventType.AddOneMinute, EntityUpdate);
        EventManager.Instance.RemoveListener(EventType.PlayerMove, RefreshSlot);
    }

    private void EntityUpdate()
    {
        if (isUpdatePaused || Destroyed) return;

        // 先更新仇恨
        UpdateAggro();
        // 再更新AI
        UpdateAI();
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        tip = string.Empty;
        if (card.TryGetComponent<WeaponComponent>(out var weapon) && weapon.WithinAttackRange(this))
        {
            tip = $"攻击该单位\n耗时:  {weapon.attackTime}分钟\n造成伤害:  {weapon.atk}";
            return true;
        }
        return false;
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();
        card.TryGetComponent<WeaponComponent>(out var weapon);
        weapon.DealDamage(this); // 消耗时间在dealdamage方法里面处理了
    }

    #region AI
    /// <summary>
    /// 得到优先级最高的意图
    /// </summary>
    protected abstract string GetHighestPriorityIntention();

    /// <summary>
    /// 注册该实体的所有可能意图
    /// </summary>
    protected abstract void RegisterIntentions();

    protected void AddIntention(string name, int preparationMinutes, UnityAction action)
    {
        if (!intentions.ContainsKey(name))
        {
            intentions.Add(name, new(preparationMinutes));
        }
        intentions[name].action = action;
    }

    /// <summary>
    /// 更新AI逻辑
    /// </summary>
    private void UpdateAI()
    {
        // AI冷却中
        if (aiRefreshCooldown > 0)
        {
            // 更新AI冷却
            UpdateAIRefreshCooldown();
        }
        // 意图准备中
        else
        {
            // 更新当前意图
            UpdateCurrentIntention();
        }
    }

    /// <summary>
    /// 更新AI冷却
    /// </summary>
    private void UpdateAIRefreshCooldown()
    {
        aiRefreshCooldown--;
        // 冷却时间结束
        if (aiRefreshCooldown <= 0)
        {
            aiRefreshCooldown = 0;
            // 重新获取意图
            TryGetNewIntention();
        }
    }

    /// <summary>
    /// 尝试获取新意图
    /// </summary>
    private void TryGetNewIntention()
    {
        // 获取最高优先级意图
        currentIntention = GetHighestPriorityIntention();

        // 没有可执行的意图
        if (CurrentIntention == null)
        {
            // 重置ai冷却
            aiRefreshCooldown = entity.aiRefreshInterval;
        }
        // 有可执行意图
        else
        {
            // 开始准备意图
            CurrentIntention.Prepare();
        }
    }

    /// <summary>
    /// 更新当前意图
    /// </summary>
    private void UpdateCurrentIntention()
    {
        // 更新意图执行倒计时
        CurrentIntention.UpdateExecutionCountdown();
        if (CurrentIntention.IsReady)
        {
            // 倒计时完成，执行意图
            CurrentIntention.Execute();
            // 刷新意图
            TryGetNewIntention();
        }
    }
    #endregion

    #region 仇恨
    /// <summary>
    /// 添加仇恨
    /// </summary>=
    protected void AddAggro(IEntity entity, int priority, int remainingMinutes)
    {
        if (entity == null) return;
        aggroCollection.AddOrUpdate(entity, priority, remainingMinutes, false);
    }

    /// <summary>
    /// 添加永久仇恨
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="priority"></param>
    protected void AddPermanentAggro(IEntity entity, int priority)
    {
        if (entity == null) return;
        aggroCollection.AddOrUpdate(entity, priority, default, true);
    }

    /// <summary>
    /// 获取仇恨目标
    /// </summary>
    protected EntityAggro GetAggroTarget()
    {
        // 先清除无效的仇恨目标
        aggroCollection.RemoveUnavailableItems();
        // 再取最高优先级
        return aggroCollection.GetHighestPriority();
    }

    ///// <summary>
    ///// 移除仇恨实体
    ///// </summary>
    //protected void RemoveAggroEntity(IEntity entity) => aggroCollection.RemoveByUuid(entity.Uuid);

    ///// <summary>
    ///// 清除无效的仇恨目标
    ///// </summary>
    //protected void RemoveUnavailableAggroEntities() => aggroCollection.RemoveUnavailableItems();

    /// <summary>
    /// 更新仇恨
    /// </summary>
    private void UpdateAggro()
    {
        // 更新仇恨持续时间
        aggroCollection.UpdateRemainingMinutes();
        // 遍历当前地点的实体并判断其是否是仇恨目标
        foreach (var entity in (Bag as EnvironmentBag).AllEntities)
        {
            // 如果是仇恨目标，则添加到集合
            TryAddAggro(entity);
        }
    }

    /// <summary>
    /// 派生类重写的仇恨逻辑
    /// </summary>
    protected virtual void TryAddAggro(IEntity entity) { }
    #endregion
}
