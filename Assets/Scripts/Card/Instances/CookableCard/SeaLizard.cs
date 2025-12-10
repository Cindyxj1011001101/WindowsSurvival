/// <summary>
/// 海爬虫
/// </summary>
[CardId("海爬虫")]
public class SeaLizard : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "希望不会有毒吧", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 10 },
                { PlayerStateEnum.Sanity, -3 },
                { PlayerStateEnum.Itchiness, 25 }
            },
            sound: "吃_01");
    }
}
