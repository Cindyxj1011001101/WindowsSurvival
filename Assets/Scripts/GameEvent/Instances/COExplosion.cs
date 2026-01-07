using System.Collections.Generic;

/// <summary>
/// 一氧化碳爆炸
/// </summary>
public class COExplosion : GameEvent
{
    private const float CO_LEVEL_THRESHOLD = 75f; // 一氧化碳浓度阈值

    private List<Card> fireSources = new();

    private string destroyedCardsStr;

    public override string GetDetails()
    {
        return "麦麦在有着高浓度一氧化碳的室内点火，真是勇敢。\n\n" +
               "总之，地点中的火源爆炸了，麦麦以及该地点内的所有生物都被炸得体无完肤。\n\n" +
               "地点里的一氧化碳和氧气减少了。\n\n" +
               "这些东西被炸毁了: " + ColorManager.Warning(destroyedCardsStr);
    }

    protected override bool CanTriggerThisEvent()
    {
        var env = GameManager.Instance.CurEnvironmentBag;

        if (!env.StateDict.TryGetValue(EnvironmentStateEnum.COLevel, out var value)) return false;

        var coLevel = value.CurValue;
        fireSources = env.FindCards(c => c.TryGetComponent<FuelStorageComponent>(out var fuelStorage) && fuelStorage.isBurning);

        return coLevel >= CO_LEVEL_THRESHOLD && !fireSources.IsNullOrEmpty(); // 当一氧化碳浓度高且有燃烧源时，事件可以触发
    }

    protected override void OnTrigger()
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        // 减少70氧气
        env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -70f);
        // 减少70一氧化碳浓度
        env.ChangeEnvironmentState(EnvironmentStateEnum.COLevel, -70f);
        // 增加250疼痛
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 250f);
        // 所有实体减少60生命
        var entites = new List<IEntity>(GameManager.Instance.CurEnvironmentBag.AllEntities);
        foreach (var entity in entites)
        {
            // 玩家单独处理
            if (entity is Player)
            {
                // 玩家减少30生命
                entity.TakeDamage(-30f, null);
                continue;
            }
            entity.TakeDamage(-60f, null);
        }

        var destroyedCards = new Dictionary<string, int>();
        // 删除所有火源的内容物
        foreach (var fireSource in fireSources)
        {
            if (fireSource.TryGetComponent<InnerContentsComponent>(out var inn))
            {
                inn.Clear();
            }
            // 火源被拆毁，掉落掉落物
            if (fireSource is ConstructionCard con)
            {
                con.DemolishThis(null);
            }

            // 记录被破坏的卡牌数量
            if (!destroyedCards.ContainsKey(fireSource.CardName))
                destroyedCards.Add(fireSource.CardName, 1);
            else
                destroyedCards[fireSource.CardName]++;
        }

        destroyedCardsStr = "";
        foreach (var kvp in destroyedCards)
        {
            destroyedCardsStr += $"{kvp.Key} x {kvp.Value}、";
    }
        destroyedCardsStr = destroyedCardsStr.TrimEnd('、');
        fireSources.Clear();
    }
}
