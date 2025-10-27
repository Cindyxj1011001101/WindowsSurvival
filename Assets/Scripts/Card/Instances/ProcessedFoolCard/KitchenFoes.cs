public class KitchenFoes : Card
{
    private KitchenFoes()
    {
        Events = new()
        {
            new CardEvent("食用", "食用厨房恶物", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 13 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -4 }
            })
        };
    }
}