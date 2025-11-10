using System;
using System.Collections.Generic;

/// <summary>
/// 狮子水母
/// </summary>
public class LionJellyfish : EntityCard
{
    // 潜在仇恨目标
    private static List<Type> potentialAggroEntityTypes = new()
    {
        typeof(Player),
        typeof(Fruitfish),
        typeof(Rat),
        typeof(SuckerWorm),
    };

    protected override void RegisterIntentions()
    {
        AddIntention("攻击或移动", 5, Intention_MoveOrAttack);
        AddIntention("食用", 15, Intention_Eat);
    }

    protected override void TryAddAggro(IEntity entity)
    {
        // 狮子水母会将距离其[0,10]的部分实体单位加入仇恨列表，优先级8，持续时间15分钟。
        // 包括以下单位：麦麦、食果鲀、裙水母、狮子水母、老鼠
        if (potentialAggroEntityTypes.Contains(entity.GetType()) && DistanceTo(entity) <= 10)
        {
            AddAggro(entity, 8, 15);
        }
    }

    public override void TakeDamage(float damage, IEntity damageDealer)
    {
        base.TakeDamage(damage, damageDealer);
        // 狮子水母被攻击后会将攻击来源目标加入仇恨列表，优先度9，持续时间60分钟。
        AddAggro(damageDealer, 9, 60);
    }

    protected override string GetHighestPriorityIntention(out object[] cache)
    {
        cache = null;
        var target = GetAggroTarget();
        var isInSameLocation = target != null && IsInSameLocation(target.Target);

        // 与仇恨优先级 > 7 的目标处于同一地点
        if (target != null && isInSameLocation && target.Priority > 7)
        {
            cache = new object[] { target.TargetUuid };
            return "攻击或移动";
        }

        var env = Bag as EnvironmentBag;
        // 地上有带有"肉食"tag的卡牌
        var meatCards = env.FindCardsOfTag(CardTag.Meat);
        if (!meatCards.IsNullOrEmpty())
        {
            cache = new object[] { meatCards.GetRandomly().Uuid };
            return "食用";
        }

        // 与仇恨优先级 <= 7 的目标处于同一地点
        if (target != null && isInSameLocation && target.Priority <= 7)
        {
            cache = new object[] { target.TargetUuid };
            return "攻击或移动";
        }

        // 仇恨目标不在同一地点
        if (target != null && !isInSameLocation)
        {
            cache = new object[] { target.TargetUuid };
            return "攻击或移动";
        }

        return null;
    }

    private void Intention_MoveOrAttack(object[] cache)
    {
        var targetUuid = cache[0] as string;
        var target = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 目标已不存在，意图执行失败
        if (target == null) return;

        var dist = DistanceTo(target);
        // 与目标的距离 <= 10
        if (dist <= 10)
        {
            // 与目标的距离 < 4
            if (dist < 4)
                // 远离目标
                MoveAwayFrom(target, CurrentIntention.PreparationMinutes * moveDistPerMin);
            else
                // 针刺攻击
                AcupunctureAttack(target);
            return;
        }

        // 与目标的攻击距离 > 10，但是和目标在一个地点
        if (IsInSameLocation(target))
        {
            // 靠近目标
            MoveTowards(target, CurrentIntention.PreparationMinutes * moveDistPerMin);
            return;
        }

        // 与目标的攻击距离 > 10，且和目标不在一个地点
        // TODO: 跨地点

    }
    
    /// <summary>
    /// 针刺攻击
    /// </summary>
    private void AcupunctureAttack(IEntity target)
    {
        NormalAttack(target, 0.4f);
        // 如果对象是玩家，则+50瘙痒
        if (target is Player)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, +50);
        }
    }

    private void Intention_Eat(object[] cache)
    {
        var cardUuid = cache[0] as string;
        var toEat = GlobalDataManager.Instance.GetCardByUuid(cardUuid);

        // 食物已不存在，意图执行失败
        if (toEat == null) return;

        // 食物不在当前地点，意图执行失败
        if (!IsInSameBag(toEat)) return;

        // 吃掉
        // TODO: 吃掉动效
        toEat.DestroyThis();
    }
}
