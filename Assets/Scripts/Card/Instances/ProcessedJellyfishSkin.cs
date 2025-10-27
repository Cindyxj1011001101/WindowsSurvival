/// <summary>
/// 已处理的海蜇皮
/// </summary>
public class ProcessedJellyfishSkin : Card
{
    private ProcessedJellyfishSkin()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Itchiness, +5 }
            }),
        };
    }
}
