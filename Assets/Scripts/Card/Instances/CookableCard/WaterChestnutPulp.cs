/// <summary>
/// 四角菱果肉
/// </summary>
public class WaterChestnutPulp : CookableCard
{
    private WaterChestnutPulp()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, +10 }
            })
        };
    }
}
