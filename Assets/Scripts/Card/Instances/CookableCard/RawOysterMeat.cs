/// <summary>
/// 生贝肉
/// </summary>
public class RawOysterMeat : CookableCard
{
    private RawOysterMeat()
    {
        Events = new()
        {
            new CardEvent("食用", "吃得很快，但不管饱", (out string s) => EasyEvent(out s, "吃_01"), null, () => 5,
            () => new()
            {
                { PlayerStateEnum.Hunger, 6 },
                { PlayerStateEnum.Health, -1.2f }
            })
        };
    }
}