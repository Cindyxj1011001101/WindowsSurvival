public class Tatsuage : Card
{
    private Tatsuage()
    {
        Events = new()
        {
            new CardEvent("食用", "食用肉排", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 75 },
                { PlayerStateEnum.San, 25 },
                { PlayerStateEnum.Health, 30 }
            })
        };
    }
}