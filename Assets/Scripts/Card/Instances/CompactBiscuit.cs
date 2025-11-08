/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 }
            },
            sound: "吃_01");
    }
}