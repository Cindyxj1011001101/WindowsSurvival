/// <summary>
/// 熟小块肉
/// </summary>
public class CookedLittleMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 18 },
                { PlayerStateEnum.Health, 1 },
            },
            sound: "吃_01");
    }
}