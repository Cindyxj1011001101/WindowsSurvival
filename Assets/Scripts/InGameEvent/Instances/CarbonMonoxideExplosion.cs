using System.Collections.Generic;

/// <summary>
/// 一氧化碳爆炸
/// </summary>
public class CarbonMonoxideExplosion : GameEvent
{
    private const float CO_LEVEL_THRESHOLD = 75f; // 一氧化碳浓度阈值
    private List<Card> fireSources = new();

    public override bool CanTriggerThisEvent()
    {
        var coLevel = GameManager.Instance.CurEnvironmentBag.StateDict[EnvironmentStateEnum.CarbonMonoxideLevel].CurValue;
        fireSources = GameManager.Instance.CurEnvironmentBag.FindCards(c =>
        {
            return c.TryGetComponent<FuelStorageComponent>(out var fuelStorage) && fuelStorage.isBurning;
        });
        return coLevel >= CO_LEVEL_THRESHOLD && !fireSources.IsNullOrEmpty(); // 当一氧化碳浓度高且有燃烧源时，事件可以触发
    }

    protected override void OnTrigger()
    {
        // 减少70氧气

        // 减少70一氧化碳浓度

        // 减少30健康

        // 增加250疼痛

        // 所有实体减少60生命

        // 删除所有火源的内容物

        // 火源被拆毁，掉落掉落物
    }
}
