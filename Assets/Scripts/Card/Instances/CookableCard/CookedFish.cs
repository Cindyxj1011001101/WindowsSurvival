/// <summary>
/// 大块熟鱼肉
/// </summary>
public class CookedFish : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 38 },
                { PlayerStateEnum.Sanity, 3 },
                { PlayerStateEnum.Health, 10 },
            },
            sound: "吃_01");
    }
}