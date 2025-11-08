/// <summary>
/// 炸虫串
/// </summary>
public class FriedInsectStick : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 58 },
                { PlayerStateEnum.Hydration, -4 },
                { PlayerStateEnum.Sanity, 12 }
            },
            sound: "吃_01");
    }
}