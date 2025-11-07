/// <summary>
/// 炸虫串
/// </summary>
public class FriedInsectStick : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 58 },
                { PlayerStateEnum.Hydration, -4 },
                { PlayerStateEnum.Sanity, 12 }
            });
    }
}