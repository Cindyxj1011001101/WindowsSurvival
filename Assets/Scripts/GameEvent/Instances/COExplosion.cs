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
               "这些东西被炸毁了: " + destroyedCardsStr;
    }

    public override bool CanTriggerThisEvent()
    {
        var coLevel = GameManager.Instance.CurEnvironmentBag.StateDict[EnvironmentStateEnum.COLevel].CurValue;
        fireSources = GameManager.Instance.CurEnvironmentBag.FindCards(c =>
        {
            return c.TryGetComponent<FuelStorageComponent>(out var fuelStorage) && fuelStorage.isBurning;
        });
        return coLevel >= CO_LEVEL_THRESHOLD && !fireSources.IsNullOrEmpty(); // 当一氧化碳浓度高且有燃烧源时，事件可以触发
    }

    public override void OnTrigger()
    {
        var env = GameManager.Instance.CurEnvironmentBag;
        // 减少70氧气
        env.ChangeEnvironmentState(EnvironmentStateEnum.Oxygen, -70f);
        // 减少70一氧化碳浓度
        env.ChangeEnvironmentState(EnvironmentStateEnum.COLevel, -70f);
        // 减少30健康
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.Health, -30f);
        // 增加250疼痛
        StateManager.Instance.ChangePlayerState(PlayerStateEnum.PainLevel, 250f);
        // 所有实体减少60生命
        var entites = new List<IEntity>(GameManager.Instance.CurEnvironmentBag.Entities);
        foreach (var entity in entites)
        {
            if (entity is Player) continue; // 玩家已经单独处理过
            entity.TakeDamage(-60f, null);
        }
        // 删除所有火源的内容物
        destroyedCardsStr = "";
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
            destroyedCardsStr += $"{fireSource.CardName}、";
        }
        destroyedCardsStr = destroyedCardsStr.TrimEnd('、');
        fireSources.Clear();
    }
}
