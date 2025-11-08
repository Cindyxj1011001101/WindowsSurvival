/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "连瓶子也喝掉", EasyEvent_Destroy, null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hydration, 20 }
            },
            sound: "喝_01");
    }
}