public class CoalGrilledMeat : Card
{
    private CoalGrilledMeat()
    {
        Events = new()
        {
            new Event("食用", "有着一层酥脆的皮", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 105 },
                { PlayerStateEnum.Thirst, -10 },
                { PlayerStateEnum.San, 8 },
                { PlayerStateEnum.Health, 5 }
            })
        };
    }
}