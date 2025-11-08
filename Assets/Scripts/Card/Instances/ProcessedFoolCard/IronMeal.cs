/// <summary>
/// 铁齿铜牙餐
/// </summary>
public class IronMeal : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 40 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -7 },
                { PlayerStateEnum.PainLevel, 50 }
            },
            sound: "吃_01");
    }
}   