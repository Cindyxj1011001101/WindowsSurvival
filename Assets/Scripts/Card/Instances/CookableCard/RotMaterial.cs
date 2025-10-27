/// <summary>
/// 腐烂物
/// </summary>
public class RotMaterial : CookableCard
{
    private RotMaterial()
    {
        Events = new()
        {
            new CardEvent("食用", "吃这个？你疯了", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 6 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -10 }
            })
        };
    }
}