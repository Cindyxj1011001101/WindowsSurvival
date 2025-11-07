/// <summary>
/// 肉排
/// </summary>
public class Steak : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 72 },
                { PlayerStateEnum.Health, 8 }
            });
    }
}