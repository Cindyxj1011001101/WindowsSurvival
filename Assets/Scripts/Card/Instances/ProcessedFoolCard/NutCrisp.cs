/// <summary>
/// 坚果酥
/// </summary>
public class NutCrisp: Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 54 },
                { PlayerStateEnum.Health, 5 },
                { PlayerStateEnum.Sanity, 3 },
            });
    }
}   