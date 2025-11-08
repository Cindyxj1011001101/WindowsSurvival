/// <summary>
/// 熟贝肉
/// </summary>
public class CookedOysterMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hunger, 8 },
                { PlayerStateEnum.Health, 1 },
                { PlayerStateEnum.Sanity, 1 },
            },
            sound: "吃_01");
    }
}