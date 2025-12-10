/// <summary>
/// 熟水壶兰种
/// </summary>
[CardId("熟水壶兰种")]
public class CookedKettleFlowerSeed : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 36 },
                { PlayerStateEnum.Hydration, 14 },
                { PlayerStateEnum.Health, 3 },
            },
            sound: "吃_01");
    }
}