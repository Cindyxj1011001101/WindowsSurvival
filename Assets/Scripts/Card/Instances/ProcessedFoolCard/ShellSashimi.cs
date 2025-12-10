/// <summary>
/// 贝类刺身
/// </summary>
[CardId("贝类刺身")]
public class ShellSashimi : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 5,
            () => new()
            {
                { PlayerStateEnum.Hunger, 54 },
                { PlayerStateEnum.Hydration, 14 },
                { PlayerStateEnum.Sanity, 13 },
                { PlayerStateEnum.Health, -3 }
            },
            sound: "吃_01");
    }
}