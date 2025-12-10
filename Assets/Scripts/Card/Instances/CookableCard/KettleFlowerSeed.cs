/// <summary>
/// 水壶兰种子
/// </summary>
[CardId("水壶兰种子")]
public class KettleFlowerSeed : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Hydration, 14 },
            },
            sound: "吃_01");
    }
}