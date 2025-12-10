/// <summary>
/// 蛤蜊浓汤
/// </summary>
[CardId("蛤蜊浓汤")]
public class ClamSoup : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 36 },
                { PlayerStateEnum.Hydration, 66 },
                { PlayerStateEnum.Sanity, 15 }
            },
            sound: "喝_01");
    }
}