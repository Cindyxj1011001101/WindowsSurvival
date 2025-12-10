/// <summary>
/// 鱼皮
/// </summary>
[CardId("鱼皮")]
public class FishSkin : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 3 },
                { PlayerStateEnum.Health, 10 }
            },
			sound: "吃_01");
    }
}