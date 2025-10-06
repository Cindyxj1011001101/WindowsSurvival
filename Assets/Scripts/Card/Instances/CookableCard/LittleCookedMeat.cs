public class LittleCookedMeat : CookableCard
{
    private LittleCookedMeat()
    {
        Events = new()
        {
            new Event("食用", "食用小块熟肉", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 18 },
                { PlayerStateEnum.Health, 1 },
            })
        };
    }
}