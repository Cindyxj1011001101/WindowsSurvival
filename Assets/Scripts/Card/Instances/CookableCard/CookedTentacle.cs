/// <summary>
/// 熟触手
/// </summary>
[CardId("熟触手")]
public class CookedTentacle : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 24 },
                { PlayerStateEnum.Sanity, -1 },
            },
            sound: "吃_01");
    }
}