public class FruitPudding: Card
{
    private FruitPudding()
    {
        Events = new()
        {
            new CardEvent("食用", "食用铁齿铜牙餐", (out string s) => EasyEvent(out s, "吃_01"), null, () => 15,
            () => new()
            {
                { PlayerStateEnum.Fullness, 25 },
                { PlayerStateEnum.Thirst, 25 },
                { PlayerStateEnum.San, 12 },
            })
        };
    }
}   