/// <summary>
/// 坚果酥
/// </summary>
[CardId("坚果酥")]
public class NutCrisp: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 54 },
                { PlayerStateEnum.Health, 5 },
                { PlayerStateEnum.Sanity, 3 },
            },
            sound: "吃_01");
    }
}   