public class CookedOysterMeat : CookableCard
{
    private CookedOysterMeat()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 3,
            () => new()
            {
                { PlayerStateEnum.Fullness, 8 },
                { PlayerStateEnum.Health, 1 },
                { PlayerStateEnum.San, 1 },
            })
        };
    }
}