/// <summary>
/// 贝类刺身
/// </summary>
public class ShellSashimi : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 5,
            () => new()
            {
                { PlayerStateEnum.Hunger, 54 },
                { PlayerStateEnum.Hydration, 14 },
                { PlayerStateEnum.Sanity, 13 },
                { PlayerStateEnum.Health, -3 }
            });
    }
}