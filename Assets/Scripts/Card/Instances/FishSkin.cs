/// <summary>
/// 鱼皮
/// </summary>
public class FishSkin : Card
{
    private FishSkin()
    {
        Events = new()
        {
            new CardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null, () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 3 },
                { PlayerStateEnum.Health, 10 }
            })
        };
    }
}