/// <summary>
/// 大块熟鱼肉
/// </summary>
public class CookedFish : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 38 },
                { PlayerStateEnum.Sanity, 3 },
                { PlayerStateEnum.Health, 10 },
            });
    }
}