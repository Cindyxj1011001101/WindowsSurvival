/// <summary>
/// 厨房恶物
/// </summary>
[CardId("厨房恶物")]
public class KitchenFoes : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 13 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -4 }
            },
            sound: "吃_01");
    }
}