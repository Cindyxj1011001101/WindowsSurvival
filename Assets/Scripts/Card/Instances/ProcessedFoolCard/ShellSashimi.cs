/// <summary>
/// 贝类刺身
/// </summary>
public class ShellSashimi : Card
{
    private ShellSashimi()
    {
        Events = new()
        {
            new CardEvent("食用", "食用贝类刺身", (out string s) => EasyEvent(out s, "吃_01"), null, () => 5,
            () => new()
            {
                { PlayerStateEnum.Fullness, 54 },
                { PlayerStateEnum.Thirst, 14 },
                { PlayerStateEnum.San, 13 },
                { PlayerStateEnum.Health, -3 }
            })
        };
    }
}