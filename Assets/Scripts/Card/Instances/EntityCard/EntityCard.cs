using Newtonsoft.Json;
using UnityEngine;

public abstract class EntityCard : Card, IEntity
{
    protected float health => entity.value;
    protected float maxHealth => entity.maxValue;
    protected float atk => entity.atk;
    protected float moveDistPerMin => entity.moveDistPerMin;
    protected string deadDrops => entity.deadDrops;
    protected BehavioralTendency behavioralTendency => entity.behavioralTendency;

    [JsonProperty] private EntityIntention currentIntention = null;             // 当前意图
    [JsonProperty] private int aiRefreshCooldown = 0;                           // ai刷新冷却
    [JsonProperty] private EntityAggroCollection aggroCollection = new();       // 仇恨列表

    [JsonIgnore] public Coordinate Coordinate => coordinate.coordinate;         // 坐标
    [JsonIgnore] public EntityIntention CurrentIntention => currentIntention;   // 当前意图

    public virtual void TakeDamage(float damage, IEntity damageDealer) => entity.TakeDamage(damage, damageDealer);

    protected override void RegisterCardEvents()
    {
        // 注册所有武器的攻击
        foreach (var c in CardFactory.GetStaticCardInstancesByComponent<WeaponComponent>())
        {
            c.TryGetComponent<WeaponComponent>(out var weapon);
            var weaponName = c.CardName;
            var weaponAtk = weapon.atk;
            var weaponAtkTime = weapon.attackTime;
            AddCardEvent($"用{weaponName}攻击", $"用{weaponName}攻击{CardName}\n造成伤害:  {weaponAtk}",
                e => BeAttacked(GameManager.Instance.PlayerBag.FindCardOfName(weaponName)),
                (out string s) => CanBeAttacked(weaponName, out s),
                () => weaponAtkTime,
                shouldHideThis: () => GameManager.Instance.PlayerBag.FindCardOfName(weaponName) == null); // 没有对应武器则隐藏交互
        }

        // 注册空手攻击
        AddCardEvent($"空手攻击", $"用双手攻击{CardName}\n造成伤害:  {Player.Instance.Atk}",
                e => Player.Instance.DealDamage(this),
                (out string s) => Player.Instance.CanAttack(this, out s),
                () => Player.Instance.AttackTime);
    }

    protected override void OnLateConstructor()
    {
        // 添加坐标组件
        coordinate = new();
        AddComponent(coordinate);
    }

    protected override void OnInit()
    {
        // 记录到全局数据中
        GlobalDataManager.Instance.CreateEntity(this);

        // 加入当前地点中
        (Bag as EnvironmentBag).AddEntity(this);

        // 初始化仇恨集
        aggroCollection.Init(this);

        // 设置意图的执行者
        currentIntention?.SetBelongedEntity(this);

        // 刷新冷却为0，且当前意图为空，说明是第一次生成
        if (aiRefreshCooldown == 0 && currentIntention == null)
        {
            // 第一次生成时获取一下意图
            TryGetNewIntention();
        }

        // 监听每分钟的实体更新
        UpdateManager.Instance.AddEntityUpdateListener(ref updateOrder, OnEntityUpdate);
    }

    protected override void OnDestroy()
    {
        aggroCollection.Clear();
        GlobalDataManager.Instance.DestroyEntity(this);
        UpdateManager.Instance.RemoveEntityUpdateListener(updateOrder);
    }

    private bool CanBeAttacked(string weaponName, out string s)
    {
        var weaponCard = GameManager.Instance.PlayerBag.FindCardOfName(weaponName);
        if (weaponCard == null)
        {
            s = $"需要{weaponName}";
            return false;
        }

        weaponCard.TryGetComponent<WeaponComponent>(out var weapon);
        return weapon.CanAttack(this, out s);
    }

    private void BeAttacked(Card weaponCard)
    {
        weaponCard.TryGetComponent<WeaponComponent>(out var weapon);
        weapon.DealDamage(this);
    }

    private void OnEntityUpdate()
    {
        if (isUpdateFreezed || Locked || Destroyed) return;

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

    public override void QuickIneract(SlotCards slot, int count)
    {
        BeAttacked(slot.PeekCard());
    }

    #region AI
    /// <summary>
    /// 得到优先级最高的意图
    /// </summary>
    protected abstract EntityIntention GetHighestPriorityIntention();

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
        if (currentIntention == null)
        {
            // 重置ai冷却
            aiRefreshCooldown = entity.aiRefreshInterval;
        }
        // 有可执行意图
        else
        {
            // 开始准备意图
            currentIntention.Prepare();
            currentIntention.SetBelongedEntity(this);
        }
    }

    /// <summary>
    /// 更新当前意图
    /// </summary>
    private void UpdateCurrentIntention()
    {
        // 更新意图执行倒计时
        currentIntention.UpdateExecutionCountdown();

        if (!currentIntention.IsReady) return;

        // 倒计时完成，尝试执行意图
        if (currentIntention.CanExecute())
        {
            currentIntention.Execute();
        }

        // 无论执行是否成功，都刷新意图
        TryGetNewIntention();
    }
    #endregion

    #region 仇恨
    /// <summary>
    /// 添加仇恨
    /// </summary>=
    public void AddAggro(IEntity entity, int priority, int remainingMinutes)
    {
        if (entity == null) return;
        aggroCollection.AddOrUpdate(entity, priority, remainingMinutes, false);
    }

    /// <summary>
    /// 添加永久仇恨
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="priority"></param>
    public void AddPermanentAggro(IEntity entity, int priority)
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

    /// <summary>
    /// 移除仇恨
    /// </summary>
    protected void RemoveAggro(IEntity entity) => aggroCollection.RemoveByUuid(entity.Uuid);

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

    #region 辅助方法
    public float DistanceTo(IEntity other) => coordinate.DistanceTo(other);
    public bool IsInSameLocation(IEntity other) => coordinate.IsInSameLocation(other);
    public void Move(float dist) => coordinate.Move(dist);
    /// <summary>
    /// 估计移动结束位置
    /// </summary>
    public float EstimateMoveEndPosition(IEntity target, float moveDist, bool moveClose, bool stopAfterReach = true)
    {
        var distToTarget = DistanceTo(target); // 目标距离
        var moveDir = Coordinate.DirectionTo(target.Coordinate); // 目标方向

        if (!moveClose)
            moveDir = -moveDir;

        if (stopAfterReach)
            moveDist = Mathf.Min(distToTarget, moveDist);

        float dest = Coordinate.Position + moveDist * moveDir;

        dest = Mathf.Clamp(dest, Coordinate.Location.PlaceData.minCoord, Coordinate.Location.PlaceData.maxCoord);

        return dest;
    }
    #endregion

    #region 行为
    /// <summary>
    /// 普通攻击
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="dmg">伤害值</param>
    public void SingleAttack(IEntity target, float dmg)
    {
        target.TakeDamage(dmg, this);
    }

    public void ChaseAcrossLocation(IEntity target, float successProb = 0.1f)
    {
        if (Random.value > successProb) return;

        SlotCards.RemoveCard(this);
        GameManager.Instance.AddCardsToTargetEnv(target.Coordinate.Location, this);
    }

    public void MoveTowards(IEntity other, float dist, bool stopAfterReach = true) => coordinate.MoveTowards(other, dist, stopAfterReach);

    public void MoveAwayFrom(IEntity other, float dist) => coordinate.MoveAwayFrom(other, dist);

    public void EscapeFrom(IEntity entity, float dist)
    {
        // 向远离entity的方向移动
        MoveAwayFrom(entity, dist);
        // 如果移动到了边界，且不在室内
        if (Coordinate.IsAtBoundary && !(Bag as EnvironmentBag).PlaceData.isIndoor)
        {
            // 消失
            DestroyThis();
            ShowTip($"{CardName}逃走了");
        }
    }
    #endregion
}
