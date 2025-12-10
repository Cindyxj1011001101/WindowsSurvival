/// <summary>
/// 四角菱果肉
/// </summary>
[CardId("菱果肉")]
public class WaterChestnutPulp : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, +10 }
            },
            sound: "吃_01");
    }
}
