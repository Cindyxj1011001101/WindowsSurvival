public class CoalGrilledMeat : Card
{
    private CoalGrilledMeat()
    {
        Events = new()
        {
            new CardEvent("食用", "有着一层酥脆的皮", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 105 },
                { PlayerStateEnum.Hydration, -10 },
                { PlayerStateEnum.Sanity, 8 },
                { PlayerStateEnum.Health, 5 }
            })
        };
    }
}