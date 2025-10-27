public class Steak : Card
{
    private Steak()
    {
        Events = new()
        {
            new CardEvent("食用", "食用肉排", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 72 },
                { PlayerStateEnum.Health, 8 }
            })
        };
    }
}