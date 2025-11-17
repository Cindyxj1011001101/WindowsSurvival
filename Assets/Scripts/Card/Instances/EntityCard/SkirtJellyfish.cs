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
        base.RegisterCardEvents(); // 攻击事件
    }

    // 潜在仇恨目标
    private static List<Type> potentialAggroEntityTypes = new()
    {
        typeof(Fruitfish),
        typeof(Rat)
    };

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

    protected override EntityIntention GetHighestPriorityIntention()
    {
        var target = GetAggroTarget();
        var isInSameLocation = target != null && IsInSameLocation(target.Target);

        // 与仇恨优目标处于同一地点
        if (isInSameLocation)
        {
            var dist = DistanceTo(target.Target);
            if (dist <= 5)
            {
                // 距离 <= 5 则攻击
                if (dist <= 2)
                    // 距离 <= 2 近战攻击
                    return new MeleeAttackIntention(5, target.TargetUuid, atk, AttackForm.Single, (0, 2));
                else
                    // 距离 > 2 针刺攻击
                    return new AcupunctureAttackIntention(5, target.TargetUuid, atk * 0.4f, AttackForm.Single, (2, 5), 25);
            }
            else
            {
                // 距离 > 5 则移动
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

        // 仇恨目标不在同一地点
        if (target != null && !isInSameLocation)
        {
            return new AcrossLocationMoveIntention(5, target.TargetUuid);
        }

        return null;
    }

    private void Catch(Card tool, CardEvent e)
    {
        tool.Use();
        ApplyEventEffects(e, () =>
        {
            DestroyThis();
            ParseAndDrop(deadDrops);
        });
    }

    private void Event_CatchByNet(CardEvent e)
    {
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

    public override void QuickIneract(SlotCards slot, int count)
    {
        var card = slot.PeekCard();

        if (card.CardId == "捞网")
        {
            // 用捞网捞
            Catch(card, Events[0]);
            return;
        }

        base.QuickIneract(slot, count);
    }
}
