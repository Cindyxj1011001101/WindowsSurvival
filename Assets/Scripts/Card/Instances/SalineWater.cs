/// <summary>
/// 盐水
/// </summary>
public class SalineWater : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "会导致脱水", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hydration, -25 },
            },
            sound: "喝_01");
    }
}