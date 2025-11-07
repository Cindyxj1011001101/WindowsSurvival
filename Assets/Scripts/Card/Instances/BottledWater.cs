/// <summary>
/// 瓶装水
/// </summary>
public class BottledWater : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "连瓶子也喝掉", (out string s) => EasyEvent(out s, "喝_01"), null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hydration, 20 }
            });
    }
}