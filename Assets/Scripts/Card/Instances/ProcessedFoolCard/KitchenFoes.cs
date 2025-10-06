public class KitchenFoes : Card
{
    private KitchenFoes()
    {
        Events = new()
        {
            new Event("食用", "食用厨房恶物", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 13 },
                { PlayerStateEnum.San, -6 },
                { PlayerStateEnum.Health, -4 }
            })
        };
    }
}