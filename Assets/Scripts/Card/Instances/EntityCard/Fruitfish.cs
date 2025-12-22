using System.Collections.Generic;

/// <summary>
/// 食果鲀
/// </summary>
[CardId("食果鲀")]
public class Fruitfish : EntityCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("用捞网捉", "", Event_CatchByNet, Judge_CatchByNet, () => 15);
        base.RegisterCardEvents(); // 攻击事件
    }

    protected override EntityIntention GetHighestPriorityIntention()
    {
        var env = Bag as EnvironmentBag;
        
        // 地上有素食卡牌
        var vegeCards = env.FindCardsOfTag(CardTag.Vege);
        if (!vegeCards.IsNullOrEmpty())
        {
            return new EatIntention(15, vegeCards.GetRandomly().Uuid);
        }

        // 离predator的最近距离小于7
        var closestPredator = GetClosestPredator();
        if (closestPredator != null && DistanceTo(closestPredator) < 7)
        {
            return new EscapeIntention(5, closestPredator.Uuid, 5 * moveDistPerMin);
        }

        // 地点有成熟的果实作物
        var fruitCards = GetRipeFruitCrops();
        if (!fruitCards.IsNullOrEmpty())
        {
            return new EatIntention(15, fruitCards.GetRandomly().Uuid);
        }

        return null;
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

    private List<Card> GetRipeFruitCrops()
    {
        var env = Bag as EnvironmentBag;
        return env.FindCards(c => c.Tags.Contains(CardTag.FruitCrop) && c is PlantCard p && p.IsRipe);
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
