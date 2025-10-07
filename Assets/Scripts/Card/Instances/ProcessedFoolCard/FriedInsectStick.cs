public class FriedInsectStick : Card
{
    private FriedInsectStick()
    {
        Events = new()
        {
            new CardEvent("食用", "食用炸虫串", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 58 },
                { PlayerStateEnum.Thirst, -4 },
                { PlayerStateEnum.San, 12 }
            })
        };
    }
}