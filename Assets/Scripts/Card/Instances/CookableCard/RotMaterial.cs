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
                { PlayerStateEnum.Fullness, 6 },
                { PlayerStateEnum.San, -20 },
                { PlayerStateEnum.Health, -10 }
            })
        };
    }
}