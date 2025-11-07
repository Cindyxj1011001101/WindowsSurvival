/// <summary>
/// 凉拌海蜇
/// </summary>
public class ColdJellyfishSalad : Card
{
    protected override void RegisterCardEvents()
    {
        AddCardEvent("食用", "", (out string s) => EasyEvent(out s, "吃_01"), null,
            () => 15,
            () => new()
            {
                { PlayerStateEnum.Hunger, 40 },
                { PlayerStateEnum.Hydration, 25 },
            });
    }
}   