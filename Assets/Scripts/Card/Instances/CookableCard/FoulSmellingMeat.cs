/// <summary>
/// ∂Ò≥Ù»‚
/// </summary>
public class FoulSmellingMeat : CookableCard
{
    private FoulSmellingMeat()
    {
        Events = new()
        {
            new Event(" ≥”√", "", (out string s) => EasyEvent(out s, "≥‘_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 14 },
                { PlayerStateEnum.San, -20 },
                { PlayerStateEnum.Health, -15 }
            }),
        };
    }
}
