/// <summary>
/// 黑金炭烤肉
/// </summary>
[CardId("黑金炭烤肉")]
public class CoalGrilledMeat : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "有着一层酥脆的皮", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 105 },
                { PlayerStateEnum.Hydration, -10 },
                { PlayerStateEnum.Sanity, 8 },
                { PlayerStateEnum.Health, 5 }
            },
            sound: "吃_01");
    }
}