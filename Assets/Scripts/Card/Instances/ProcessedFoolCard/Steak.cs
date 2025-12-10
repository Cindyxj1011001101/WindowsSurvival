/// <summary>
/// 肉排
/// </summary>
[CardId("肉排")]
public class Steak : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 72 },
                { PlayerStateEnum.Health, 8 }
            },
            sound: "吃_01");
    }
}