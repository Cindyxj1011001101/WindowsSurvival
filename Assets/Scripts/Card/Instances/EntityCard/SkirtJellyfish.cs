using System;
using System.Collections.Generic;

/// <summary>
/// 裙水母
/// </summary>
public class SkirtJellyfish : EntityCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用捞网捉", "", Event_CatchByNet, Judge_CatchByNet, () => 15);
    }

    // 潜在仇恨目标
    private static List<Type> potentialAggroEntityTypes = new()
    {
        typeof(Fruitfish),
        typeof(Rat)
    };

    protected override void RegisterIntentions()
    {
        AddIntention("攻击或移动", 5, Intention_MoveOrAttack);
        AddIntention("食用", 15, Intention_Eat);
    }

    protected override void TryAddAggro(IEntity entity)
    {
        // 裙水母会将距离其[0,7]的部分实体单位加入仇恨列表，优先级5，持续时间15分钟。
        // 包括以下单位：食果鲀、老鼠
        if (potentialAggroEntityTypes.Contains(entity.GetType()) && DistanceTo(entity) <= 7)
        {
            AddAggro(entity, 5, 15);
        }
    }

    public override void TakeDamage(float damage, IEntity damageDealer)
    {
        base.TakeDamage(damage, damageDealer);
        // 裙水母被攻击后会将攻击来源目标加入仇恨列表，优先度9，持续时间60分钟。
        AddAggro(damageDealer, 9, 60);
    }

    protected override string GetHighestPriorityIntention(out object[] cache)
    {
        cache = null;
        var target = GetAggroTarget();
        var isInSameLocation = IsInSameLocation(target.Target);

        // 与仇恨优目标处于同一地点
        if (target != null && isInSameLocation)
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
        // 与目标的距离 <= 5
        if (dist <= 5)
        {
            // 与目标的距离 <= 2
            if (dist <= 2)
                // 普通攻击
                NormalAttack(target);
            else
                // 针刺攻击
                AcupunctureAttack(target);
            return;
        }

        // 与目标的攻击距离 > 5，但是和目标在一个地点
        if (IsInSameLocation(target))
        {
            // 靠近目标
            MoveTowards(target, CurrentIntention.PreparationMinutes * moveDistPerMin);
            return;
        }

        // 与目标的攻击距离 > 5，且和目标不在一个地点
        // TODO: 跨地点

    }

    /// <summary>
    /// 针刺攻击
    /// </summary>
    private void AcupunctureAttack(IEntity target)
    {
        NormalAttack(target, 0.4f);
        // 如果对象是玩家，则+25瘙痒
        if (target is Player)
        {
            StateManager.Instance.ChangePlayerState(PlayerStateEnum.Itchiness, +25);
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
    private void Catch(Card tool, CardEvent e)
    {
        // 销毁卡牌
        DestroyThis();
        tool.Use();

        ApplyEventEffects(e);

        // 掉落产物
        ParseAndDrop(deadDrops);
    }

    private void Event_CatchByNet(out string tip, CardEvent e)
    {
        tip = string.Empty;
        Catch(GameManager.Instance.PlayerBag.FindCardOfName("捞网"), e);
    }

    private bool Judge_CatchByNet(out string hint)
    {
        hint = string.Empty;
        if (GameManager.Instance.PlayerBag.FindCardOfName("捞网") == null)
        {
            hint = "需要捞网";
            return false;
        }
        return true;
    }

    public override bool CanQuickInteract(Card card, out string tip)
    {
        // 用捞网捉
        if (card.CardId == "捞网")
        {
            tip = Events[0].Name;
            return true;
        }

        // 攻击
        return base.CanQuickInteract(card, out tip);
    }

    public override void QuickIneract(SlotCards slot, int count, out string tip)
    {
        tip = string.Empty;
        var card = slot.PeekCard();

        if (card.CardId == "捞网")
        {
            // 用捞网捞
            Catch(card, Events[0]);
            return;
        }

        base.QuickIneract(slot, count, out tip);
    }
}
