/// <summary>
/// 凉拌海蜇
/// </summary>
public class ColdJellyfishSalad : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 40 },
                { PlayerStateEnum.Hydration, 25 },
            },
            sound: "吃_01");
    }
}   