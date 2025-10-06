/// <summary>
/// 烧焦的食物
/// </summary>
public class BurntFood : CookableCard
{
    private BurntFood()
    {
        Events = new()
        {
            new Event("食用", "食用烧焦的食物", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 10 },
                { PlayerStateEnum.Thirst, -20 },
                { PlayerStateEnum.Health, -5 },
                { PlayerStateEnum.BodyTemperature, 20 }
            })
        };
    }
}