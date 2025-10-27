public class FriedInsectStick : Card
{
    private FriedInsectStick()
    {
        Events = new()
        {
            new CardEvent("食用", "食用炸虫串", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 58 },
                { PlayerStateEnum.Hydration, -4 },
                { PlayerStateEnum.Sanity, 12 }
            })
        };
    }
}