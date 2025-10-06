/// <summary>
/// 磁性触手
/// </summary>
public class MagneticTentacle : CookableCard
{
    private MagneticTentacle()
    {
        Events = new()
        {
            new Event("食用", "闻起来有铁锈味", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Fullness, 14 },
                { PlayerStateEnum.San, -6 },
                { PlayerStateEnum.Health, -5 }
            })
        };
    }
}