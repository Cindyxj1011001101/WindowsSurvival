public class IronMeal : Card
{
    private IronMeal()
    {
        Events = new()
        {
            new Event("食用", "食用铁齿铜牙餐", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Fullness, 40 },
                { PlayerStateEnum.San, -6 },
                { PlayerStateEnum.Health, -7 },
                { PlayerStateEnum.PainLevel, 50 }
            })
        };
    }
}   