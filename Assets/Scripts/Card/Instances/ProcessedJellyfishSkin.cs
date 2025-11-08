/// <summary>
/// 已处理的海蜇皮
/// </summary>
public class ProcessedJellyfishSkin : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", EasyEvent_Destroy, null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Itchiness, +5 }
            },
            sound: "吃_01");
    }
}
