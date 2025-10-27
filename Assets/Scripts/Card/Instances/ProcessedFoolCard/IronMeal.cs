public class IronMeal : Card
{
    private IronMeal()
    {
        Events = new()
        {
            new CardEvent("食用", "食用铁齿铜牙餐", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 40 },
                { PlayerStateEnum.Sanity, -6 },
                { PlayerStateEnum.Health, -7 },
                { PlayerStateEnum.PainLevel, 50 }
            })
        };
    }
}   