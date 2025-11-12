/// <summary>
/// 老鼠
/// </summary>
public class Rat : EntityCard
{
    protected override void RegisterIntentions()
    {
        AddIntention("攻击或移动", 5, Intention_MoveOrAttack);
        AddIntention("食用", 5, Intention_Eat);
    }

    public override void TakeDamage(float damage, IEntity damageDealer)
    {
        base.TakeDamage(damage, damageDealer);
        // 老鼠被攻击后会将攻击来源目标加入仇恨列表，优先度9，持续时间15分钟。
        AddAggro(damageDealer, 9, 15);
    }

    protected override string GetHighestPriorityIntention(out object[] cache)
    {
        cache = null;
        var target = GetAggroTarget();
        var isInSameLocation = target != null && IsInSameLocation(target.Target);

        // 与仇恨优目标处于同一地点
        if (target != null && isInSameLocation)
        {
            cache = new object[] { target.TargetUuid };
            return "攻击或移动";
        }

        var env = Bag as EnvironmentBag;
        // 地上有带有"food"tag的卡牌
        var meatCards = env.FindCardsOfTag(CardTag.Food);
        if (!meatCards.IsNullOrEmpty())
        {
            cache = new object[] { meatCards.GetRandomly().Uuid };
            return "食用";
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

        // 与目标的距离 <= 1
        if (DistanceTo(target) <= 1)
        {
            // 普通攻击
            NormalAttack(target);
            return;
        }

        // 与目标的攻击距离 > 1，但是和目标在一个地点
        if (IsInSameLocation(target))
        {
            // 靠近目标
            MoveTowards(target, CurrentIntention.PreparationMinutes * moveDistPerMin);
            return;
        }

        // 与目标的攻击距离 > 1，且和目标不在一个地点
        ChaseTargetAcrossLocation(target);
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