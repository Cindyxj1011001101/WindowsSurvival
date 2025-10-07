/// <summary>
/// 海爬虫
/// </summary>
public class SeaLizard : CookableCard
{
    private SeaLizard()
    {
        Events = new()
        {
            new CardEvent("食用", "希望不会有毒吧", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 10 },
                { PlayerStateEnum.San, -3 },
                { PlayerStateEnum.Itchiness, 25 }
            })
        };
    }
}
