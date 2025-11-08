/// <summary>
/// 铁齿铜牙餐
/// </summary>
public class FruitPudding: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Hydration, 25 },
                { PlayerStateEnum.Sanity, 12 },
            },
            sound: "吃_01");
    }
}   