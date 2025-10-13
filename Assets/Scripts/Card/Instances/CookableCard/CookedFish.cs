/// <summary>
/// 大块熟鱼肉
/// </summary>
public class CookedFish : CookableCard
{
    private CookedFish()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Fullness, 38 },
                { PlayerStateEnum.San, 3 },
                { PlayerStateEnum.Health, 10 },
            })
        };
    }
}