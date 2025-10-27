/// <summary>
/// ∂Ò≥Ù»‚
/// </summary>
public class FoulSmellingMeat : CookableCard
{
    private FoulSmellingMeat()
    {
        Events = new()
        {
            new CardEvent(" ≥”√", "", (out string s) => EasyEvent(out s, "≥‘_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 14 },
                { PlayerStateEnum.Sanity, -20 },
                { PlayerStateEnum.Health, -15 }
            }),
        };
    }
}
