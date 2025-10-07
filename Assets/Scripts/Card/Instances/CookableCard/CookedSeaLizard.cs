public class CookedSeaLizard : CookableCard
{
    private CookedSeaLizard()
    {
        Events = new()
        {
            new CardEvent("食用", "食用熟海爬虫", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 9 },
                { PlayerStateEnum.Itchiness, 8 }
            })
        };
    }
}
