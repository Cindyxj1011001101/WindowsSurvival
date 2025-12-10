/// <summary>
/// 育卵液
/// </summary>
[CardId("育卵液")]
public class EggRearingFluid : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hydration, 40 },
                { PlayerStateEnum.Hunger, 10 },
            },
            sound: "喝_01");
    }
}