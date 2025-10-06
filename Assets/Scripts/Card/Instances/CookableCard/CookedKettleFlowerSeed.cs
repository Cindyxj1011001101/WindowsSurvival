/// <summary>
/// 熟水壶兰种
/// </summary>
public class CookedKettleFlowerSeed : CookableCard
{
    private CookedKettleFlowerSeed()
    {
        Events = new()
        {
            new Event("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 36 },
                { PlayerStateEnum.Thirst, 14 },
                { PlayerStateEnum.Health, 3 },
            })
        };
    }
}