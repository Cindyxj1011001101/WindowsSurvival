/// <summary>
/// 熟海爬虫
/// </summary>
public class CookedSeaLizard : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 9 },
                { PlayerStateEnum.Itchiness, 8 }
            });
    }
}
