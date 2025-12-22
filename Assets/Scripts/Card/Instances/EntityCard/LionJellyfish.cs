using System;
using System.Collections.Generic;

/// <summary>
/// 狮子水母
/// </summary>
[CardId("狮子水母")]
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

    protected override void TryAddAggro(IEntity entity)
    {
        // 狮子水母会将距离其[0,10]的部分实体单位加入仇恨列表，优先级8，持续时间15分钟。
        // 包括以下单位：麦麦、食果豚、裙水母、狮子水母、老鼠
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

    protected override EntityIntention GetHighestPriorityIntention()
    {
        var target = GetAggroTarget();
        var isInSameLocation = target != null && IsInSameLocation(target.Target);

        // 与仇恨优先级 > 7 的目标处于同一地点
        if (isInSameLocation && target.Priority > 7)
        {
            var dist = DistanceTo(target.Target);
            if (dist <= 10)
            {
                if (dist >= 4)
                    // 距离 >= 4 则攻击
                    return new AcupunctureAttackIntention(5, target.TargetUuid, atk * 0.4f, AttackForm.Single, (4, 10), 50);
                else
                    // 距离 < 4 则远离
                    return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, false);
            }
            else
            {
                // 距离 > 10 则靠近
                return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, true);
            }
        }

        var env = Bag as EnvironmentBag;
        // 地上有带有"肉食"tag的卡牌
        var meatCards = env.FindCardsOfTag(CardTag.Meat);
        if (!meatCards.IsNullOrEmpty())
        {
            return new EatIntention(15, meatCards.GetRandomly().Uuid);
        }

        // 与仇恨优先级 <= 7 的目标处于同一地点
        if (isInSameLocation && target.Priority <= 7)
        {
            var dist = DistanceTo(target.Target);
            if (dist <= 10)
            {
                if (dist >= 4)
                    // 距离 >= 4 则攻击
                    return new AcupunctureAttackIntention(5, target.TargetUuid, atk * 0.4f, AttackForm.Single, (4, 10), 50);
                else
                    // 距离 < 4 则远离
                    return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, false);
            }
            else
            {
                // 距离 > 10 则靠近
                return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, true);
            }
        }

        // 仇恨目标不在同一地点
        if (target != null && !isInSameLocation)
        {
            return new AcrossLocationMoveIntention(5, target.TargetUuid);
        }

        return null;
    }
}
