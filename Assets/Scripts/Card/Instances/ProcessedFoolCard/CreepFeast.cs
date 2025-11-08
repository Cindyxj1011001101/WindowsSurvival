/// <summary>
/// 蠕动盛宴
/// </summary>
public class CreepFeast: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 38 },
                { PlayerStateEnum.Sanity, -26 },
            },
            sound: "吃_01");
    }
}   