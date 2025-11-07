using System.Collections.Generic;

/// <summary>
/// 食果鲀
/// </summary>
public class Fruitfish : EntityCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用捞网捉", "", Event_CatchByNet, Judge_CatchByNet, () => 15);
    }

    protected override string GetHighestPriorityIntention()
    {
        var env = Bag as EnvironmentBag;
        // 地上有素食卡牌
        if (!env.FindCardsOfTag(CardTag.Vege).IsNullOrEmpty())
        {
            return "食用";
        }

        // 离predator的最近距离小于7
        var closestPredator = GetClosestPredator();
        if (closestPredator != null && closestPredator.DistanceTo(this) < 7)
        {
            return "逃跑";
        }

        // 地点有成熟的果实作物
        if (!GetRipeFruitCrops().IsNullOrEmpty())
        {
            return "采摘并食用";
        }

        return null;
    }

    protected override void RegisterIntentions()
    {
        AddIntention("食用", 15, Intention_Eat);
        AddIntention("逃跑", 5, Intention_Escape);
        AddIntention("采摘并食用", 15, Intention_PickAndEat);
    }

    private void Intention_Eat()
    {
        // 食果鲀会随机选择该地点的地上的一张带有素食tag的卡牌吃掉
        var env = Bag as EnvironmentBag;
        // 获取当前地点的所有素食卡牌
        var foodCards = env.FindCardsOfTag(CardTag.Vege);

        // 没有素食卡牌，意图执行失败
        if (foodCards.IsNullOrEmpty()) return;

        // 随机选择一张素食卡牌
        var selected = foodCards[UnityEngine.Random.Range(0, foodCards.Count)];
        // 吃掉
        // TODO: 吃掉动效
        selected.DestroyThis();
    }

    private void Intention_Escape()
    {
        // 往最近的（带猎人tag的卡牌或玩家）的反方向移动(准备时间*移动速度)的距离。
        // 如果移动后的坐标超过了该地点的坐标范围，则判断该地点是否是室内地点。如是，则不会超出坐标范围而是停在范围边界。如不是，则弹出“食果鲀逃走了”的提示，并销毁食果鲀。
        var env = Bag as EnvironmentBag;
        
        // 找到最近的predator
        var closetPredator = GetClosestPredator();

        // 没有predator，意图执行失败
        if (closetPredator == null) return;

        // 向远离最近的predator的方向逃离
        this.MoveAwayFrom(closetPredator, moveDistPerMin * intentions["逃跑"].PreparationMinutes);

        // 如果逃离到边界并且当前地点不是室内地点
        if (Coordinate.IsAtBoundary && !env.PlaceData.isIndoor)
        {
            // 食果鲀消失
            DestroyThis();
            ShowTip($"{CardName}逃跑了");
            return;
        }

        RefreshSlot();
    }

    private IEntity GetClosestPredator()
    {
        var env = Bag as EnvironmentBag;
        // 获取所有的predator
        var predatorCards = env.FindCardsOfTag(CardTag.Predator);
        // 获取最近的predator
        var closetDist = float.MaxValue;
        IEntity closetPredator = null;
        foreach (var c in predatorCards)
        {
            if (c is not IEntity entity) continue;

            var dist = this.DistanceTo(entity);
            if (dist >= closetDist) continue;

            closetDist = dist;
            closetPredator = entity;
        }
        // 如果玩家和食果鲀在同一地点
        if (Player.Instance.Coordinate.Location == env)
        {
            // 将玩家也加入判断
            var dist = this.DistanceTo(Player.Instance);
            if (dist < closetDist)
            {
                closetPredator = Player.Instance;
            }
        }

        return closetPredator;
    }

    private void Intention_PickAndEat()
    {
        // 食果鲀会随机选择该地点的一张“果实作物”tag的“是否成熟”为true的作物卡牌，使其是否成熟变为否，作物生长度变为0。
        // 获取当前地点的所有成熟的果实作物
        var ripeCards = GetRipeFruitCrops();

        // 没有成熟的果实作物，意图执行失败
        if (ripeCards.IsNullOrEmpty()) return;

        // 随机选择一个
        var selected = ripeCards[UnityEngine.Random.Range(0, ripeCards.Count)] as PlantCard;
        // 使其作物生长度变为0
        // TODO: 动效
        selected.SetPlantGrowth(0);
    }

    private List<Card> GetRipeFruitCrops()
    {
        var env = Bag as EnvironmentBag;
        return env.FindCards(c => c.Tags.Contains(CardTag.FruitCrop) && c is PlantCard p && p.IsRipe);
    }

    private void Event_CatchByNet(out string tip)
    {
        Catch(GameManager.Instance.PlayerBag.FindCardOfName("捞网"), out tip);
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

    private void Catch(Card tool, out string tip)
    {
        tip = string.Empty;

        // 销毁卡牌
        DestroyThis();
        // 1. 消耗耐久
        tool.Use();

        // 2. 时间变化
        ApplyEventEffects(0);

        // 掉落产物
        ParseAndDrop(deadDrops);
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
        var card = slot.PeekCard();

        if (card.CardId == "捞网")
        {
            // 用捞网捞
            Catch(card, out tip);
            return;
        }

        base.QuickIneract(slot, count, out tip);
    }
}
