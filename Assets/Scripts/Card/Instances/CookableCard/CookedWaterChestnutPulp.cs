/// <summary>
/// 烤四角菱果肉
/// </summary>
public class CookedWaterChestnutPulp : CookableCard
{
    private CookedWaterChestnutPulp()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, +16 }
            })
        };
    }
}
