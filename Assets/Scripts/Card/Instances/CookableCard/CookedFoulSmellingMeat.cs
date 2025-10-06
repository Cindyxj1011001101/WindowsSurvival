/// <summary>
///  Ï∂Ò≥Ù»‚
/// </summary>
public class CookedFoulSmellingMeat : CookableCard
{
    private CookedFoulSmellingMeat()
    {
        Events = new()
        {
            new Event(" ≥”√", "", (out string s) => EasyEvent(out s, "≥‘_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 20 },
                { PlayerStateEnum.San, -15 },
                { PlayerStateEnum.Health, -10 }
            }),
        };
    }
}
