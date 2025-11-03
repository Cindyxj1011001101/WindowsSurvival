using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Events;

public abstract class EntityCard : Card, IEntity
{
    private EntityComponent entity;
    private CoordinateComponent coordinate;

    protected float health => entity.value;
    protected float maxHealth => entity.maxValue;
    protected float atk => entity.atk;
    protected float moveDistPerMin => entity.moveDistPerMin;
    protected string deadDrops => entity.deadDrops;

    [JsonIgnore] public Coordinate Coordinate => coordinate.coordinate;

    [JsonProperty] private bool firstInit;              // 是否第一次初始化完成

    [JsonProperty] protected Dictionary<string, EntityIntention> intentions = new(); // 所有可能的意图

    [JsonProperty] private string currentIntention;     // 当前意图

    [JsonProperty] private int aiRefreshCooldown;       // ai刷新冷却

    private EntityIntention CurrentIntention
    {
        get
        {
            if (string.IsNullOrEmpty(currentIntention) || !intentions.ContainsKey(currentIntention)) return null;

            return intentions[currentIntention];
        }
    }

    public void TakeDamage(float damage, IEntity damageDealer) => entity.TakeDamage(damage, damageDealer);

    public override void LateConstrcutor()
    {
        base.LateConstrcutor();

        TryGetComponent(out entity);
        if (!TryGetComponent(out coordinate))
        {
            coordinate = new();
            AddComponent(coordinate);
        }

        // 注册意图
        RegisterIntentions();
    }

    public override void Init()
    {
        base.Init();
        EventManager.Instance.AddListener(EventType.AddOneMinute, UpdateAI);
        EventManager.Instance.AddListener(EventType.PlayerMove, RefreshSlot);
    }

    protected override void OnDestroy()
    {
        EventManager.Instance.RemoveListener(EventType.AddOneMinute, UpdateAI);
        EventManager.Instance.RemoveListener(EventType.PlayerMove, RefreshSlot);
    }

    public override void OnAdd(Bag bag)
    {
        base.OnAdd(bag);

        if (!firstInit)
        {
            firstInit = true;
            // 第一次生成时，判断一下意图
            TryGetNewIntention();
        }
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
        if (Destroyed) return;

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
        // 为true时表示意图准备完毕，并且已经执行
        if (CurrentIntention.UpdatePreparationCountdown())
        {
            // 当前意图已执行，刷新意图
            TryGetNewIntention();
        }
    }
    #endregion
}
