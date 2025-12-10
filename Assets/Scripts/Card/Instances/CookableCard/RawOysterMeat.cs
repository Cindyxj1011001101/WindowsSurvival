/// <summary>
/// 生贝肉
/// </summary>
[CardId("生贝肉")]
public class RawOysterMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "吃得很快，但不管饱", EasyEvent_Destroy, null,
            () => 5,
            () => new()
            {
                { PlayerStateEnum.Hunger, 6 },
                { PlayerStateEnum.Health, -1.2f }
            },
            sound: "吃_01");
    }
}