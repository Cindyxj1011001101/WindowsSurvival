/// <summary>
/// 育卵液
/// </summary>
public class EggRearingFluid : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("饮用", "", (out string s) => EasyEvent(out s, "喝_01"), null,
            () => 3,
            () => new()
            {
                { PlayerStateEnum.Hydration, 40 },
                { PlayerStateEnum.Hunger, 10 },
            });
    }
}