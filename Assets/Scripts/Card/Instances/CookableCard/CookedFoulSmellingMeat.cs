/// <summary>
/// 熟恶臭肉
/// </summary>
public class CookedFoulSmellingMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 20 },
                { PlayerStateEnum.Sanity, -15 },
                { PlayerStateEnum.Health, -10 }
            },
            sound: "吃_01");
    }
}
