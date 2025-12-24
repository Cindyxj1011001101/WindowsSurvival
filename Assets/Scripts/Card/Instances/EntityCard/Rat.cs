/// <summary>
/// 老鼠
/// </summary>
[CardId("老鼠")]
public class Rat : EntityCard
{
    public override void TakeDamage(float damage, IEntity damageDealer)
    {
        // 老鼠被攻击后会将攻击来源目标加入仇恨列表，优先度9，持续时间15分钟。
        AddAggro(damageDealer, 9, 15);
        base.TakeDamage(damage, damageDealer);
    }

    protected override EntityIntention GetHighestPriorityIntention()
    {
        var target = GetAggroTarget();
        var isInSameLocation = target != null && IsInSameLocation(target.Target);

        // 与仇恨优目标处于同一地点
        if (isInSameLocation)
        {
            var dist = DistanceTo(target.Target);
            if (dist <= 1)
            {
                // 距离 <= 1 则攻击
                return new MeleeAttackIntention(5, target.TargetUuid, atk, AttackForm.Single, (0, 1));
            }
            else
            {
                // 距离 > 1 则移动
                return new InLocationMoveIntention(5, target.TargetUuid, 5 * moveDistPerMin, true);
            }
        }

        var env = Bag as EnvironmentBag;
        // 地上有带有"Food"tag的卡牌
        var foodCards = env.FindCardsOfTag(CardTag.Food);
        if (!foodCards.IsNullOrEmpty())
        {
            return new EatIntention(5, foodCards.GetRandomly().Uuid);
        }

        // 仇恨目标不在同一地点
        if (target != null && !isInSameLocation)
        {
            return new AcrossLocationMoveIntention(5, target.TargetUuid);
        }

        return null;
    }
}