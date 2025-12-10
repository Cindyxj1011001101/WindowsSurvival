/// <summary>
/// 大块生鱼肉
/// </summary>
[CardId("大块生鱼肉")]
public class RawFish : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 33 },
                { PlayerStateEnum.Sanity, -1 },
                { PlayerStateEnum.Health, -5 },
            },
            sound: "吃_01");
    }
}