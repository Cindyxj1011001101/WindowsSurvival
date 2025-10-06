/// <summary>
/// 小块肉
/// </summary>
public class LittleRawMeat : CookableCard
{
    private LittleRawMeat()
    {
        Events = new()
        {
            new Event("食用", "食用小块生肉", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 12 },
                { PlayerStateEnum.San, -2 },
                { PlayerStateEnum.Health, -3 }
            })
        };
    }
}