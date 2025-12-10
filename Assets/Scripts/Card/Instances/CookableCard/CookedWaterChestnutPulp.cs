/// <summary>
/// 烤四角菱果肉
/// </summary>
[CardId("烤菱果肉")]
public class CookedWaterChestnutPulp : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, +16 }
            },
            sound: "吃_01");
    }
}
