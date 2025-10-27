/// <summary>
/// 大块生鱼肉
/// </summary>
public class RawFish : CookableCard
{
    private RawFish()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 33 },
                { PlayerStateEnum.Sanity, -1 },
                { PlayerStateEnum.Health, -5 },
            })
        };
    }
}