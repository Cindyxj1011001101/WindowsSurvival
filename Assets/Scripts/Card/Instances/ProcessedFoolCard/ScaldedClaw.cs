/// <summary>
/// 白灼触手
/// </summary>
public class ScaldedClaw : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 45,
            () => new()
            {
                { PlayerStateEnum.Hunger, 81 },
                { PlayerStateEnum.Sanity, -3 }
            },
            sound: "吃_01");
    }
}