/// <summary>
/// 海爬虫
/// </summary>
public class SeaLizard : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "希望不会有毒吧", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 10 },
                { PlayerStateEnum.Sanity, -3 },
                { PlayerStateEnum.Itchiness, 25 }
            });
    }
}
