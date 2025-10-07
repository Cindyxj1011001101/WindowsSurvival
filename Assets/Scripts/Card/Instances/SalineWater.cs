/// <summary>
/// 盐水
/// </summary>
public class SalineWater : Card
{
    private SalineWater()
    {
        Events = new()
        {
            new CardEvent("饮用", "会导致脱水", (out string s) => EasyEvent(out s, "喝_01"), null, () => 3,
            () => new()
            {
                { PlayerStateEnum.Thirst, -25 },
            })
        };
    }
}