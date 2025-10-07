public class FishSoup : Card
{
    private FishSoup()
    {
        Events = new()
        {
            new CardEvent("食用", "食用鱼汤", (out string s) => EasyEvent(out s, "喝_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 18 },
                { PlayerStateEnum.Thirst, 33 },
                { PlayerStateEnum.San, 12 },
                { PlayerStateEnum.Health, 20 },
                { PlayerStateEnum.PainLevel, -25 }
            })
        };
    }
}