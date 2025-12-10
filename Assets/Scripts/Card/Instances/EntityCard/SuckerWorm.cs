using System;
using System.Collections.Generic;

/// <summary>
/// 吸盘蠕虫
/// </summary>
[CardId("吸盘蠕虫")]
public class SuckerWorm : EntityCard
{
    // 潜在仇恨目标
    private static List<Type> potentialAggroEntityTypes = new()
    {
        typeof(Player),
        typeof(Fruitfish),
        typeof(SkirtJellyfish),
        typeof(LionJellyfish),
        typeof(Rat)
    };

    protected override void OnLateConstructor()
    {
        base.OnLateConstructor();
        // 自带对玩家的永久仇恨，优先级为7
        AddPermanentAggro(Player.Instance, 7);
    }

    protected override void TryAddAggro(IEntity entity)
    {
        // 吸盘蠕虫会将距离其[0,5]的部分实体单位加入仇恨列表，优先级8，持续时间15分钟。
        // 包括以下单位：麦麦、食果鲀、裙水母、狮子水母、老鼠
        if (potentialAggroEntityTypes.Contains(entity.GetType()) && DistanceTo(entity) <= 5)
        {
            AddAggro(entity, 8, 15);
        }
    }

    public override void TakeDamage(float damage, IEntity damageDealer)
    {
        base.TakeDamage(damage, damageDealer);
        // 吸盘蠕虫被攻击后会将攻击来源目标加入仇恨列表，优先度9，持续时间60分钟。
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
            if (dist <= 3)
                // 距离 <= 3 则攻击
                return new MeleeAttackIntention(5, target.TargetUuid, atk, AttackForm.Single, (0, 3));
            else
                // 距离 > 3 则移动
                return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, true);
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
            if (dist <= 3)
                // 距离 <= 3 则攻击
                return new MeleeAttackIntention(5, target.TargetUuid, atk, AttackForm.Single, (0, 3));
            else
                // 距离 > 3 则移动
                return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, true);
        }

        // 仇恨目标不在同一地点
        if (target != null && !isInSameLocation)
        {
            return new AcrossLocationMoveIntention(5, target.TargetUuid);
        }

        return null;
    }
}
