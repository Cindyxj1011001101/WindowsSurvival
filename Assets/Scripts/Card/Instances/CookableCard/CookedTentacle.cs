/// <summary>
/// 熟触手
/// </summary>
public class CookedTentacle : CookableCard
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 30,
            () => new()
            {
                { PlayerStateEnum.Hunger, 24 },
                { PlayerStateEnum.Sanity, -1 },
            });
    }
}