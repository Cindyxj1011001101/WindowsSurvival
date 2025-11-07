/// <summary>
/// 熟贝肉
/// </summary>
public class CookedOysterMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hunger, 8 },
                { PlayerStateEnum.Health, 1 },
                { PlayerStateEnum.Sanity, 1 },
            });
    }
}