/// <summary>
/// 小块生肉
/// </summary>
public class RawLittleMeat : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 12 },
                { PlayerStateEnum.Sanity, -2 },
                { PlayerStateEnum.Health, -3 }
            });
    }
}