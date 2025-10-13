/// <summary>
/// 压缩饼干
/// </summary>
public class CompactBiscuit : Card
{
    private CompactBiscuit()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 3,
            () => new()
            {
                { PlayerStateEnum.Fullness, 14 }
            })
        };
    }
}