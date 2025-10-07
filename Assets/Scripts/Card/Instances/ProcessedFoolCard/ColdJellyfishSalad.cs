public class ColdJellyfishSalad : Card
{
    private ColdJellyfishSalad()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 40 },
                { PlayerStateEnum.Thirst, 25 },
            })
        };
    }
}   