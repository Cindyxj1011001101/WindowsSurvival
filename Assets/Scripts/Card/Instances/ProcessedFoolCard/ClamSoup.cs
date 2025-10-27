public class ClamSoup : Card
{
    private ClamSoup()
    {
        Events = new()
        {
            new CardEvent("食用", "食用蛤蜊浓汤", (out string s) => EasyEvent(out s, "喝_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 36 },
                { PlayerStateEnum.Hydration, 66 },
                { PlayerStateEnum.Sanity, 15 }
            })
        };
    }
}