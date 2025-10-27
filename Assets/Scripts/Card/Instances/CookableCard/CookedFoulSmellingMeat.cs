/// <summary>
///  Ï∂Ò≥Ù»‚
/// </summary>
public class CookedFoulSmellingMeat : CookableCard
{
    private CookedFoulSmellingMeat()
    {
        Events = new()
        {
            new CardEvent(" ≥”√", "", (out string s) => EasyEvent(out s, "≥‘_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 20 },
                { PlayerStateEnum.Sanity, -15 },
                { PlayerStateEnum.Health, -10 }
            }),
        };
    }
}
