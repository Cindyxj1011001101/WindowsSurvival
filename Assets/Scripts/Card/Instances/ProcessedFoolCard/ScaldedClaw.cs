/// <summary>
/// 白灼触手
/// </summary>
public class ScaldedClaw : Card
{
    private ScaldedClaw()
    {
        Events = new()
        {
            new Event("食用", "食用白灼触手", (out string s) => EasyEvent(out s, "吃_01"), null, () => 45,
            () => new()
            {
                { PlayerStateEnum.Fullness, 81 },
                { PlayerStateEnum.San, -3 }
            })
        };
    }
}