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

    protected override string GetHighestPriorityIntention(out object[] cache)
    {
        cache = null;
        var env = Bag as EnvironmentBag;
        
        // 地上有素食卡牌
        var vegeCards = env.FindCardsOfTag(CardTag.Vege);
        if (!vegeCards.IsNullOrEmpty())
        {
            cache = new object[] { vegeCards.GetRandomly().Uuid };
            return "食用";
        }

        // 离predator的最近距离小于7
        var closestPredator = GetClosestPredator();
        if (closestPredator != null && DistanceTo(closestPredator) < 7)
        {
            cache = new object[] { closestPredator.Uuid };
            return "逃跑";
        }

        // 地点有成熟的果实作物
        var fruitCards = GetRipeFruitCrops();
        if (!fruitCards.IsNullOrEmpty())
        {
            cache = new object[] { fruitCards.GetRandomly().Uuid };
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

    private void Intention_Eat(object[] cache)
    {
        // 食果鲀会随机选择该地点的地上的一张带有素食tag的卡牌吃掉
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

    private void Intention_Escape(object[] cache)
    {
        // 往最近的（带猎人tag的卡牌或玩家）的反方向移动(准备时间*移动速度)的距离。
        // 如果移动后的坐标超过了该地点的坐标范围，则判断该地点是否是室内地点。如是，则不会超出坐标范围而是停在范围边界。如不是，则弹出“食果鲀逃走了”的提示，并销毁食果鲀。

        var targetUuid = cache[0] as string;
        var closetPredator = GlobalDataManager.Instance.GetEntityByUuid(targetUuid);

        // 目标已不存在，意图执行失败
        if (closetPredator == null) return;

        // 向远离最近的predator的方向逃离
        MoveAwayFrom(closetPredator, CurrentIntention.PreparationMinutes * moveDistPerMin);

        var env = Bag as EnvironmentBag;
        // 如果逃离到边界并且当前地点不是室内地点
        if (Coordinate.IsAtBoundary && !env.PlaceData.isIndoor)
        {
            // 食果鲀消失
            DestroyThis();
            ShowTip($"{CardName}逃跑了");
        }
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

            var dist = DistanceTo(entity);
            if (dist >= closetDist) continue;

            closetDist = dist;
            closetPredator = entity;
        }
        // 如果玩家和食果鲀在同一地点
        if (IsInSameLocation(Player.Instance))
        {
            // 将玩家也加入判断
            var dist = DistanceTo(Player.Instance);
            if (dist < closetDist)
            {
                closetPredator = Player.Instance;
            }
        }

        return closetPredator;
    }

    private void Intention_PickAndEat(object[] cache)
    {
        // 食果鲀会随机选择该地点的一张“果实作物”tag的“是否成熟”为true的作物卡牌，使其是否成熟变为否，作物生长度变为0。
        var cardUuid = cache[0] as string;
        var toEat = GlobalDataManager.Instance.GetCardByUuid(cardUuid);

        // 食物已不存在，意图执行失败
        if (toEat == null) return;

        // 食物不在当前地点，意图执行失败
        if (!IsInSameBag(toEat)) return;

        // 吃掉
        // TODO: 吃掉动效
        (toEat as PlantCard).SetPlantGrowth(0);
    }

    private List<Card> GetRipeFruitCrops()
    {
        var env = Bag as EnvironmentBag;
        return env.FindCards(c => c.Tags.Contains(CardTag.FruitCrop) && c is PlantCard p && p.IsRipe);
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
