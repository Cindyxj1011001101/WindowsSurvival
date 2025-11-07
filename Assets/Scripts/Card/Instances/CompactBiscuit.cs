/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 }
            });
    }
}