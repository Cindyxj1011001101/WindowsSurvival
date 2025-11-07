/// <summary>
/// 铁齿铜牙餐
/// </summary>
public class FruitPudding: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 25 },
                { PlayerStateEnum.Hydration, 25 },
                { PlayerStateEnum.Sanity, 12 },
            });
    }
}   