public class CookedOysterMeat : CookableCard
{
    private CookedOysterMeat()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 3,
            () => new()
            {
                { PlayerStateEnum.Hunger, 8 },
                { PlayerStateEnum.Health, 1 },
                { PlayerStateEnum.Sanity, 1 },
            })
        };
    }
}