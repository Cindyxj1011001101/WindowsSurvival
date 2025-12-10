/// <summary>
/// 熟海爬虫
/// </summary>
[CardId("熟海爬虫")]
public class CookedSeaLizard : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 9 },
                { PlayerStateEnum.Itchiness, 8 }
            },
            sound: "吃_01");
    }
}
