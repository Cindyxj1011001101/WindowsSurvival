/// <summary>
/// 小块肉
/// </summary>
public class LittleRawMeat : CookableCard
{
    private LittleRawMeat()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 12 },
                { PlayerStateEnum.Sanity, -2 },
                { PlayerStateEnum.Health, -3 }
            })
        };
    }
}