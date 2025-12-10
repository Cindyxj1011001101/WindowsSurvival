/// <summary>
/// 恶臭肉
/// </summary>
[CardId("恶臭肉")]
public class FoulSmellingMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -15 }
            },
            sound: "吃_01");
    }
}
