/// <summary>
/// 水壶兰种子
/// </summary>
public class KettleFlowerSeed : CookableCard
{
    private KettleFlowerSeed()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Hydration, 14 },
            })
        };
    }
}