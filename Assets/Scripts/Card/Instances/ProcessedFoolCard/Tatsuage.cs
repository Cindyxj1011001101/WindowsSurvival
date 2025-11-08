/// <summary>
/// 立鳞烧
/// </summary>
public class Tatsuage : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 75 },
                { PlayerStateEnum.Sanity, 25 },
                { PlayerStateEnum.Health, 30 }
            },
            sound: "吃_01");
    }
}